Vlix Http Server
================

<img align="left" width="64" height="64" src="https://cdn.vlix.me/vlixicon-128x128.png"><br><br>
  
**Vlix Http Server** is a simple and high performance http server used to serve static file content. This means any directory on your PC can be turned into a web server to serve web documents such as HTML files. Other files include as 'png','jpeg','avi','mp4' and etc. Although very lightweight, Vlix Http Server is high performance, as it:

- Works multi threaded.

- Has content caching.

- Prevents illegal directory lookups, such as http://myfolder/../mysecret.txt

- Logs all calls (Default log directory is *C:\ProgramData\Vlix\HttpServer\Logs*)

  

<br />Vlix Http Server can serve **thousands of requests** with ease, without latency.

> The Vlix Http Server is part of **Vlix** (http://vlix.me). **Vlix** overall is an Industrial Data Platform which uses this Http Server to serve content from it's back end. It's therefore industrial grade! 😃



<br />Vlix Http Server targets the **DotNet 4.5.2 and DotNet Standard 2.0** framework.



## Embedding in your project / Consuming the DLL

Install the [VlixHttpServer Package](https://www.nuget.org/packages/VlixHttpServer/) via Nuget:

```powershell
Install-Package VlixHttpServer
```

In your project, to create a Http Server, simply use:

```c#
VlixHttpServer vlixHttpServer = new VlixHttpServer("C:\\YourWWWDirectory",80,443);
vlixHttpServer.Start();
```

You can also handle logs and exceptions via delegates. Example:

```C#
VlixHttpServer vlixHttpServer = new VlixHttpServer("C:\\YourWWWDirectory", 80, 443);
vlixHttpServer.OnError = (ex) => Log.Error(ex.ToString());
vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
vlixHttpServer.Start();
```

To stop the server simply call:

```C#
vlixHttpServer.Stop();
```



## Installing as a Service

To install Vlix Http Server as a windows service,
simply run the MSI Installer which can be obtained from

https://github.com/vlixplatform/VlixHttpServer/releases<br />
<img align="left" height="300" src="https://cdn.vlix.me/res/install.png">
<br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br /><br />
Ater installation, Vlix Http Server should be seen running as a windows service:
<div>
<img align="left" width="auto" height="auto" src="https://cdn.vlix.me/res/httpserverservice.png">
</div>
<br /><br /><br />

The default port for the server is *80 for Http* and *443 for Https* and the default directory is *C:\ProgramData\Vlix\HttpServer\www*. To use a different port and directory, edit the config found in the installation directory:<br />

**C:\Program Files\Vlix\HttpServer\VlixHttpServerService.exe.config**<br />

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="HTTPPort" value="80"/>
    <add key="HTTPSPort" value="443"/>
    <add key="EnableCache" value="True"/>
    <add key="OnlyCacheItemsLessThenMB" value="10"/>
    <add key="MaximumCacheSizeInMB" value="250"/>
    <add key="LogDirectory" value="[ProgramDataDirectory]\Vlix\HTTPServer\Logs"/>
    <add key="WWWDirectory" value="[ProgramDataDirectory]\Vlix\HTTPServer\www"/>
    <add key="AllowLocalhostConnectionsOnlyForHttp" value="False"/>
  </appSettings>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2" />
    </startup>
</configuration>
```

<br />**Note: Using ''[ProgramDataDirectory]' will map to *C:\ProgramData*

<br />Make sure to **restart the service** after updating this file.



## How caching works

For every first caller that makes a HTTP request to a file, Vlix Http Server will read and store the file in memory. Any consecutive calls to the file will not cause another file read, but will read from the memory cache. The read also verifies if the file's timestamp has changed. If it has, the cache will be refreshed. 

If the cache size (memory usage) exceeds the "MaximumCacheSizeInMB", the files with the least access will be removed. This ensures that the Maximum Cache size will be within the limit. The default size of this cache is 250 MB.



## Licence

MIT
