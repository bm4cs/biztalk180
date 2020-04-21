# BizTalk 180

A simpler and better 360, re-imagined as a small Windows Service.

Boilerplate windows service based on https://github.com/HarpyWar/windows-service-template, which includes:

* The ability to run your program in both console or service mode
* Self installer/uninstaller using command line args `/install` and `/uninstall` (shorten `/i` and `/u`)
* [Formo](https://github.com/ChrisMissal/Formo) for easy reading `App.settings` properties
* [NLog](https://github.com/NLog/NLog) for logging


## Developer notes

`Service.cs` is the windows service (`ServiceBase`) entry point and hooks usual events such as `Start`, `Stop` and `Shutdown`.

`BizTalkMonitorService.cs` runs on a separate thread, and is responsible for monitoring BizTalk Server things.

In the `Installer.cs` you can handle `Before` and `After` installation events.

In the `AppSettings.cs` you can define own properties that should conform to properties in `App.config` of section `appSettings`.

Define `Logger` variable in each class where you need logging. It's needed to keep correct caller class name in each log line:

```cs
private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
```

Then use `Logger.Debug()`, `Logger.Info()`, `Logger.Error()`, etc.

I added `OutputDebugString` as a log output so we can use DebugView (available in C:\Tools).


## Installation


Administrator privileges are required to install/uninstall a service. UAC execution level of the application is defined in `app.manifest`. Switch comment block there if you need to disable required administrator rights.
```xml
<!--<requestedExecutionLevel level="asInvoker" uiAccess="false" />-->
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
```
By default a service will be installed under account `LocalSystem`. You can change it in `Installer.cs` (`serviceProcessInstaller1.Account` property from a designer or in code).

Before install/uninstall you can change `DisplayName`, `ServiceName` and `Description` in `Installer.cs`.

The [`ServiceName`](https://msdn.microsoft.com/en-us/library/system.serviceprocess.serviceinstaller.servicename(v=vs.110).aspx) cannot be null or have zero length. Its maximum size is 256 characters. It also cannot contain forward or backward slashes, '/' or '\', or characters from the ASCII character set with value less than decimal value 32.


