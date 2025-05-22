using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading.Tasks;

namespace SiteServiceMonitor
{
    class Program
    {

        static string dataFile = "monitorlist.json";

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

        static async Task Main(string[] args)
        {
            LoadMonitorData();
            while (true)
            {
                Console.WriteLine("\nSite/Service Monitor");
                Console.WriteLine("1. Add website URL");
                Console.WriteLine("2. Add server name or IP");
                Console.WriteLine("3. Add service on server");
                Console.WriteLine("4. Check status");
                Console.WriteLine("5. Exit");
                Console.Write("Select an option: ");
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.Write("Enter website URL: ");
                        var url = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(url)) monitorData.WebsiteUrls.Add(url);
                        SaveMonitorData();
                        break;
                    case "2":
                        Console.Write("Enter server name or IP: ");
                        var server = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(server)) monitorData.ServerNamesOrIps.Add(server);
                        SaveMonitorData();
                        break;
                    case "3":
                        Console.Write("Enter server name or IP: ");
                        var svcServer = Console.ReadLine();
                        Console.Write("Enter service name: ");
                        var svcName = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(svcServer) && !string.IsNullOrWhiteSpace(svcName))
                            monitorData.Services.Add(new ServiceEntry { Server = svcServer, Service = svcName });
                        SaveMonitorData();
                        break;
                    case "4":
                        await CheckStatus();
                        break;
                    case "5":
                        SaveMonitorData();
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
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

        static void SaveMonitorData()
        {
            var json = JsonSerializer.Serialize(monitorData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dataFile, json);
        }

        static async Task CheckStatus()
        {
            Console.WriteLine("\n--- Website Status ---");
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
                        Console.WriteLine($"{url}: {(response.IsSuccessStatusCode ? "OK" : $"Error ({response.StatusCode})")}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{url}: Error ({ex.Message})");
                    }
                }
            }

            Console.WriteLine("\n--- Server Status (Ping) ---");
            foreach (var server in monitorData.ServerNamesOrIps)
            {
                try
                {
                    var ping = new System.Net.NetworkInformation.Ping();
                    var reply = ping.Send(server, 2000);
                    Console.WriteLine($"{server}: {(reply.Status == System.Net.NetworkInformation.IPStatus.Success ? "Reachable" : "Unreachable")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{server}: Error ({ex.Message})");
                }
            }

            Console.WriteLine("\n--- Service Status ---");
            foreach (var entry in monitorData.Services)
            {
                try
                {
                    ServiceController sc = new ServiceController(entry.Service, entry.Server);
                    Console.WriteLine($"{entry.Service} on {entry.Server}: {sc.Status}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{entry.Service} on {entry.Server}: Error ({ex.Message})");
                }
            }
        }
    }
}
