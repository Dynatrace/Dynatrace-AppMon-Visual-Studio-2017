# FAQ / Troubleshooting Guide

## Problems? Questions? Suggestions?

Post any problems, questions or suggestions to the Dynatrace Community's [Application Monitoring & UEM Forum](https://answers.dynatrace.com/spaces/146/index.html).
 
## Web Project - Known Issues and Limitations

* "External program" and "Don't open a page" start actions are not supported
* The web application will always use IIS Express when started with the Dynatrace Launcher, even when another server is configured in the project settings
* Only default site name supported (named after the project)
* Two agents with the same name will connect to the Dynatrace Server: IIS Express and the IIS Express Tray which is a child process of IIS Express. Since the child process inherits environment variables from parents, the agent is also injected to that Tray application.

## Test Project

test projects are not yet supported.

To capture PurePaths and Test Automation data for Unit Tests run from Visual Studio, you need to instrument the process **TE.ProcessHost.Managed.exe** using the <a href="https://community.dynatrace.com/community/display/DOCDT63/.NET+Agent+Configuration" target="_blank">.NET Agent Configuration Tool</a>.

## Log files

The log files are available under: C:\Users\<USERNAME>\AppData\Roaming\dynaTrace\CodeLinkNet\log

## Project doesn't compile in Visual Studio
Please ensure you have [Visual Studio SDK](https://docs.microsoft.com/en-us/visualstudio/extensibility/installing-the-visual-studio-sdk) installed for your version.

If OleMenuCommandService or Package classes are not found, replace the reference to Microsoft.VisualStudio.Shell.**12**.0, with one located in *VisualStudioInstallationDir*\Common7\IDE\PrivateAssemblies\Microsoft.VisualStudio.Shell.**15**.0.dll (select ...**15**.0.dll for VS 2017, the highest one available in the directory for others).
You can do this by:
1. right-clicking the References node under DynatraceAppMon
2. selecting "Add Reference..."
3. selecting "Browse..."
4. navigating to and selecting *VisualStudioInstallationDir*\Common7\IDE\PrivateAssemblies\Microsoft.VisualStudio.Shell.**15**.0.dll
5. removing the Microsoft.VisualStudio.Shell.12.0 Reference

![Compilation problem screenshot](https://github.com/Dynatrace/Dynatrace-Visual-Studio-2017/raw/master/img/faq/compilation_problem.png)