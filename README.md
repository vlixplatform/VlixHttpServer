Vlix Http Server
================

**Vlix Http Server** is a simple yet production ready http server used to serve static file content. This means any directory on your PC can be turned into a web server to serve files such as 'html'. Even though it is very lightweight Vlix Http Server is  production ready, as it supports:

- Multi threaded request processing.

- Content Caching and management.

- Prevents parent directory look ups, such as http://myfolder/../mysecret.txt

- Logging

  

These features allow the Vlix Http Server to serve **thousands of requests** with ease, without latency.

> The Vlix Http Server is part of **Vlix** (http://vlix.me). **Vlix** overall is an Industrial Data Platform which uses this Http Server to serve content from it's back end. 😃



Vlix Http Server targets the **DotNet Framework**, which lacks a dedicated web server like Kestrel for DotNet Core.



## Embedding it in your project

Install the [VlixHttpServer Package](https://www.nuget.org/packages/VlixHttpServer/) via Nuget:

```powershell
Install-Package VlixHttpServer
```

In your project, to create a Http Server simply use:

```c#
VlixHttpServer vlixHttpServer = new VlixHttpServer("C:\\YourDirectory",8080);
vlixHttpServer.Start();
```

You can catch also handle logs and exceptions. Example:

```C#
VlixHttpServer vlixHttpServer = new VlixHttpServer("C:\\YourDirectory", 8080);
vlixHttpServer.OnError = (ex) => Log.Error(ex.ToString());
vlixHttpServer.OnInfoLog = (log) => Log.Information(log);
vlixHttpServer.OnWarningLog = (log) => Log.Warning(log);
vlixHttpServer.Start();
```

To stop the server simply call

```C#
vlixHttpServer.Stop();
```



## Installing as a Service

For every first caller that makes a HTTP request to a file



## How caching works

For every first caller that makes a HTTP request to a file, Vlix Http Server will read and store the file in memory for a duration of 1 minute. Within this duration, any consecutive calls to the file will not cause another file read but will read from memory instead. After 1 minute, the file is removed from memory, to allow the next caller to read from file in case the file was updated.



## Licence

MIT
