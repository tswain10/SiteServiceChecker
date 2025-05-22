using System;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace SiteServiceMonitor
{
    class Program
    {
        static List<string> websiteUrls = new List<string>();
        static List<string> serverNamesOrIps = new List<string>();
        static List<(string server, string service)> services = new List<(string, string)>();

        static async Task Main(string[] args)
        {
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
                        if (!string.IsNullOrWhiteSpace(url)) websiteUrls.Add(url);
                        break;
                    case "2":
                        Console.Write("Enter server name or IP: ");
                        var server = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(server)) serverNamesOrIps.Add(server);
                        break;
                    case "3":
                        Console.Write("Enter server name or IP: ");
                        var svcServer = Console.ReadLine();
                        Console.Write("Enter service name: ");
                        var svcName = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(svcServer) && !string.IsNullOrWhiteSpace(svcName))
                            services.Add((svcServer, svcName));
                        break;
                    case "4":
                        await CheckStatus();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static async Task CheckStatus()
        {
            Console.WriteLine("\n--- Website Status ---");
            using (var httpClient = new HttpClient())
            {
                foreach (var url in websiteUrls)
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
            foreach (var server in serverNamesOrIps)
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
            foreach (var (server, service) in services)
            {
                try
                {
                    ServiceController sc = new ServiceController(service, server);
                    Console.WriteLine($"{service} on {server}: {sc.Status}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{service} on {server}: Error ({ex.Message})");
                }
            }
        }
    }
}
