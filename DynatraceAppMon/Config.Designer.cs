namespace FirstPackage
{
    partial class Config
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Config));
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.configToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbServerPort = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbServerName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.linkLabelHelp = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.tbAgentName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbBuild = new System.Windows.Forms.CheckBox();
            this.tbWaitTimeBrowser = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.tbClientRestPort = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(300, 383);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 9;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(219, 383);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 23);
            this.btOK.TabIndex = 8;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.cbBuild);
            this.groupBox1.Controls.Add(this.tbWaitTimeBrowser);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.tbAgentName);
            this.groupBox1.Controls.Add(this.tbServerPort);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.tbServerName);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(23, 137);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(352, 154);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Agent";
            this.configToolTip.SetToolTip(this.groupBox1, "dynaTrace Launcher Add-In will launch the currently active project and based on t" +
        "hese settings configures the dynaTrace Agent");
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // tbServerPort
            // 
            this.tbServerPort.Location = new System.Drawing.Point(169, 45);
            this.tbServerPort.Name = "tbServerPort";
            this.tbServerPort.Size = new System.Drawing.Size(156, 20);
            this.tbServerPort.TabIndex = 2;
            this.configToolTip.SetToolTip(this.tbServerPort, "Port of dynaTrace Server to connect the launched process to");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Collector port:";
            this.configToolTip.SetToolTip(this.label6, "Port of dynaTrace Server to connect the launched process to");
            // 
            // tbServerName
            // 
            this.tbServerName.Location = new System.Drawing.Point(169, 18);
            this.tbServerName.Name = "tbServerName";
            this.tbServerName.Size = new System.Drawing.Size(156, 20);
            this.tbServerName.TabIndex = 1;
            this.configToolTip.SetToolTip(this.tbServerName, "dynaTrace Server to connect the launched process to");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Collector host:";
            this.configToolTip.SetToolTip(this.label5, "dynaTrace Server to connect the launched process to");
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pictureBox1.Image = global::FirstPackage.CommandBar.Dynatrace_Logo;
            this.pictureBox1.Location = new System.Drawing.Point(22, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(353, 65);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 22;
            this.pictureBox1.TabStop = false;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkVisited = true;
            this.linkLabel1.Location = new System.Drawing.Point(22, 112);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(104, 13);
            this.linkLabel1.TabIndex = 21;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "www.dynatrace.com";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label9.Location = new System.Drawing.Point(20, 79);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(365, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Build, optimize, test and operate highly-scalable applications with Dynatrace.";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.Location = new System.Drawing.Point(22, 99);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Size = new System.Drawing.Size(129, 13);
            this.linkLabelHelp.TabIndex = 26;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "Help and Troubleshooting";
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Agent name:";
            this.configToolTip.SetToolTip(this.label1, "dynaTrace Agent name to be used by launched process. \r\nIf not specified, the modu" +
        "le name of the launched process will be used as the dynaTrace Agent name");
            // 
            // tbAgentName
            // 
            this.tbAgentName.Location = new System.Drawing.Point(168, 72);
            this.tbAgentName.Name = "tbAgentName";
            this.tbAgentName.Size = new System.Drawing.Size(156, 20);
            this.tbAgentName.TabIndex = 3;
            this.configToolTip.SetToolTip(this.tbAgentName, "dynaTrace Agent name to be used by launched process. \r\nIf not specified, the modu" +
        "le name of the launched process will be used as the dynaTrace Agent name");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Build before launch";
            this.configToolTip.SetToolTip(this.label3, "Build active Visual Studio Solution before launching the active project?");
            // 
            // cbBuild
            // 
            this.cbBuild.AutoSize = true;
            this.cbBuild.Location = new System.Drawing.Point(169, 98);
            this.cbBuild.Name = "cbBuild";
            this.cbBuild.Size = new System.Drawing.Size(15, 14);
            this.cbBuild.TabIndex = 4;
            this.configToolTip.SetToolTip(this.cbBuild, "Build active Visual Studio Solution before launching the active project?");
            this.cbBuild.UseVisualStyleBackColor = true;
            // 
            // tbWaitTimeBrowser
            // 
            this.tbWaitTimeBrowser.Location = new System.Drawing.Point(168, 118);
            this.tbWaitTimeBrowser.Name = "tbWaitTimeBrowser";
            this.tbWaitTimeBrowser.Size = new System.Drawing.Size(156, 20);
            this.tbWaitTimeBrowser.TabIndex = 5;
            this.tbWaitTimeBrowser.TextChanged += new System.EventHandler(this.tbWaitTimeBrowser_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 121);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(137, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Launch delay for Web [ms]:";
            this.configToolTip.SetToolTip(this.label4, "When active project is a Visual Studio Web Project your default browser is launch" +
        "ed after the Web Application is deployed on the ASP.NET Development Server.");
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.tbClientRestPort);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.checkBox1);
            this.groupBox4.Location = new System.Drawing.Point(23, 297);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(352, 80);
            this.groupBox4.TabIndex = 27;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "CodeLink";
            // 
            // tbClientRestPort
            // 
            this.tbClientRestPort.Location = new System.Drawing.Point(169, 45);
            this.tbClientRestPort.Name = "tbClientRestPort";
            this.tbClientRestPort.Size = new System.Drawing.Size(70, 20);
            this.tbClientRestPort.TabIndex = 7;
            this.tbClientRestPort.Text = "8030";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 48);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "Client port:";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(16, 21);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(107, 17);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "Enable CodeLink";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // Config
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(399, 416);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.linkLabelHelp);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Config";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dynatrace AppMon Extension";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.ToolTip configToolTip;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbServerPort;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbServerName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.LinkLabel linkLabelHelp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbBuild;
        private System.Windows.Forms.TextBox tbWaitTimeBrowser;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbAgentName;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox tbClientRestPort;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}