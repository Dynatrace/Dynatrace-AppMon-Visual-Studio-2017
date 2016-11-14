<img src="/img/logo/visual-studio.png" width="300" />

# Dynatrace AppMon Visual Studio 2017 Extension

The Dynatrace Visual Studio Extension enable you to launch applications with an injected Dynatrace AppMon Agent directly from Visual Studio and look up the source code from applications under diagnosis in Dynatrace AppMon.

* Download the latest version from the [Visual Studio Marketplace](https://visualstudiogallery.msdn.microsoft.com/77c28a92-9bbe-46a9-b206-98301d4ecd3b)
* The Visual Studio Extension on the [Dynatrace Community](https://community.dynatrace.com/community/display/DL/Dynatrace+AppMon+Visual+Studio+Extension)

#### Table of Contents

* [Installation](#installation)  
 * [Prerequisites](#prerequisites)  
 * [Manual Installation](#manual)
* [Configuration](#configuration)
* [Use the Visual Studio Extension](#use)
 * [Launcher](#launcher)
 * [Source Code Look-up](#source_code)
*  [Problems? Questions? Suggestions?](#feedback)


<a name="installation"/>
## Installation

<a name="prerequisites"/>
### Prerequisites

* Dynatrace Application Monitoring Version: 6.3 and above
* Visual Studio Version: 2017 (all editions supported)

Find further information in the [Dynatrace community](https://community.dynatrace.com/community/display/DL/Dynatrace+AppMon+Visual+Studio+Extension)

<a name="manual"/>
### Manual Installation

* Download the [latest release](https://github.com/Dynatrace/Dynatrace-Visual-Studio-2017/releases/latest) from Github
* Double-click on the VSIX file
* Follow the steps displayed by the Visual Studio Extension Installer
* Restart Visual Studio

<a name="configuration"/>
## Configuration

The settings for the extension are located under Tools / Dynatrace AppMon Extension Configuration:

![configuration](/img/conf/configuration_1.png) 

Configuration screen:

![configuration](/img/conf/configuration_2.jpg) 

<a name="use"/>
## Use the Visual Studio Extension

<a name="launcher"/>
### Launcher

The lancher will run applications with an injected Dynatrace Agent using the agent name and additional parameters defined in the run configuration. The agent will output debug information into the console.

![edit run configurations](/img/use/launcher.png) 


The launcher supports Windows and Web based project; Test projects are not yet supported.


<a name="source_code"/>
### Source Code Look-up

The Dynatrace Client enables you to analyze PurePaths down to the individual methods that have been instrumented in the context of the captured transaction. When you identify a problematic method either in the PurePath view or in the Methods view of the Dynatrace Client, you can use the Source Code Lookup to jump to the source code line if you have the Visual Studio solution open.

![edit run configurations](/img/use/source_code_lookup.png) 

<a name="feedback"/>
## Problems? Questions? Suggestions?

* [Visual Studio Extension FAQ / Troubleshooting Guide](FAQ.md)
* Post any problems, questions or suggestions to the Dynatrace Community's [Application Monitoring & UEM Forum](https://answers.dynatrace.com/spaces/146/index.html).
