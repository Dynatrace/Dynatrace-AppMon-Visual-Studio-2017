//------------------------------------------------------------------------------
// <copyright file="FirstPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Resources;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.Win32;
using EnvDTE;
using EnvDTE80;


namespace FirstPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(FirstPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class FirstPackage : Package
    {
        /// <summary>
        /// FirstPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "3fd87c63-4e32-4bd6-87fb-b7a12c35176d";
        DynaTrace.CodeLink.Context context;
        private bool _bConnected = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPackage"/> class.
        /// </summary>
        public FirstPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            DTE2 dte = (DTE2)FirstPackage.GetGlobalService(typeof(DTE));

            if (context == null)
            {
                context = new DynaTrace.CodeLink.Context(dte);
            }

            context.log("onConnection - Initialize");

            context.log(DynaTrace.CodeLink.Context.LOG_INFO + "CodeLink Version: " + context.VersionString);

            //from MS-Code only UISetup is necessary, but we need Startup because we have troubles with VS05 (show the menu item)
            // if (connectMode == ext_ConnectMode.ext_cm_UISetup || connectMode == ext_ConnectMode.ext_cm_Startup) {

            if (!_bConnected)
            {
                object[] contextGUIDS = new object[] { };

                string toolsMenuName;
                // if ((!found && connectMode == ext_ConnectMode.ext_cm_Startup) || (connectMode == ext_ConnectMode.ext_cm_UISetup)) {
                bool found = false;
                if (!found)
                {
                    try
                    {
                        ResourceManager resourceManager = new ResourceManager("DynaTrace.CodeLink.CommandBar", Assembly.GetExecutingAssembly());
                        CultureInfo cultureInfo = new System.Globalization.CultureInfo(dte.LocaleID);
                        string resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
                        toolsMenuName = resourceManager.GetString(resourceName);
                    }
                    catch
                    {
                        //We tried to find a localized version of the word Tools, but one was not found.
                        //  Default to the en-US word, which may work for the current culture.
                        toolsMenuName = "Tools";
                    }

                    // Place the command on the tools menu.
                    // Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                    Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar =
                        ((Microsoft.VisualStudio.CommandBars.CommandBars)dte.CommandBars)["MenuBar"];

                    // Find the Tools command bar on the MenuBar command bar:
                    CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
                    CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                    //// Create the menu entry for dynaTrace Configuration Dialog
                    //try
                    //{
                    //    try
                    //    {
                    //        _commandSetup = (Command)commands.Item(COMMAND_NAME + ".Setup", 0);
                    //    }
                    //    catch (Exception) { /* context.log("INFO " + e.ToString()); */}

                    //    if (_commandSetup == null)
                    //        _commandSetup = commands.AddNamedCommand2(_addInInstance, "Setup", "dynaTrace Configuration",
                    //            "Opens the configuration dialog for CodeLink & Launcher", false, 1,
                    //            ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled,
                    //            (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                    //    else
                    //        _commandSetup = null;
                    //}
                    //catch (Exception e)
                    //{
                    //    //If we are here, then the exception is probably because a command with that name
                    //    //  already exists. If so there is no need to recreate the command and we can 
                    //    //  safely ignore the exception.
                    //    context.log(Context.LOG_ERROR + e.ToString());
                    //}

                    //// Create the menu entry for the dynaTrace Launcher
                    //try
                    //{
                    //    try
                    //    {
                    //        _commandLauncher = (Command)commands.Item(COMMAND_NAME + ".dynaTraceLauncher", 0);
                    //    }
                    //    catch (System.Exception) { /* context.log("INFO " + e.ToString()); */}

                    //    //Add a command to the Commands collection:
                    //    if (_commandLauncher == null)
                    //        _commandLauncher = commands.AddNamedCommand2(_addInInstance, "dynaTraceLauncher", "dynaTrace Launcher", "Launches the startup project with dynaTrace Support", false, 1 /*59*/, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);
                    //    else
                    //        _commandLauncher = null; // already there - therefore dont add it again to the tools menu
                    //}
                    //catch (System.Exception e)
                    //{
                    //    context.log(Context.LOG_ERROR + e.ToString());
                    //}

                    ////Add to tools menu
                    //if (toolsPopup != null)
                    //{
                    //    try
                    //    {
                    //        if (_commandSetup != null)
                    //            _commandSetup.AddControl(toolsPopup.CommandBar, 1);
                    //        if (_commandLauncher != null)
                    //            _commandLauncher.AddControl(toolsPopup.CommandBar, 1);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        context.log(Context.LOG_ERROR + e.ToString());
                    //    }
                    //}
                }
                _bConnected = true;
            }

            // create the launcher implementation class
//            if (_launcher == null)
//            {
//                _launcher = new dynaTraceLauncher.LauncherConnect(_applicationObject, _addInInstance);
//                _launcher.Context = context;
//            }
//            // In VS2010 also create the WebTestPlugin
//#if (VS_2010)
//            try
//            {
//                dynaTrace.VSTS.WebResultAddin.WebTestConnect webTestConnect = new dynaTrace.VSTS.WebResultAddin.WebTestConnect();
//                webTestConnect.OnConnection(application, connectMode, addInInst, ref custom);
//            }
//            catch (Exception e) {context.log(Context.LOG_ERROR + e.ToString());}
//#endifs

            if (context.Config.PluginEnabled)
            {
                context.connect();
            }

            LaunchCommand.Initialize(this);
            LaunchCommand.Instance.Launcher = new Launcher(dte, context);
            ConfigCommand.Initialize(this);
            ConfigCommand.Instance.Context = context;
            ConfigCommand.Instance.Launcher = LaunchCommand.Instance.Launcher;
            RunTestsMenu.Initialize(this);
        }
        protected override void Dispose(bool disposing)
        {
            context.log("onDisconnection - ");
            _bConnected = false;
            context = null;
            base.Dispose(disposing);
        }

        #endregion
    }
}
