
# SiteServiceMonitor

SiteServiceMonitor is a C# .NET Windows Service that monitors website URLs, server names/IPs, and Windows services on those servers. It logs all status checks and sends Windows notifications if a problem is detected.

## Features
- Monitor website URLs for HTTP response.
- Monitor server names or IPs for reachability (ping).
- Monitor Windows service status on specified servers.
- Logs all checks to `monitorlog.txt`.
- Sends Windows notifications for errors/problems.

## Requirements
- .NET 8.0 SDK or later
- Windows OS (for service checks and notifications)

## Setup Instructions
1. **Clone or download this repository.**
2. **Edit `monitorlist.json`** to add the websites, servers, and services you want to monitor. Example:
   ```json
   {
     "WebsiteUrls": [
       "localhost",
       "localhost:5000"
     ],
     "ServerNamesOrIps": [
       "localhost"
     ],
     "Services": [
       { "Server": "localhost", "Service": "Spooler" },
       { "Server": "localhost", "Service": "W3SVC" }
     ]
   }
   ```
3. **Build and publish the service:**
   ```powershell
   dotnet publish -c Release -o publish
   ```
4. **Install the service:**
   Open an **elevated** PowerShell or Command Prompt and run:
   ```powershell
   sc create SiteServiceMonitor binPath= "C:\Path\To\publish\SiteServiceMonitor.exe"
   ```
   Replace the path with your actual publish directory.
5. **Start the service:**
   ```powershell
   sc start SiteServiceMonitor
   ```

## Configuration
- Edit `monitorlist.json` to add or remove items to monitor. Changes will be picked up automatically on the next check.
- All logs are written to `monitorlog.txt` in the same directory as the executable.
- Windows notifications will appear if a website, server, or service is unreachable or in error.

## Uninstalling the Service
To stop and remove the service:
```powershell
sc stop SiteServiceMonitor
sc delete SiteServiceMonitor
```

## Notes
- This application runs as a Windows Service and does not have a user interface.
- For best results, ensure the service has permission to read/write in its directory.
- You can use Task Scheduler as an alternative if you do not want to run as a service.
