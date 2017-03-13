<img src="/img/logo/visual-studio.png" width="300" />

# Dynatrace AppMon Visual Studio 2017 Extension

The Dynatrace Visual Studio Extension enable you to launch applications with an injected Dynatrace AppMon Agent directly from Visual Studio and look up the source code from applications under diagnosis in Dynatrace AppMon.

* Download the latest version from the [Visual Studio Marketplace](https://visualstudiogallery.msdn.microsoft.com/77c28a92-9bbe-46a9-b206-98301d4ecd3b)
* The Visual Studio Extension on the [Dynatrace Community](https://community.dynatrace.com/community/display/DL/Dynatrace+AppMon+Visual+Studio+Extension)

#### Table of Contents

* [Installation](#installation)  
 * [Prerequisites](#prerequisites)  
 * [Installation](#installation)
* [Configuration](#configuration)
* [Use the Visual Studio Extension](#use)
 * [Launcher](#launcher)
 * [Source Code Look-up](#source_code)
*  [Problems? Questions? Suggestions?](#feedback)


<a name="installation"/>
## Installation

<a name="prerequisites"/>
### Prerequisites

* Visual Studio Version: 2017 (all editions supported), starting with plugin version 6.5.3 Visual Studio 2013 and 2015 are also supported 
* Dynatrace AppMon Server 6.3+
* Dynatrace AppMon .NET agent installed on your machine (to run/test your application with the .NET agent)
* Dynatrace AppMon Client running on your machine (for CodeLink) 

<a name="installation"/>
### Installation

* In Visual Studio open Tools -> Extensions and Updates...
* Select "Online" and use the "Search Visual Studion Gallery" search box
* Search for Dynatrace AppMon and follow the instructions to install the extension

<a name="configuration"/>
## Configuration

First you will need to configure the extension. Open the settings dialog from Tools -> Dynatrace AppMon Extension Configuration and enter the details about the Collector you would like the .NET agent to connect to. If you want to use CodeLink you have to make sure that the REST API in your AppMon Client is enabled. 

The settings will be stored in your Visual Studio solution, so you can maintain different configurations for different solutions.

![configuration](/img/conf/configuration_2.jpg) 

<a name="use"/>
## Use the Visual Studio Extension

<a name="launcher"/>
### Launcher

The lancher will run applications with an injected Dynatrace .NET agent using the agent name and additional parameters defined in the run configuration. The agent will output debug information into the console. The launcher supports Windows and Web based projects and tests.

![edit run configurations](/img/use/launcher.png) 

<a name="source_code"/>
### Source Code Look-up

The AppMon client lets you analyze PurePaths down to the individual methods that have been instrumented in the context of the captured transaction. When you identify a problematic method either in the PurePath view or in the Methods view of the AppMon client, you can use the CodeLink functionality to jump to the source code line in the open Visual Studio solution.

![edit run configurations](/img/use/source_code_lookup.png) 

<a name="feedback"/>
## Problems? Questions? Suggestions?

* [Visual Studio Extension FAQ / Troubleshooting Guide](FAQ.md)
* Post any problems, questions or suggestions to the Dynatrace Community's [Application Monitoring & UEM Forum](https://answers.dynatrace.com/spaces/146/index.html).
