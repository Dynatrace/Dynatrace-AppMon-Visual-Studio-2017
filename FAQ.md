# FAQ / Troubleshooting Guide

## Problems? Questions? Suggestions?

Post any problems, questions or suggestions to the Dynatrace Community's [Application Monitoring & UEM Forum](https://answers.dynatrace.com/spaces/146/index.html).
 
## Web Project - Known Issues and Limitations

* "External program" and "Don't open a page" start actions are not supported
* The web application will always use IIS Express when started with the Dynatrace Launcher, even when another server is configured in the project settings
* Default IIS Express application pool is used ("Clr4IntegratedAppPool")
* Only default site name supported (named after the project)
* Always launching IE browser
* Two agents with the same name will connect to the Dynatrace Server: IIS Express and the IIS Express Tray which is a child process of IIS Express. Since the child process inherits environment variables from parents, the agent is also injected to that Tray application.

## Test Project

test projects are not yet supported.

To capture PurePaths and Test Automation data for Unit Tests run from Visual Studio, you need to instrument the process **TE.ProcessHost.Managed.exe** using the <a href="https://community.dynatrace.com/community/display/DOCDT63/.NET+Agent+Configuration" target="_blank">.NET Agent Configuration Tool</a>.

## Log files

The log files are available under: C:\Users\<USERNAME>\AppData\Roaming\dynaTrace\CodeLinkNet\log
