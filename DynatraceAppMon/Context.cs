using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using EnvDTE;
using EnvDTE80;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TextRange = EnvDTE.TextRange;
using TextSelection = EnvDTE.TextSelection;

namespace DynaTrace.CodeLink
{

    public class Context
    {

        public const string LOG_INFO = "INFO ";
        public const string LOG_WARN = "WARN ";
        public const string LOG_ERROR = "ERROR ";
        public const string PROJECT_KIND_WEBSITE_PROJECT = "{E24C65DC-7377-472B-9ABA-BC803B73C61A}";

        private DynatraceConfig config;
        private string visualStudioVersion;
        private DTE2 dte;
        private AddinClient connection;


        public Context(DTE2 dte)
        {
            visualStudioVersion = dte.Version;
            this.dte = dte;
            try
            {
                this.config = DynatraceConfig.read(this, visualStudioVersion);
            }
            catch (Exception e)
            {
                log(Context.LOG_ERROR + e.ToString());
                this.config = new DynatraceConfig();
            }
        }

        public string VisualStudioVersion
        {
            get { return dte.Version; }
        }

        private Version _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private string versionString = null;

        public String VersionString
        {
            get
            {
                if (versionString == null)
                {
                    versionString = _version.Major + "." + _version.Minor + "." + _version.Build + "." + _version.Revision;
                }
                return versionString;
            }
        }


        public DynatraceConfig Config
        {
            get
            {
                if (config == null)
                {
                    try
                    {
                        this.config = DynatraceConfig.read(this, visualStudioVersion);
                    }
                    catch (Exception e)
                    {
                        log(Context.LOG_ERROR + e.ToString());
                        this.config = new DynatraceConfig();
                    }
                }
                return config;
            }
        }

        internal void connect()
        {
            if (connection != null)
            {
                connection.disconnect();
            }
            connection = new AddinClient(this);
            connection.connect();
        }

        internal void disconnect()
        {
            if (connection != null)
            {
                connection.disconnect();
            }
        }


        /// <summary>return the path and the name (codeLinkLog + SolutionName + processID)of the logfile</summary>
        private string getLogFilePath()
        {
            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            return
                Path.Combine(
                    Path.Combine(DynatraceConfig.getAppDataPath(), "log"),
                    "log_" + p.Id.ToString() + ".log");
        }

        /// <summary>return the current date and time as string</summary>
        private static string getDateAndTime()
        {
            // 2008-06-23 08:41:56 INFO
            return DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        internal void log(string text)
        {
            try
            {
                StreamWriter sw;
                string path = getLogFilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                if (File.Exists(path))
                {
                    sw = File.AppendText(path);
                }
                else
                {
                    sw = File.CreateText(path);
                }
                sw.WriteLine(getDateAndTime() + " " + text);
                sw.Close();
            }
            catch (Exception)
            {
            }
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

        /// <param term='versionString'>full version number of the Visual Studio instance</param>
        internal static int getVisualStudioVersionAsInt(string versionString)
        {

            int version = -1;
            int majorVersion = int.TryParse(versionString.Substring(0, 3), NumberStyles.Integer | NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"), out majorVersion) ? majorVersion : -1;

            if (majorVersion >= 10)
            {
                version = 4;
            }
            else if (majorVersion == 9)
            {
                version = 3;
            }
            else if (majorVersion == 8)
            {
                version = 2;
            }
            else if (majorVersion == 7)
            {
                version = 1;
            }

            return version;
        }

        internal string SolutionName
        {
            get
            {
                string[] str = dte.Solution.FileName.Split('\\');
                string solName = str[str.Length - 1];

                return solName.Replace(".sln", "");
            }
        }
        internal bool goToMethod(Lookup lookup)
        {
            try
            {
                log(String.Format(Context.LOG_INFO + "goToMethod({0})", lookup.ToString()));

                // fetch projects also from Solution Folders
                List<Project> projects = GetProjects(dte.Solution);

                ArrayList hits = getMethod(lookup, projects);
                if (hits.Count == 1)
                {
                    CodeFunctionProject cfp = (CodeFunctionProject)hits[0];
                    CodeFunction codeFunction = cfp.Function;
                    if (codeFunction != null)
                    {
                        markMethod(codeFunction);
                        return true;
                    }
                }
                else if (hits.Count > 1)
                {
                    SelectMethodDialog cf = new SelectMethodDialog(this, hits, lookup.methodName);
                    cf.ShowDialog();
                    return true;
                }
                else
                {
                    return markType(lookup, dte.Solution.Projects);
                }
                return false;
            }
            catch (Exception e)
            {
                log(Context.LOG_ERROR + e.ToString());
                return false;
            }
        }

        private bool HasClassFiles(ProjectItem projectItem)
        {
            return (GetClassFiles(projectItem, true).Any());
        }

        private List<String> GetClassFiles(ProjectItem projectItem, bool stopOnFirstFound)
        {
            List<String> classFiles = new List<string>();
            //Indexes of FileNames are from 1 to FileCount for the project item.
            for (short i = 1; i <= projectItem.FileCount; i++)
            {
                string fileName = projectItem.FileNames[i];
                string fileExtension = Path.GetExtension(fileName);
                if (fileExtension != null && (fileExtension.EndsWith(".cs")))
                {
                    classFiles.Add(fileName);
                    if (stopOnFirstFound) break;
                }
            }
            return classFiles;
        }

        private List<String> GetClassFiles(ProjectItem projectItem)
        {
            return GetClassFiles(projectItem, false);
        }

        private CodeType FindCodeType(ProjectItems projectItems, string fullName)
        {
            CodeType codeType = null;
            if (projectItems != null)
            {
                foreach (ProjectItem projectItem in projectItems)
                {
                    if (projectItem.FileCodeModel != null && HasClassFiles(projectItem))
                    {
                        codeType = FindCodeType(projectItem.FileCodeModel.CodeElements, fullName);
                        if (codeType != null)
                        {
                            break;
                        }
                    }

                    codeType = FindCodeType(projectItem.ProjectItems, fullName);
                    if (codeType != null)
                    {
                        break;
                    }
                }
            }
            return codeType;
        }

        private CodeType FindCodeType(CodeElements codeElements, string fullName)
        {
            CodeType codeType = null;
            if (codeElements != null)
            {
                foreach (CodeElement codeElement in codeElements)
                {
                    if (codeElement.Kind == vsCMElement.vsCMElementNamespace)
                    {
                        codeType = FindCodeType(((CodeNamespace)codeElement).Members, fullName);
                    }
                    else if (codeElement.Kind == vsCMElement.vsCMElementClass)
                    {
                        if (((CodeClass)codeElement).FullName == fullName)
                        {
                            codeType = (CodeType)codeElement;
                        }
                    }
                    if (codeType != null)
                    {
                        break;
                    }
                }
            }
            return codeType;
        }

        /// <summary>scan every project of the solution about the search method, and returned ArrayList which contains every hit, returns an empty ArrayList if nothing is found</summary>
        /// <param term='lookup'>is a wrapper for all lookup informations.</param>
        /// <param term='projects'>a list of all projects in the solution.</param>
        public ArrayList getMethod(Lookup lookup, List<Project> projects)
        {

            log(String.Format(Context.LOG_INFO + "getMethod called on {0} projects", projects == null ? 0 : projects.Count));

            CodeFunction codeFunction = null;
            ArrayList codeFunctions = new ArrayList();
            CodeClass2 codeClass = null;
            CodeFunctionProject cfp = null;

            foreach (Project p in projects)
            {
                log(String.Format(Context.LOG_INFO + "getMethod working on project {0} - ProjectType is {1}", p.Name, DynaTrace.CodeLink.Context.DecodeProjectKind(p.Kind)));
                try
                {
                    if (p.CodeModel != null)
                    {
                        try
                        {
                            // search in the complete codeModel of the project after the required class
                            codeClass = (CodeClass2)p.CodeModel.CodeTypeFromFullName(lookup.typeName);

                            //search in project items - works for Website projects
                            //CodeClass were not found if class file was not opened
                            if (codeClass == null)
                            {
                                List<String> hits = new List<string>();
                                FindClasses(lookup, p.ProjectItems, ref hits);
                                //Open proper documents
                                foreach (string hit in hits)
                                {
                                    dte.Documents.Open(hit);
                                }
                                codeClass = (CodeClass2)FindCodeType(p.ProjectItems, lookup.typeName);
                            }

                            log(String.Format(Context.LOG_INFO + "getMethod CodeTypeFromFullName({0}) returns {1}", lookup.typeName, codeClass == null ? "null" : codeClass.Name));

                            if (codeClass != null)
                            {
                                codeFunction = getMethod(codeClass, lookup.parameters, false, lookup.methodName);
                                // if the codeFunction is null maybe we can find it in the partial class
                                if (codeFunction == null)
                                {
                                    try
                                    {
                                        if ((codeClass.Parts != null) && (codeClass.Parts.Count != 0))
                                        {
                                            codeFunction = getMethod(codeClass, lookup.parameters, true, lookup.methodName);
                                        }
                                    }
                                    catch (System.Runtime.InteropServices.COMException)
                                    {
                                        // access to codeClass.Parts.Count may throw COMException
                                    }
                                }

                                if (codeFunction != null)
                                {
                                    // Check, if the codeFunction returned can also be opened.
                                    try
                                    {
                                        object test = codeClass.ProjectItem;
                                        test = codeFunction.ProjectItem;
                                        cfp = new CodeFunctionProject(p.Name, codeFunction);
                                        codeFunctions.Add(cfp);
                                    }
                                    catch (System.Runtime.InteropServices.COMException) { }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log(Context.LOG_ERROR + e.ToString());
                        }
                    }
                    else
                    {
                        log(String.Format(Context.LOG_INFO + "Project {0} has no CodeModel - ProjectType is {1}", p.Name, DynaTrace.CodeLink.Context.DecodeProjectKind(p.Kind)));
                        if (p != null) logProjectData(p);
                    }
                }
                catch (NotImplementedException)
                {
                    log(String.Format(Context.LOG_INFO + "Project {0} - No CodeModel Implemented - ProjectType is {1}", p.Name, DynaTrace.CodeLink.Context.DecodeProjectKind(p.Kind)));
                    if (p != null) logProjectData(p);
                }
            }
            return codeFunctions;
        }

        private void FindClasses(Lookup lookup, ProjectItems projectItems, ref List<String> hits)
        {
            foreach (ProjectItem item in projectItems)
            {
                try
                {
                    if (item.ProjectItems != null)
                    {
                        FindClasses(lookup, item.ProjectItems, ref hits);
                    }
                    else if (HasClassFiles(item))
                    {
                        //Indexes of FileNames are from 1 to FileCount for the project item.
                        for (short i = 1; i <= item.FileCount; i++)
                        {
                            SyntaxTree tree = CSharpSyntaxTree.ParseText(File.ReadAllText(item.FileNames[i]));
                            var root = (CompilationUnitSyntax)tree.GetRoot();
                            //get all classes from file
                            var classes = root.DescendantNodes()
                                .OfType<ClassDeclarationSyntax>()
                                .ToList();
                            foreach (var clazz in classes)
                            {
                                if (lookup.typeName.Contains(clazz.Identifier.ToString()))
                                {
                                    hits.AddRange(GetClassFiles(item));
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log(LOG_ERROR + e);
                }
            }
        }

        public static string DecodeProjectKind(string kind)
        {
            const string PROJECT_KIND_ENTERPRISE_PROJECT = "{7D353B21-6E36-11D2-B35A-0000F81F0C06}";
            const string PROJECT_KIND_CPLUSPLUS_PROJECT = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";
            const string PROJECT_KIND_VSNET_SETUP = "{54435603-DBB4-11D2-8724-00A0C9A8B90C}";

            string result = "<unknown>";

            kind = kind.ToUpper();

            if (kind.Equals(VSLangProj.PrjKind.prjKindVBProject.ToUpper()))
            {
                result = "VSLangProj.PrjKind.prjKindVBProject";
            }
            else if (kind.Equals(VSLangProj.PrjKind.prjKindCSharpProject.ToUpper()))
            {
                result = "VSLangProj.PrjKind.prjKindCSharpProject";
            }
            else if (kind.Equals(VSLangProj2.PrjKind2.prjKindVJSharpProject))
            {
                result = "VSLangProj2.PrjKind2.prjKindVJSharpProject";
            }
            else if (kind.Equals(VSLangProj2.PrjKind2.prjKindSDEVBProject))
            {
                result = "VSLangProj2.PrjKind2.prjKindSDEVBProject";
            }
            else if (kind.Equals(VSLangProj2.PrjKind2.prjKindSDECSharpProject))
            {
                result = "VSLangProj2.PrjKind2.prjKindSDECSharpProject";
            }
            else if (kind.Equals(EnvDTE.Constants.vsProjectKindMisc.ToUpper()))
            {
                result = "EnvDTE.Constants.vsProjectKindMisc";
            }
            else if (kind.Equals(EnvDTE.Constants.vsProjectKindSolutionItems.ToUpper()))
            {
                result = "EnvDTE.Constants.vsProjectKindSolutionItems";
            }
            else if (kind.Equals(EnvDTE.Constants.vsProjectKindUnmodeled.ToUpper()))
            {
                result = "EnvDTE.Constants.vsProjectKindUnmodeled";
            }
            else if (kind.Equals(VSLangProj.PrjKind.prjKindVSAProject.ToUpper()))
            {
                result = "VSLangProj.PrjKind.prjKindVSAProject";
            }
            else if (kind.Equals(PROJECT_KIND_ENTERPRISE_PROJECT))
            {
                result = "Enterprise project";
            }
            else if (kind.Equals(PROJECT_KIND_CPLUSPLUS_PROJECT))
            {
                result = "C++ project";
            }
            else if (kind.Equals(PROJECT_KIND_VSNET_SETUP))
            {
                result = "Setup project";
            }
            else if (kind.Equals(PROJECT_KIND_WEBSITE_PROJECT))
            {
                result = "Web Site project";
            }

            return result;
        }


        private void logProjectData(Project p)
        {
            try
            {
                string logMsg = "Fullname: " + p.FullName;
                logMsg += "\nLanguage: " + getLanguage(p);
                logMsg += "\nApplication containing this project: " + p.DTE.Name;
                logMsg += "\nProperties:";
                foreach (Property prop in p.Properties)
                {
                    logMsg += "\n Name: " + prop.Name + " with Value \"" + prop.Value + "\"";
                }
                logMsg += "\nGlobal Vaiables:";
                foreach (String s in (Array)p.Globals.VariableNames)
                {
                    logMsg += "\n " + s;
                }

                log(logMsg);
            }
            catch (Exception) { };
        }

        private string getLanguage(Project p)
        {
            string langs = "unknown";
            try
            {
                if (p.CodeModel != null)
                {
                    CodeModel cm = p.CodeModel;
                    switch (cm.Language)
                    {
                        case CodeModelLanguageConstants.vsCMLanguageMC:
                        case CodeModelLanguageConstants.vsCMLanguageVC:
                            langs += "C++";
                            break;
                        case CodeModelLanguageConstants.vsCMLanguageCSharp:
                            langs += "C#";
                            break;
                        case CodeModelLanguageConstants.vsCMLanguageVB:
                            langs += "Visual Basic";
                            break;
                        case "{E6FDF8BF-F3D1-11D4-8576-0002A516ECE8}":
                            langs += "J#";
                            break;
                    }
                }
            }
            catch
            {
            }
            return langs;
        }


        /// <summary>retrives the searched method as a CodeFunction form the assign CodeClass</summary>
        /// <param term='codeClass'>is the CodeClass that contains the search method</param>
        /// <param term='parameterCount'>number of parameters from the searched method</param>
        /// <param term='parameters'>is a string array, every string is a parameter</param>
        /// <param term='partClass'>is true when the class has a partial class and method is in that--> cast to CodeClass2 (only VS05 and higher)</param>
        /// <param term='methodName'>name of the method</param>
        /// 
        // TODO add method "CodeProperty getProperty(....)"
        private CodeFunction getMethod(CodeClass codeClass, string[] parameters, bool partClass, string methodName)
        {

            log(String.Format(Context.LOG_INFO + "getMethod looks for method {0} in codeClass {1} with {2} parameters and partClass={3}", methodName, codeClass.Name, parameters.Length, partClass));

            CodeElements codeElements;
            if (partClass)
            {
                CodeClass2 codeClass2 = (CodeClass2)codeClass;
                codeElements = codeClass2.Parts;
            }
            else
            {
                codeElements = codeClass.Members;
            }

            foreach (CodeElement elem in codeElements)
            {
                if (elem is CodeClass)
                {
                    CodeFunction cf = getMethod((CodeClass)elem, parameters, false, methodName);
                    if (cf != null)
                    {
                        return cf;
                    }
                }
                if (elem is CodeFunction)
                {
                    CodeFunction cf = (CodeFunction)elem;

                    if (!cf.Name.Equals(methodName)) continue;

                    if (compareParameterList(cf.Parameters, parameters))
                        return cf;
                }
            }
            return null;
        }

        private bool compareParameterList(CodeElements codeElements, string[] parameters)
        {
            Debug.Assert(codeElements != null);
            Debug.Assert(parameters != null);

            // only look further if the parameter counts match
            if (codeElements.Count != parameters.Length) return false;

            //string modifiedParameter;
            int i = 0;
            bool match = true;
            foreach (CodeElement element in codeElements)
            {
                CodeParameter codeParameter = (CodeParameter)element;

                //modifiedParameter = indexOfMultipleParams.Replace(parameters[i], "");

                if (!compareSingleParameter(codeParameter, parameters[i]))
                {
                    //log(String.Format(Context.LOG_INFO + "Parameter[{0}]: types dont match {1}!={2}", i, codeParameter.Type.AsString, modifiedParameter));
                    match = false;
                    break;
                }
                i++;
            }

            return match;
        }

        private bool compareSingleParameter(CodeParameter codeParameter, string parameter)
        {
            Debug.Assert(codeParameter != null);
            Debug.Assert(parameter != null);

            if (parameter.Equals(codeParameter.Type.AsString)) return true;
            if (parameter.StartsWith(codeParameter.Type.AsFullName)) return true; // so we also get the REF Parameters
            if (parameter.Equals("ubyte") && "System.Byte".Equals(codeParameter.Type.AsFullName)) return true;
            if (parameter.Equals("byte") && "System.SByte".Equals(codeParameter.Type.AsFullName)) return true;
            if (parameter.Equals("boolean") && "System.Boolean".Equals(codeParameter.Type.AsFullName)) return true;

            return false;
        }

        /// <summary>mark the method and bring the Visual Studio on front</summary>
        /// <param term='codeFunction'>is the right codeFunction element to the searched method</param>
        /// <param name="methodString">name of the method</param>

        internal void markMethod(CodeFunction codeFunction)
        {

            dte.Documents.Open(codeFunction.ProjectItem.get_FileNames(0), "Auto", false);

            TextSelection textSelection = (TextSelection)dte.ActiveDocument.Selection;

            textSelection.MoveToLineAndOffset(codeFunction.StartPoint.Line, codeFunction.StartPoint.LineCharOffset, false);

            TextRanges textRanges = null;
            string pattern = @"[:b]<{" + codeFunction.Name + @"}>[:b(\n]";
            if (textSelection.FindPattern(pattern, (int)vsFindOptions.vsFindOptionsRegularExpression, ref textRanges))
            {
                TextRange r = textRanges.Item(2);
                textSelection.MoveToLineAndOffset(r.StartPoint.Line, r.StartPoint.LineCharOffset, false);
                textSelection.MoveToLineAndOffset(r.EndPoint.Line, r.EndPoint.LineCharOffset, true);
            }
            dte.MainWindow.Activate();
        }

        internal bool markType(Lookup lookup, Projects projects)
        {
            foreach (Project p in projects)
            {
                try
                {
                    if (p.CodeModel != null)
                    {
                        CodeClass2 codeClass = (CodeClass2)p.CodeModel.CodeTypeFromFullName(lookup.typeName);
                        if (codeClass == null)
                        {
                            CodeType codeType = FindCodeType(p.ProjectItems, lookup.typeName);
                            if (codeType != null)
                            {
                                codeClass = (CodeClass2)codeType;
                            }
                        }
                        if (codeClass != null)
                        {
                            try
                            {
                                object test = codeClass.ProjectItem;
                                dte.Documents.Open(codeClass.ProjectItem.get_FileNames(0), "Auto", false);
                                TextSelection textSelection = (TextSelection)dte.ActiveDocument.Selection;
                                textSelection.MoveToLineAndOffset(codeClass.StartPoint.Line, codeClass.StartPoint.LineCharOffset, false);
                                dte.MainWindow.Activate();
                                log(String.Format(Context.LOG_INFO + "Method {0} not found but type {1} -> marking type", lookup.methodName, lookup.typeName));
                                return true;
                            }
                            catch (System.Runtime.InteropServices.COMException) { }

                        }
                    }
                }
                catch (NotImplementedException) { }
                catch (Exception e)
                {
                    log(Context.LOG_ERROR + e.ToString());
                }
            }

            log(Context.LOG_WARN + "Could not find the Type \"" + lookup.typeName + "\"!");
            return false;
        }


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
                if (project.Kind == Constants.vsProjectKindSolutionItems)
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


        internal string getSolutionPath()
        {
            string[] str = dte.Solution.FullName.Split('\\');
            StringBuilder stringBuilder = new StringBuilder();
            for (int j = 0; j < (str.Length - 1); j++)
            {
                stringBuilder.Append(str[j]);
                stringBuilder.Append('\\');
            }
            return stringBuilder.ToString();
        }

    }

}