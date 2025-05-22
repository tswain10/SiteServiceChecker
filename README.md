# SiteServiceMonitor

A C# .NET console application to monitor website URLs, server names/IPs, and services on those servers. The application allows you to add items to a list and checks the response from websites and whether specified services are running.

## Features
- Add website URLs to monitor their HTTP response.
- Add server names or IPs and check if they are reachable.
- Add Windows service names to check if they are running on specified servers.

## Usage
1. Run the application.
2. Use the menu to add websites, servers, or services to the monitoring list.
3. The application will periodically check the status and display results.

## Requirements
- .NET 8.0 SDK or later
- Windows OS (for service checks)

## Build and Run
```powershell
dotnet build
dotnet run
```
