using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace FirstPackage
{
    public partial class Config : Form
    {
        private Launcher _addIn;

        private DynaTrace.CodeLink.DynatraceConfig startUpConfig = null;
        private DynaTrace.CodeLink.Context context;
        public DynaTrace.CodeLink.Context Context
        {
            get { return context; }
            set { context = value; }
        }

        public DynaTrace.CodeLink.DynatraceConfig StartUpConfig
        {
            get { return startUpConfig; }
            set { startUpConfig = value; }
        }

        public CheckBox EnabledCheckBox
        {
            get { return checkBox1; }
            set { checkBox1 = value; }
        }

        public Button OkButton
        {
            get { return btOK; }
        }

        public Config(Launcher addIn, DynaTrace.CodeLink.Context context)
        {
            _addIn = addIn;
            InitializeComponent();

            string link = "http://www.dynatrace.com";
            linkLabel1.Links.Add(0, link.Length, link);
            string linkHelp = "https://community.dynatrace.com/community/display/DL/Visual+Studio+2015+Extension";
            linkLabelHelp.Links.Add(0, linkHelp.Length, linkHelp);

            this.context = context;
            startUpConfig = new DynaTrace.CodeLink.DynatraceConfig(context.Config);

            this.tbAgentName.Text = addIn.CustomAgentName;
            this.tbServerName.Text = addIn.ServerName;
            this.tbServerPort.Text = addIn.ServerPort.ToString();
            this.cbBuild.Checked = addIn.BuildBeforeLaunch;
            this.tbClientRestPort.Text = context.Config.ClientPort.ToString();
            this.tbWaitTimeBrowser.Text = addIn.WaitForBrowserTime.ToString();
            //if (_addIn.AgentGuid == "{79B3AFB5-500F-4950-91F1-B5486984099E}") // 2.6
            //    this.cbVersion.SelectedIndex = 0;
            //if (_addIn.AgentGuid == "{EED96696-27C0-45F1-B406-E79C92C3F372}") // 3.0
            //    this.cbVersion.SelectedIndex = 1;
            //if (_addIn.AgentGuid == "{E39D7032-0104-49f7-A694-F7FDB39ABCE0}") // 3.1
            //    this.cbVersion.SelectedIndex = 2;
            //if (_addIn.AgentGuid == "{00AADF80-D8CA-45B6-AF1C-7BC09077B44F}") // 3.2
            //    this.cbVersion.SelectedIndex = 3;
            //if (_addIn.AgentGuid == "{4C9739F3-FD77-4ee8-83FF-D7888B2277A9}") // 3.5
            //    this.cbVersion.SelectedIndex = 4;
            //if (_addIn.AgentGuid == "{E1F00E4B-4010-4a1c-8E9B-6DFA7E802D15}") // 3.5.1
            //    this.cbVersion.SelectedIndex = 5;
            //if (_addIn.AgentGuid == "{709179A4-5D87-4c2e-9EE6-90A8CFCC37FF}") // 3.5.2
            //    this.cbVersion.SelectedIndex = 6;
            //if (_addIn.AgentGuid == "{333A026A-B413-486f-91A6-A33D8C9874D6}") // 4.0.0
            //    this.cbVersion.SelectedIndex = 7;
            //if (_addIn.AgentGuid == "{DA7CFC47-3E35-4c4e-B495-534F93B28683}") // 4.1.0
            //    this.cbVersion.SelectedIndex = 8;

            //TODO fetch agent GUIDs from https://wiki.dynatrace.local/display/DEV/.NET+GUIDs

            //this.cbVSTestHost.Checked = addIn.EnableVSTestHost;

            DynaTrace.CodeLink.ConfigFormCode.Load(context, this);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            DynaTrace.CodeLink.ConfigFormCode.EnableChanged(this);
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (!DynaTrace.CodeLink.ConfigFormCode.Ok(context, EnabledCheckBox.Checked, tbClientRestPort.Text, startUpConfig))
                return;

            _addIn.ServerName = this.tbServerName.Text;
            _addIn.CustomAgentName = this.tbAgentName.Text;
            _addIn.BuildBeforeLaunch = this.cbBuild.Checked;
            //_addIn.EnableVSTestHost = this.cbVSTestHost.Checked;
            //if (this.cbVersion.SelectedIndex == 0)
            //    _addIn.AgentGuid = "{79B3AFB5-500F-4950-91F1-B5486984099E}";
            //if (this.cbVersion.SelectedIndex == 1)
            //    _addIn.AgentGuid = "{EED96696-27C0-45F1-B406-E79C92C3F372}";
            //if (this.cbVersion.SelectedIndex == 2)
            //    _addIn.AgentGuid = "{E39D7032-0104-49f7-A694-F7FDB39ABCE0}";
            //if (this.cbVersion.SelectedIndex == 3)
            //    _addIn.AgentGuid = "{00AADF80-D8CA-45B6-AF1C-7BC09077B44F}";
            //if (this.cbVersion.SelectedIndex == 4)
            //    _addIn.AgentGuid = "{4C9739F3-FD77-4ee8-83FF-D7888B2277A9}";
            //if (this.cbVersion.SelectedIndex == 5)
            //    _addIn.AgentGuid = "{E1F00E4B-4010-4a1c-8E9B-6DFA7E802D15}";
            //if (this.cbVersion.SelectedIndex == 6)
            //    _addIn.AgentGuid = "{709179A4-5D87-4c2e-9EE6-90A8CFCC37FF}";
            //if (this.cbVersion.SelectedIndex == 7)
            //    _addIn.AgentGuid = "{333A026A-B413-486f-91A6-A33D8C9874D6}";
            //if (this.cbVersion.SelectedIndex == 8)
            _addIn.AgentGuid = "{DA7CFC47-3E35-4c4e-B495-534F93B28683}";

            //TODO fetch agent GUIDs from https://wiki.dynatrace.local/display/DEV/.NET+GUIDs

            try
            {
                _addIn.WaitForBrowserTime = Int32.Parse(this.tbWaitTimeBrowser.Text);
            }
            catch { 
                MessageBox.Show("Invalid format of Wait for Browser value. Must be a number (milliseconds)", "dynaTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                _addIn.ServerPort = Int32.Parse(this.tbServerPort.Text);
            }
            catch
            {
                MessageBox.Show("Invalid format of Server Port value. Must be a number", "dynaTrace", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void btCancel_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void linkLabelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData.ToString());
        }

        private void tbWaitTimeBrowser_TextChanged(object sender, EventArgs e)
        {

        }
    }
}