using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using System.Reflection;
using DynaTrace.CodeLink;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;

namespace FirstPackage
{
    public class Launcher
    {

        private string UNEXPECTED_DEFAULT_PROPERTY_VALUE = "dynaTrace_unexpected";
        private System.Diagnostics.Process lastRunningIIS = null;
        private DTE2 _applicationObject;
        private Context context;

        private enum LaunchType { Assembly, WebApplication, WebSite };
        /// <summary>
        /// web site project GUID refers to project type
        /// </summary>
        private readonly string WEB_SITE_PROJECT_TYPE_GUID = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";
        /// <summary>
        /// web application project GUID refers to project subtype, project type GUID in this case describes language thus
        /// may be omitted in launch type check
        /// </summary>
        private readonly string WEB_APPLICATION_PROJECT_SUBTYPE_GUID = "{349C5851-65DF-11DA-9384-00065B846F21}";

        public bool BuildBeforeLaunch
        {
            get
            {
                try { return Boolean.Parse(GetGlobalProperty("BuildBeforeLaunch", true).ToString()); }
                catch { return true; }
            }
            set { SetGlobalProperty("BuildBeforeLaunch", value); }
        }

        public string AgentGuid
        {
            get { return (string)GetGlobalProperty("AgentGuid", "{DA7CFC47-3E35-4c4e-B495-534F93B28683}"); } // 4.1.0
            set { SetGlobalProperty("AgentGuid", value); }
        }

        public string CustomAgentName
        {
            get { return (string)GetGlobalProperty("CustomAgentName", null); }
            set { SetGlobalProperty("CustomAgentName", value); }
        }

        public int WaitForBrowserTime
        {
            get { try { return Int32.Parse(GetGlobalProperty("WaitForBrowserTime", (Int32)3000).ToString()); } catch { return 3000; } }
            set { SetGlobalProperty("WaitForBrowserTime", value); }
        }


        public string ServerName
        {
            get { return (string)GetGlobalProperty("ServerName", "localhost"); }
            set { SetGlobalProperty("ServerName", value); }
        }

        public int ServerPort
        {
            get { return Int32.Parse(GetGlobalProperty("ServerPort", (int)9998).ToString()); }
            set { SetGlobalProperty("ServerPort", value); }
        }

        public Launcher(DTE2 _applicationObject, Context context)
        {
            this._applicationObject = _applicationObject;
            this.context = context;
        }

        /// <summary>return a string with the right VS version e.g. Visual Studio 2005</summary>
        /// <param term='versionString'>full version number of the Visual Studio instance</param>
        internal static string getVisualStudioVersion(string versionString)
        {
            string version = "Visual Studio";
            string shortVersionString = versionString.Substring(0, 3);

            switch (shortVersionString)
            {
                case ("12."):
                    version = "Visual Studio 2013";
                    break;
                case ("11."):
                    version = "Visual Studio 2012";
                    break;
                case ("10."):
                    version = "Visual Studio 2010";
                    break;
                case ("9.0"):
                    version = "Visual Studio 2008";
                    break;
                case ("8.0"):
                    version = "Visual Studio 2005";
                    break;
                case ("7.1"):
                    version = "Visual Studio 2003";
                    break;
                default:
                    version = "unknown";
                    break;
            }
            return version;
        }


        private void logProject(Project startProject)
        {
            if (startProject != null)
            {
                string msg = "";
                msg = "FileName: " + startProject.FileName;
                //msg += "\nFullName: " + startProject.FullName;
                if (startProject.CodeModel != null) { 
                    msg += "\nProject-level access to " + startProject.CodeModel.CodeElements.Count.ToString() +
                        " CodeElements through the CodeModel";
                }
                msg += "\nApplication containing this project: " + startProject.DTE.Name + ", Version: " +
                    getVisualStudioVersion(_applicationObject.Version) + " " +
                    _applicationObject.Version;
                //if (startProject.Saved)
                //    msg += "\nThis project hasn't been modified since the last save.";
                //else
                //    msg += "\nThis project has been modified since the last save.";
                //msg += "\nProperties: ";
                //foreach (Property prop in startProject.Properties)
                //{
                //    msg += "\n  " + prop.Name;
                //}
                //msg += "\n  Globale Variables:";
                //foreach (String s in (Array)startProject.Globals.VariableNames)
                //{
                //    msg += "\n  " + s;
                //}

                context.log(Context.LOG_INFO + msg);
            }
        }


        /// <summary>
        /// Launches the active or first runnable project
        /// </summary>
        public void ExecLaunch()
        {
            // get the name of the startup project and then search it by iterating through the project
            // for web projects its not so easy as the StartupProject.Name is not the same as the project.Name - therefore we will fallback to the first IsRunnable project in case we dont find a match

            string startupProjectName = "";
            Project startupProject = null;

            //NavigateSolution();

            SolutionBuild2 solutionBuilder = (SolutionBuild2)_applicationObject.DTE.Solution.SolutionBuild;

            startupProjectName = (string)_applicationObject.DTE.Solution.Properties.Item("StartupProject").Value;
            context.log(Context.LOG_INFO + "Startup project name: " + startupProjectName);

            // traverse projects of solution to get startup project
            // (aware of Solution Folders; see comment on http://msdn.microsoft.com/en-us/library/ms228782.aspx)
            // (http://social.msdn.microsoft.com/Forums/en-US/vsx/thread/36adcd56-5698-43ca-bcba-4527daabb2e3)
            try
            {
                startupProject = _applicationObject.DTE.Solution.Projects.Item(startupProjectName);
            }
            catch (Exception)
            {
                // ArgumentException -> traverse Solution Folders                
                startupProject = GetStartupProject(_applicationObject.DTE.Solution);
            }

            if (startupProject != null)
            {
                logProject(startupProject);
            }
            else
            {
                context.log(Context.LOG_ERROR + "startup project is null");
            }

            Project firstRunnable = null;
            if (IsRunnable(startupProject))
            {
                firstRunnable = startupProject;
            }
            else
            {
                List<Project> projects = GetProjects(_applicationObject.DTE.Solution);

                foreach (Project proj in projects)
                {
                    if (IsRunnable(proj))
                    {
                        firstRunnable = proj;
                        break;
                    }
                }
            }

            if (firstRunnable != null)
            {
                try
                {

                    if (BuildBeforeLaunch)
                        _applicationObject.DTE.Solution.SolutionBuild.Build(true);

                    // start project
                    switch (GetProjectLaunchType(firstRunnable)) {
                        case LaunchType.WebSite:
                            LaunchWebSiteProject(firstRunnable);
                            break;
                        case LaunchType.WebApplication:
                            LaunchWebApplicationProject(firstRunnable);
                            break;
                        case LaunchType.Assembly:
                            LaunchProject(firstRunnable);
                            break;
                    }
                }
                catch (Exception launchExp)
                {
                    context.log(Context.LOG_ERROR + "LaunchException: " + launchExp.Message + "\nSource: " + launchExp.Source + "\nStack: " + launchExp.StackTrace);
                    if (_applicationObject.DTE.Debugger.DebuggedProcesses.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Message: " + launchExp.Message + "\nSource: " + launchExp.Source + "\nStack: " + launchExp.StackTrace, "dynaTrace Launcher", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("dynaTrace Launcher can not start your project if it is already running in debug mode and 'Build Solution before launch' option is checked.");
                    }

                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("dynaTrace Launcher requires an open solution with an active launchable project.", "dynaTrace Launcher", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }

        private bool IsRunnable(Project project)
        {
            try
            {
                // Web Projects in VS2008 throw an exception when accessing ActiveConfiguration - therefore we return true in case we get an excpetion
                return project.ConfigurationManager.ActiveConfiguration.IsRunable;
            }
            catch { }
            return true;
        }

        #region Get Startup Project        
        /// <summary> 
        /// Gets the startup project for the given solution. 
        /// </summary> 
        /// <param name="solution">The solution.</param> 
        /// <returns><c>null</c> if the startup project cannot be found.</returns> 
        public static Project GetStartupProject(Solution solution)
        {
            Project ret = null;

            if (solution != null &&
               solution.SolutionBuild != null &&
               solution.SolutionBuild.StartupProjects != null)
            {
                string uniqueName = (string)((object[])solution.SolutionBuild.StartupProjects)[0];

                // Can't use the solution.Item(uniqueName) here since that doesn't work 
                // for projects under solution folders. 
                //// _applicationObject.DTE.Solution.Item(startupProjectName);

                ret = GetProject(solution, uniqueName);
            }

            return ret;
        }

        /// <summary> 
        /// Gets the project located in the given solution. 
        /// </summary> 
        /// <param name="solution">The solution.</param> 
        /// <param name="uniqueName">The unique name of the project.</param> 
        /// <returns><c>null</c> if the project could not be found.</returns> 
        public static Project GetProject(Solution solution, string uniqueName)
        {
            Project ret = null;

            if (solution != null && uniqueName != null)
            {
                foreach (Project p in solution.Projects)
                {
                    ret = GetSubProject(p, uniqueName);

                    if (ret != null)
                        break;
                }
            }
            return ret;
        }


        /// <summary> 
        /// Gets a project located under another project item. 
        /// </summary> 
        /// <param name="project">The project to start the search from.</param> 
        /// <param name="uniqueName">Unique name of the project.</param> 
        /// <returns><c>null</c> if the project can't be found.</returns> 
        /// <remarks>Only works for solution folders.</remarks> 
        public static Project GetSubProject(Project project, string uniqueName)
        {
            Project ret = null;

            if (project != null)
            {
                if (project.UniqueName == uniqueName)
                {
                    ret = project;
                }
                else if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
                {
                    // Solution folder 
                    foreach (ProjectItem projectItem in project.ProjectItems)
                    {
                        ret = GetSubProject(projectItem.SubProject, uniqueName);

                        if (ret != null)
                            break;
                    }
                }
            }

            return ret;
        }
        #endregion

        #region Fetch Projects recursively (in Solution Folders)
        // traverse projects of solution to get all projects
        // (aware of Solution Folders; see comment on http://msdn.microsoft.com/en-us/library/ms228782.aspx)
        // (http://social.msdn.microsoft.com/Forums/en-US/vsx/thread/36adcd56-5698-43ca-bcba-4527daabb2e3)


        /// <summary> 
        /// Gets the projects located in the given solution. 
        /// </summary> 
        /// <param name="solution">The solution.</param>         
        /// <returns>a list of all projects scattered in solution folders.</returns> 
        public static List<Project> GetProjects(Solution solution)
        {
            List<Project> projects = new List<Project>();

            if (solution != null)
            {
                foreach (Project p in solution.Projects)
                {
                    GetSubProjects(p, ref projects);
                }
            }
            return projects;
        }


        /// <summary> 
        /// Gets a project located under another project item. 
        /// </summary> 
        /// <param name="project">The project to start the search from.</param>         
        /// <remarks>Only works for solution folders.</remarks> 
        public static void GetSubProjects(Project project, ref List<Project> projects)
        {

            if (project != null && projects != null)
            {
                if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems)
                {
                    foreach (ProjectItem projectItem in project.ProjectItems)
                    {
                        GetSubProjects(projectItem.SubProject, ref projects);
                    }
                }
                else
                {
                    projects.Add(project);
                }
            }
        }

        #endregion

        /// <summary>
        /// Launches the process with the passed arguments
        /// </summary>
        /// <param name="startProgram"></param>
        /// <param name="startArguments"></param>
        /// <param name="startWorking"></param>
        /// <param name="assemblyName">only if specified (!=null) we set the dynaTrace env-variables</param>
        public System.Diagnostics.Process LaunchProcess(string startProgram, string startArguments, string startWorking, string assemblyName)
        {
            try
            {
                // lets start the process and specify the environment variables
                System.Diagnostics.ProcessStartInfo pStart = new System.Diagnostics.ProcessStartInfo();
                pStart.FileName = startProgram;
                pStart.Arguments = startArguments;
                pStart.WorkingDirectory = startWorking;
                if (assemblyName != null)
                {
                    pStart.EnvironmentVariables.Remove("DT_AGENTACTIVE");
                    pStart.EnvironmentVariables.Remove("DT_AGENTNAME");
                    pStart.EnvironmentVariables.Remove("COR_PROFILER");
                    pStart.EnvironmentVariables.Remove("COR_ENABLE_PROFILING");
                    pStart.EnvironmentVariables.Remove("DT_SERVER");

                    pStart.EnvironmentVariables.Add("DT_AGENTACTIVE", "true");
                    pStart.EnvironmentVariables.Add("DT_AGENTNAME", CustomAgentName == null ? assemblyName : CustomAgentName);
                    pStart.EnvironmentVariables.Add("COR_PROFILER", AgentGuid);
                    pStart.EnvironmentVariables.Add("COR_ENABLE_PROFILING", "0x01");
                    pStart.EnvironmentVariables.Add("DT_SERVER", String.Format("{0}:{1}", ServerName, ServerPort));
                }
                pStart.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                pStart.CreateNoWindow = false;
                pStart.UseShellExecute = false;
                return System.Diagnostics.Process.Start(pStart);
            }
            catch (Exception exp)
            {
                System.Windows.Forms.MessageBox.Show("Could not launch application with dynaTrace support:\n" + exp.Message + "\nApplication: " + startProgram + "\nArguments: " + startArguments, "dynaTrace Launcher");
            }
            return null;
        }

        private bool HasProperty(Project project, string propName)
        {
            return HasProperty(project.Properties, propName);
        }

        private bool HasProperty(Properties properties, string propName)
        {
            //Dictionary<string, Object> props = new Dictionary<string, Object>();
            System.Collections.IEnumerator propEnum = properties.GetEnumerator();
            bool ret = false;
            while (propEnum.MoveNext())
            {
                //if (((Property)propEnum.Current).Value != null)
                //{
                //    props.Add(((Property)propEnum.Current).Name, "LOKO"/*((Property)propEnum.Current).Value.ToString()*/);
                //}

                if (((Property)propEnum.Current).Name.Equals(propName))
                {
                    ret = true;
                    //return true;
                }
            }
            return ret;
            //return false;
        }

        /// <summary>
        /// Returns the property value or the defaultValue if property does not exist
        /// </summary>
        /// <param name="project"></param>
        /// <param name="propName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private string GetProperty(Project project, string propName, string defaultValue)
        {
            return GetProperty(project.Properties, propName, defaultValue);
        }

        private string GetProperty(Properties properties, string propName, string defaultValue)
        {
            if (HasProperty(properties, propName))
            {
                return properties.Item(propName).Value.ToString();
            }
            return defaultValue;
        }

        private object GetGlobalProperty(string propName, object defaultValue)
        {
            propName = "dtLauncher_" + propName;
            if (_applicationObject.DTE.Solution.Globals.get_VariableExists(propName))
                return _applicationObject.DTE.Solution.Globals[propName];
            if (_applicationObject.DTE.Globals.get_VariableExists(propName))
                return _applicationObject.DTE.Globals[propName];
            return defaultValue;
        }

        private void SetGlobalProperty(string propName, object propValue)
        {
            propName = "dtLauncher_" + propName;
            if (_applicationObject.DTE.Solution != null)
            {
                _applicationObject.DTE.Solution.Globals[propName] = propValue;
                _applicationObject.DTE.Solution.Globals.set_VariablePersists(propName, true);
            }
            else
            {
                _applicationObject.DTE.Globals[propName] = propValue;
                _applicationObject.DTE.Globals.set_VariablePersists(propName, true);
            }
        }

        private string GetVPath(string fullURL, string port)
        {
            int portIx = fullURL.IndexOf(port);
            fullURL = fullURL.Substring(portIx + port.Length);
            return fullURL;
        }

        /// <summary>
        /// returns path to WebDev.WebServer.exe
        /// </summary>
        /// <returns></returns>
        private string GetWebServerPath(Project project)
        {
            string installPath = null;
            // sample path for VS2005: "c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\WebDev.WebServer.EXE"
            // sample path for VS2008: "C:\Program Files\Common Files\Microsoft Shared\DevServer\9.0\WebDev.WebServer.EXE"
            if (_applicationObject.Version.StartsWith("8."))
            {
                Assembly corAssembly = Assembly.GetAssembly(typeof(System.String));
                installPath = System.IO.Path.GetDirectoryName(corAssembly.Location);
                if (!installPath.EndsWith("\\") && !installPath.EndsWith("/"))
                    installPath += "\\";
                installPath += "WebDev.WebServer.EXE";

                if (!System.IO.File.Exists(installPath))
                    throw new ApplicationException(String.Format("Could not find WebDev.WebServer at {0}", installPath));
            }
            else if (_applicationObject.DTE.Version.StartsWith("9."))
            {
                installPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                if (!installPath.EndsWith("\\") && !installPath.EndsWith("/"))
                    installPath += "\\";
                installPath += @"Microsoft Shared\DevServer\9.0\WebDev.WebServer.EXE";

                if (!System.IO.File.Exists(installPath))
                    throw new ApplicationException(String.Format("Could not find WebDev.WebServer at {0}", installPath));
            }
            else if (_applicationObject.DTE.Version.StartsWith("10."))
            {
                installPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                if (!installPath.EndsWith("\\") && !installPath.EndsWith("/"))
                    installPath += "\\";

                // depending on whether the project is running on .NET 4.0 or 2.0 we have two different exe's
                if (GetDotNetFrameworkForProject(project) == 4)
                    installPath += @"Microsoft Shared\DevServer\10.0\WebDev.WebServer40.EXE";
                else
                    installPath += @"Microsoft Shared\DevServer\10.0\WebDev.WebServer20.EXE";

                if (!System.IO.File.Exists(installPath))
                    throw new ApplicationException(String.Format("Could not find WebDev.WebServer at {0}", installPath));
            }

            return installPath;
        }

        private bool expectProperty(string propertyName, string propertyValue)
        {
            if (propertyValue == UNEXPECTED_DEFAULT_PROPERTY_VALUE)
            {
                context.log(Context.LOG_ERROR + "Missing project property \"" + propertyName + "\" in this project.");
                System.Windows.Forms.MessageBox.Show("Could not launch application with dynaTrace support:\nMissing project property \"" + propertyName + "\"", "dynaTrace Launcher", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private int GetDotNetFrameworkForProject(Project project)
        {
            string versionString = GetProperty(project, "TargetFrameworkMoniker", "v2.0");
            return versionString.IndexOf("v4.0") >= 0 ? 4 : 2;
        }

        private void LaunchWebApplicationProject(Project project)
        {
            context.log(Context.LOG_INFO + "Launching Webapp project");
            string DebugStartAction = GetProperty(project, "WebApplication.DebugStartAction", "");
            string ProjectBrowseURL = GetProperty(project, "WebApplication.BrowseURL", "");
            string launchURL = ProjectBrowseURL;
            switch (DebugStartAction)
            {
                case "0": // current page
                          // get active document and its URL
                          // if no document opened, open default project URL
                    Document ActiveDocument = null;
                    try
                    {
                        ActiveDocument = _applicationObject.ActiveDocument;
                    }
                    catch (ArgumentException)
                    {
                        // weird case when project properties tab is open
                        context.log(Context.LOG_INFO + "Launching \"Current page\" start action, but active document can't be located");
                    }
                    if (ActiveDocument != null)
                    {
                        Property BrowseToURL = ActiveDocument.ProjectItem.Properties.Item("EurekaExtender.BrowseToURL");
                        if (BrowseToURL != null)
                        {
                            string DocumentURL = "";
                            try
                            {
                                DocumentURL = BrowseToURL.Value.ToString();
                            }
                            catch (NullReferenceException)
                            {
                                context.log(Context.LOG_INFO + "Launching \"Current page\" start action, but active document (\"" + ActiveDocument.Name + "\") has null \"EurekaExtender.BrowseToURL\" property");
                            }
                            DocumentURL = DocumentURL.Replace("~", "");
                            if (!DocumentURL.StartsWith("/"))
                            {
                                DocumentURL = "/" + DocumentURL;
                            }
                            launchURL += DocumentURL;
                        }
                        else
                        {
                            context.log(Context.LOG_INFO + "Launching \"Current page\" start action, but failed to get \"EurekaExtender.BrowseToURL\" property of active document");
                        }
                    }
                    else
                    {
                        context.log(Context.LOG_INFO + "Launching \"Current page\" start action, but active document is null");
                    }
                    context.log(Context.LOG_INFO + "Launching \"Current page\" start action, launch URL: " + launchURL);
                    break;
                case "1": // specific page
                    string StartPage = GetProperty(project, "WebApplication.StartPageUrl", "");
                    ProjectItem StartPageItem = null;
                    try
                    {
                        StartPageItem = project.ProjectItems.Item(StartPage);
                    }
                    catch (ArgumentException)
                    {
                        context.log(Context.LOG_INFO + "Launching \"Specific page\" start action, but StartPage project item (\"" + StartPage + "\") can't be located");
                    }
                    if (StartPageItem != null)
                    {
                        // FIXME: DUPLICATED CODE!
                        Property BrowseToURL = StartPageItem.Properties.Item("EurekaExtender.BrowseToURL");
                        if (BrowseToURL != null)
                        {
                            string DocumentURL = "";
                            try
                            {
                                DocumentURL = BrowseToURL.Value.ToString();
                            }
                            catch (NullReferenceException)
                            {
                                context.log(Context.LOG_INFO + "Launching \"Specific page\" start action, but selected document (\"" + StartPageItem.Name + "\") has null \"EurekaExtender.BrowseToURL\" property");
                            }
                            DocumentURL = DocumentURL.Replace("~", "");
                            if (!DocumentURL.StartsWith("/"))
                            {
                                DocumentURL = "/" + DocumentURL;
                            }
                            launchURL += DocumentURL;
                        }
                        else
                        {
                            context.log(Context.LOG_INFO + "Launching \"Specific page\" start action, but failed to get \"EurekaExtender.BrowseToURL\" property of selected document");
                        }
                    }
                    else
                    {
                        context.log(Context.LOG_INFO + "Launching \"Specific page\" start action, but StartPage project item is null");
                    }
                    context.log(Context.LOG_INFO + "Launching \"Specific page\" start action, launch URL: " + launchURL);
                    break;
                case "2": // external program
                    context.log(Context.LOG_ERROR + "\"External program\" start action is not supported");
                    System.Windows.Forms.MessageBox.Show("\"External program\" start action is not supported", "dynaTrace Launcher", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    return;
                case "3": // start URL
                    string StartURL = GetProperty(project, "WebApplication.StartExternalUrl", "");
                    if (StartURL.Length > 0)
                    {
                        launchURL = StartURL;
                    }
                    else
                    {
                        context.log(Context.LOG_INFO + "Launching \"Start URL\" start action, but Start URL is empty");
                        System.Windows.Forms.MessageBox.Show("Start URL is empty in project properties", "dynaTrace Launcher", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                    context.log(Context.LOG_INFO + "Launching \"Start URL\" start action, launch URL: " + launchURL);
                    break;
                case "4": // don't open a page
                          // not supported
                    context.log(Context.LOG_ERROR + "\"Don't open a page\" start action is not supported");
                    System.Windows.Forms.MessageBox.Show("\"Don't open a page\" start action is not supported", "dynaTrace Launcher", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    return;
                default:

                    context.log(Context.LOG_ERROR + "Unknown start action: " + DebugStartAction);
                    System.Windows.Forms.MessageBox.Show("Unknown start action value: " + DebugStartAction, "dynaTrace Launcher", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
            }

            // we have to start the internal Microsoft WebServer
            string startProgram = "C:\\Program Files (x86)\\IIS Express\\iisexpress.exe";
            // WB string startWorking = System.IO.Path.GetDirectoryName(_addInInstance.DTE.FileName);
            string startWorking = ".";
            string ProjectPath = GetProperty(project, "FullPath", "");
            string startArguments = "/config:\"" + ProjectPath + "..\\.vs\\config\\applicationhost.config\" /site:\"" + project.Name + "\" /apppool:\"Clr4IntegratedAppPool\"";

            StopLastRunningIIS();

            context.log(Context.LOG_INFO + "Starting new IIS Express instance with command line: " + startProgram + " " + startArguments);
            lastRunningIIS = LaunchProcess(startProgram, startArguments, startWorking, "vs");

            // now we start the browser with the correct page
            OpenWebBrowserWithDelay(launchURL, this.WaitForBrowserTime); 
        }

        private void LaunchWebSiteProject(Project project)
        {
            context.log(Context.LOG_INFO + "Launching WebSite project");

            // website web application config resides beside solution, DevelopmentServerCommandLine property gives proper command line
            string startProgram = GetProperty(project, "DevelopmentServerCommandLine", "");

            StopLastRunningIIS();
            
            context.log(Context.LOG_INFO + "Starting new IIS Express instance with command line: " + startProgram);
            lastRunningIIS = LaunchProcess(startProgram, "", ".", "vs");

            // now we start the browser with the correct page
            OpenWebBrowserWithDelay(GetProperty(project, "BrowseURL", ""), this.WaitForBrowserTime);
        }

        private async void OpenWebBrowserWithDelay(string launchURL, int delay)
        {
            await System.Threading.Tasks.Task.Delay(delay);
            context.log(Context.LOG_INFO + "Starting web browser, url: " + launchURL);
            System.Diagnostics.Process.Start(launchURL);
        }

        private void LaunchProject(Project project)
        {
            // we have to start the output assembly
            // get all project related properties    
            bool result = true;
            string assemblyName = GetProperty(project, "AssemblyName", UNEXPECTED_DEFAULT_PROPERTY_VALUE);
            result &= expectProperty("AssemblyName", assemblyName);
            string projectFileName = System.IO.Path.GetDirectoryName(project.FileName);

            Properties configProperties = project.ConfigurationManager.ActiveConfiguration.Properties;

            string outputDir = GetProperty(configProperties, "OutputPath", UNEXPECTED_DEFAULT_PROPERTY_VALUE);
            result &= expectProperty("OutputPath", assemblyName);
            string startArguments = GetProperty(configProperties, "StartArguments", UNEXPECTED_DEFAULT_PROPERTY_VALUE);
            result &= expectProperty("StartArguments", assemblyName);
            string startWorking = GetProperty(configProperties, "StartWorkingDirectory", UNEXPECTED_DEFAULT_PROPERTY_VALUE);
            result &= expectProperty("StartWorkingDirectory", assemblyName);
            string startProgram = GetProperty(configProperties, "StartProgram", UNEXPECTED_DEFAULT_PROPERTY_VALUE);
            result &= expectProperty("StartProgram", assemblyName);

            // all required project properties found -> we can launch
            if (result)
            {
                // ensure we have the correct path for the executable
                if (!projectFileName.EndsWith("\\") && !projectFileName.EndsWith("/"))
                    projectFileName += "\\";
                if (!outputDir.EndsWith("\\") && !outputDir.EndsWith("/"))
                    outputDir += "\\";
                if (outputDir.StartsWith("\\") || outputDir.StartsWith("/"))
                    outputDir = outputDir.Substring(1);

                // do we start an external program or our output?
                if (startProgram == null || startProgram.Length <= 0)
                    startProgram = String.Format("{0}{1}{2}.exe", projectFileName, outputDir, assemblyName);
                if (startWorking == null || startWorking.Length <= 0)
                    startWorking = projectFileName + outputDir;

                context.log(Context.LOG_INFO + "Launching Application: " + startProgram + ", Startarguments: " + startArguments + ", Workingdir: " + startWorking + ", Assembly: " + assemblyName);

                LaunchProcess(startProgram, startArguments, startWorking, assemblyName);
            }
            
        }

        private void StopLastRunningIIS()
        {

            if (lastRunningIIS != null)
            {
                context.log(Context.LOG_INFO + "Attempting to close running IIS Express instance");
                try
                {
                    // FIXME add error handling, detect if process still running
                    if (!lastRunningIIS.HasExited)
                    {
                        lastRunningIIS.CloseMainWindow();
                        context.log(Context.LOG_INFO + "Sent close request to running IIS Express instance, sleeping...");
                        System.Threading.Thread.Sleep(1000);
                    }
                    lastRunningIIS.Close();
                }
                catch (Exception e)
                {
                    context.log(Context.LOG_ERROR + "Exceptino when closing running IIS Express instance: " + e.GetType().ToString());
                }
            }
        }

        private LaunchType GetProjectLaunchType(Project project)
        {
            IVsSolution solution = (IVsSolution) Package.GetGlobalService(typeof(SVsSolution));

            IVsHierarchy hierarchy = null;
            solution.GetProjectOfUniqueName(project.UniqueName, out hierarchy);

            IVsAggregatableProjectCorrected AP = hierarchy as IVsAggregatableProjectCorrected;
            string projTypeGuids = null;
            AP.GetAggregateProjectTypeGuids(out projTypeGuids);
            projTypeGuids = projTypeGuids.ToUpper();

            if (projTypeGuids.Contains(WEB_SITE_PROJECT_TYPE_GUID.ToUpper())) {
                return LaunchType.WebSite;
            } else if (projTypeGuids.Contains(WEB_APPLICATION_PROJECT_SUBTYPE_GUID.ToUpper())) {
                return LaunchType.WebApplication;
            }

            return LaunchType.Assembly;
        }

    }
}
