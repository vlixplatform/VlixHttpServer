Vlix Http Server
================

<img align="left" width="64" height="64" src="https://cdn.vlix.me/vlixicon-128x128.png">

<br />**Vlix Http Server** is a simple and high performance http server used to serve static file content. This means any directory on your PC can be turned into a web server to serve files such as 'html','png','jpeg','avi' and etc. Although very lightweight, Vlix Http Server is  a high performance server. Vlix:

- Works Multi threaded.

- Has content caching.

- Prevents illegal directory lookups, such as http://myfolder/../mysecret.txt

- Logs all calls (Default log directory is *C:\ProgramData\Vlix\HttpServer\Logs*)

  

<br />These features allow Vlix Http Server to serve **thousands of requests** with ease, without latency.

> The Vlix Http Server is part of **Vlix** (http://vlix.me). **Vlix** overall is an Industrial Data Platform which uses this Http Server to serve content from it's back end. It's therefore industrial grade! 😃



<br />Vlix Http Server targets the **DotNet Framework**. Unlike DotNet Core, the DotNet Framework lacks a dedicated high performance web server like Kestrel.



## Embedding in your project

Install the [VlixHttpServer Package](https://www.nuget.org/packages/VlixHttpServer/) via Nuget:

```powershell
Install-Package VlixHttpServer
```

In your project, to create a Http Server, simply use:

```c#
VlixHttpServer vlixHttpServer = new VlixHttpServer("C:\\YourDirectory",8080);
vlixHttpServer.Start();
```

You can also handle logs and exceptions by via delegates. Example:

```C#
VlixHttpServer vlixHttpServer = new VlixHttpServer("C:\\YourDirectory", 8080);
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

To install Vlix Http Server as a windows service, simply run the MSI Installer which can be obtained from

https://github.com/vlixplatform/VlixHttpServer/releases<br />

<img align="left" height="300" src="https://cdn.vlix.me/res/install.png">

<br />

After installation, Vlix Http Server should be seen running as a windows service:<br />

<img align="left" width="auto" height="auto" src="https://cdn.vlix.me/res/httpserverservice.png">

<br />

The default port for the server is *8080* and the default directory is *C:\ProgramData\Vlix\HttpServer\www*. To use a different port and directory, edit the config found in the installation directory:<br />

**C:\Program Files\Vlix\HttpServer\VlixHttpServerService.exe.config**<br />

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="Port" value="8080" />
    <add key="EnableCache" value="True" />
    <add key="LogDirectory" value="[ProgramDataDirectory]\Vlix\HttpServer\Logs" />
    <add key="WWWDirectory" value="[ProgramDataDirectory]\Vlix\HttpServer\www" />
  </appSettings>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2" />
    </startup>
</configuration>
```

<br />**Note: Using ''[ProgramDataDirectory]' will map to *C:\ProgramData*

<br />Make sure to **restart the service** after updating this file.



## How caching works

For every first caller that makes a HTTP request to a file, Vlix Http Server will read and store the file in memory for a duration of 1 minute. Within this duration, any consecutive calls to the file will not cause another file read, but will read from memory instead. After 1 minute, the file is removed from memory, to allow the next caller to re-read from file and into memory. With this, any updates will only take effect after the minute.



## Licence

MIT
