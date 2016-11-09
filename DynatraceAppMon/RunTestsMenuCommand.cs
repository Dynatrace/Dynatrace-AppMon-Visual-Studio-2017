using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.Win32;

namespace FirstPackage
{
    class RunTestsMenuCommand : OleMenuCommand
    {

        public DTE2 Dte2 { get; private set; }

        public RunTestsMenuCommand(CommandID commandID, DTE2 dte2) : base(MenuItemCallback, commandID)
        {
            this.Dte2 = dte2;
            BeforeQueryStatus += new EventHandler(OnBeforeQueryStatus);
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (null != myCommand)
            {
                UIHierarchy uih = Dte2.ToolWindows.SolutionExplorer;
                Array selectedItems = (Array)uih.SelectedItems;
                UIHierarchyItem selectedItem = selectedItems.GetValue(0) as UIHierarchyItem;
                Solution sol = selectedItem.Object as Solution;
                if (sol != null && RunTestsMenu.HasTestProjects(sol))
                {
                    myCommand.Visible = true;
                    return;
                }
                Project prj = selectedItem.Object as Project;
                if (prj == null || !RunTestsMenu.IsTestProject(prj))
                {
                    myCommand.Visible = false;
                    return;
                }

                myCommand.Visible = true;
            }
        }

        private static void RunTestProject(Project project, Solution solution)
        {
            //System.Diagnostics.Debug.WriteLine("RunTestsMenuCommand.MenuItemCallback()! " + project.Name + " ");
            //System.Diagnostics.Debug.WriteLine(project.Properties.Item("OutputFileName").Value.ToString());
            //System.Diagnostics.Debug.WriteLine(project.Properties.Item("FullPath").Value.ToString());
            //System.Diagnostics.Debug.WriteLine(GetVisualStudioInstallationPath());
            SolutionConfiguration2 solutionConfiguration2 = (SolutionConfiguration2)solution.SolutionBuild.ActiveConfiguration;
            string ProjectFullPath = project.Properties.Item("FullPath").Value.ToString();

            string buildDir = ProjectFullPath + "bin\\" + solutionConfiguration2.Name + "\\";

            string CommandLine = GetVisualStudioInstallationPath() + "mstest.exe /noresults /resultsfile:\"" + buildDir + "appmon_test_results.trx\" /testcontainer:\"" + buildDir + project.Properties.Item("OutputFileName").Value.ToString() + "\"";
            //System.Diagnostics.Debug.WriteLine(CommandLine);

            System.Diagnostics.Process process = LaunchCommand.Instance.Launcher.LaunchProcess(GetVisualStudioInstallationPath() + "\\mstest.exe", "/noresults /resultsfile:\"" + buildDir + "appmon_test_results.trx\" /testcontainer:\"" + buildDir + project.Properties.Item("OutputFileName").Value.ToString() + "\"", buildDir, "vs");
            process.WaitForExit();
        }

        private static void MenuItemCallback(object sender, EventArgs e)
        {
            RunTestsMenuCommand command = (RunTestsMenuCommand)sender;
            UIHierarchy uih = command.Dte2.ToolWindows.SolutionExplorer;
            Array selectedItems = (Array)uih.SelectedItems;
            UIHierarchyItem selectedItem = selectedItems.GetValue(0) as UIHierarchyItem;
            Solution solution = command.Dte2.Solution;
            Solution selectedSolution = selectedItem.Object as Solution;

            if (LaunchCommand.Instance.Launcher.BuildBeforeLaunch)
            {
                solution.SolutionBuild.Build(true);
            }

            if (selectedSolution != null)
            {
                //System.Diagnostics.Debug.WriteLine("RunTestsMenuCommand.MenuItemCallback() running all test projects for the solution");
                foreach (Project p in selectedSolution.Projects)
                {
                    if (RunTestsMenu.IsTestProject(p))
                    {
                        RunTestProject(p, solution);
                    }
                }
                return;
            }
            Project project = selectedItem.Object as Project;
            if (project != null)
            {
                //System.Diagnostics.Debug.WriteLine("RunTestsMenuCommand.MenuItemCallback() running a single test project");
                RunTestProject(project, solution);
            }
        }

        private static string GetVisualStudioInstallationPath()
        {
            string installationPath = null;
            if (Environment.Is64BitOperatingSystem)
            {
                installationPath = (string)Registry.GetValue(
                   "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\VisualStudio\\14.0\\",
                    "InstallDir",
                    null);
            }
            else
            {
                installationPath = (string)Registry.GetValue(
           "HKEY_LOCAL_MACHINE\\SOFTWARE  \\Microsoft\\VisualStudio\\14.0\\",
                  "InstallDir",
                  null);
            }
            return installationPath;

        }

    }
}
