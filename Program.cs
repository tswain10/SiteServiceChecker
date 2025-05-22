using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SiteServiceMonitor
{
    public class Program
    {
        static string dataFile = "monitorlist.json";
        static string logFile = "monitorlog.txt";
        static Timer? monitorTimer;
        static int monitorIntervalMs = 60000; // 60 seconds

        class MonitorData
        {
            public List<string> WebsiteUrls { get; set; } = new List<string>();
            public List<string> ServerNamesOrIps { get; set; } = new List<string>();
            public List<ServiceEntry> Services { get; set; } = new List<ServiceEntry>();
        }

        class ServiceEntry
        {
            public string Server { get; set; } = string.Empty;
            public string Service { get; set; } = string.Empty;
        }

        static MonitorData monitorData = new MonitorData();

        public static void Main(string[] args)
        {
            LoadMonitorData();
            ServiceBase.Run(new MonitorService());
        }

        public class MonitorService : ServiceBase
        {
            public MonitorService()
            {
                this.ServiceName = "SiteServiceMonitor";
                this.CanStop = true;
                this.CanPauseAndContinue = false;
                this.AutoLog = true;
            }

            protected override void OnStart(string[] args)
            {
                Log("Service started.");
                monitorTimer = new Timer(async _ => await CheckStatus(), null, 0, monitorIntervalMs);
            }

            protected override void OnStop()
            {
                Log("Service stopped.");
                monitorTimer?.Dispose();
            }
        }

        static void LoadMonitorData()
        {
            if (File.Exists(dataFile))
            {
                try
                {
                    var json = File.ReadAllText(dataFile);
                    monitorData = JsonSerializer.Deserialize<MonitorData>(json) ?? new MonitorData();
                }
                catch
                {
                    monitorData = new MonitorData();
                }
            }
        }

        static void Log(string message, bool isError = false)
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(logFile, logEntry);
            if (isError)
            {
                try
                {
                    ShowWindowsNotification("SiteServiceMonitor Alert", message);
                }
                catch { /* Notification failed, ignore */ }
            }
        }

        static void ShowWindowsNotification(string title, string message)
        {
            // Use Windows Toast Notification via PowerShell as a fallback for Windows Service
            try
            {
                string escapedTitle = title.Replace("'", "''");
                string escapedMessage = message.Replace("'", "''");
                string psCommand = $@"
                    [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] > $null
                    $template = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent([Windows.UI.Notifications.ToastTemplateType]::ToastText02)
                    $textNodes = $template.GetElementsByTagName('text')
                    $textNodes.Item(0).AppendChild($template.CreateTextNode('{escapedTitle}')) > $null
                    $textNodes.Item(1).AppendChild($template.CreateTextNode('{escapedMessage}')) > $null
                    $toast = [Windows.UI.Notifications.ToastNotification]::new($template)
                    $notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('SiteServiceMonitor')
                    $notifier.Show($toast)
                ";
                var psi = new System.Diagnostics.ProcessStartInfo("powershell.exe", $"-NoProfile -WindowStyle Hidden -Command \"{psCommand}\"");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                System.Diagnostics.Process.Start(psi);
            }
            catch { /* Notification failed, ignore */ }
        }

        static async Task CheckStatus()
        {
            using (var httpClient = new HttpClient())
            {
                foreach (var url in monitorData.WebsiteUrls)
                {
                    string checkedUrl = url;
                    if (!checkedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !checkedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        checkedUrl = "http://" + checkedUrl;
                    }
                    try
                    {
                        var response = await httpClient.GetAsync(checkedUrl);
                        if (response.IsSuccessStatusCode)
                        {
                        Log($"Website OK: {url}");
                    }
                    else
                    {
                        Log($"Website ERROR: {url} (Status: {response.StatusCode})", true);
                    }
                }
                catch (Exception ex)
                {
                    Log($"Website ERROR: {url} ({ex.Message})", true);
                }
                }
            }

            foreach (var server in monitorData.ServerNamesOrIps)
            {
                try
                {
                    var ping = new System.Net.NetworkInformation.Ping();
                    var reply = ping.Send(server, 2000);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        Log($"Server OK: {server}");
                    }
                    else
                    {
                        Log($"Server ERROR: {server} (Unreachable)", true);
                    }
                }
                catch (Exception ex)
                {
                    Log($"Server ERROR: {server} ({ex.Message})", true);
                }
            }

            foreach (var entry in monitorData.Services)
            {
                try
                {
                    ServiceController sc = new ServiceController(entry.Service, entry.Server);
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        Log($"Service OK: {entry.Service} on {entry.Server}");
                    }
                    else
                    {
                        Log($"Service ERROR: {entry.Service} on {entry.Server} (Status: {sc.Status})", true);
                    }
                }
                catch (Exception ex)
                {
                    Log($"Service ERROR: {entry.Service} on {entry.Server} ({ex.Message})", true);
                }
            }
        }
    }
}
