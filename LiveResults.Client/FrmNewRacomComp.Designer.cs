namespace LiveResults.Client
{
    partial class FrmNewRacomComp
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtStartlist = new System.Windows.Forms.TextBox();
            this.txtRawSplits = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRaceFile = new System.Windows.Forms.TextBox();
            this.Resultfile = new System.Windows.Forms.Label();
            this.txtRadioControls = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.txtCompID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.dtZeroTime = new System.Windows.Forms.DateTimePicker();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btn_loadsetting = new System.Windows.Forms.Button();
            this.openSettingsDialog = new System.Windows.Forms.OpenFileDialog();
            this.button3 = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.cbStart = new System.Windows.Forms.CheckBox();
            this.btnSplits = new System.Windows.Forms.Button();
            this.btnFinish = new System.Windows.Forms.Button();
            this.btnSCodes = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.labelCompUsername = new System.Windows.Forms.Label();
            this.labelCompPassword = new System.Windows.Forms.Label();
            this.txtCompUsername = new System.Windows.Forms.TextBox();
            this.txtCompPassword = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Startlist";
            // 
            // txtStartlist
            // 
            this.txtStartlist.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStartlist.Location = new System.Drawing.Point(15, 25);
            this.txtStartlist.Name = "txtStartlist";
            this.txtStartlist.Size = new System.Drawing.Size(538, 20);
            this.txtStartlist.TabIndex = 1;
            // 
            // txtRawSplits
            // 
            this.txtRawSplits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRawSplits.Location = new System.Drawing.Point(15, 64);
            this.txtRawSplits.Name = "txtRawSplits";
            this.txtRawSplits.Size = new System.Drawing.Size(538, 20);
            this.txtRawSplits.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Raw splits";
            // 
            // txtRaceFile
            // 
            this.txtRaceFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRaceFile.Location = new System.Drawing.Point(15, 103);
            this.txtRaceFile.Name = "txtRaceFile";
            this.txtRaceFile.Size = new System.Drawing.Size(538, 20);
            this.txtRaceFile.TabIndex = 5;
            // 
            // Resultfile
            // 
            this.Resultfile.AutoSize = true;
            this.Resultfile.Location = new System.Drawing.Point(12, 87);
            this.Resultfile.Name = "Resultfile";
            this.Resultfile.Size = new System.Drawing.Size(112, 13);
            this.Resultfile.TabIndex = 4;
            this.Resultfile.Text = "Race file (Finish times)";
            // 
            // txtRadioControls
            // 
            this.txtRadioControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRadioControls.Location = new System.Drawing.Point(15, 142);
            this.txtRadioControls.Name = "txtRadioControls";
            this.txtRadioControls.Size = new System.Drawing.Size(538, 20);
            this.txtRadioControls.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(464, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Intermediate control file (<XYZ>.splitcodes.txt && need also <XYZ>.splitnames.txt" +
    " in same directory)";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(397, 376);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "&OK";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(478, 376);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "&Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // txtCompID
            // 
            this.txtCompID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCompID.Location = new System.Drawing.Point(15, 181);
            this.txtCompID.Name = "txtCompID";
            this.txtCompID.Size = new System.Drawing.Size(236, 20);
            this.txtCompID.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(180, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "CompetitionID (Emma web server ID)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 312);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Zerotime";
            // 
            // dtZeroTime
            // 
            this.dtZeroTime.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dtZeroTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtZeroTime.Location = new System.Drawing.Point(15, 328);
            this.dtZeroTime.Name = "dtZeroTime";
            this.dtZeroTime.Size = new System.Drawing.Size(236, 20);
            this.dtZeroTime.TabIndex = 15;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 354);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(61, 17);
            this.checkBox1.TabIndex = 16;
            this.checkBox1.Text = "IsRelay";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // btn_loadsetting
            // 
            this.btn_loadsetting.Location = new System.Drawing.Point(12, 377);
            this.btn_loadsetting.Name = "btn_loadsetting";
            this.btn_loadsetting.Size = new System.Drawing.Size(124, 23);
            this.btn_loadsetting.TabIndex = 18;
            this.btn_loadsetting.Text = "Load settings from file";
            this.btn_loadsetting.UseVisualStyleBackColor = true;
            this.btn_loadsetting.Click += new System.EventHandler(this.btn_loadsetting_Click);
            // 
            // openSettingsDialog
            // 
            this.openSettingsDialog.DefaultExt = "xml";
            this.openSettingsDialog.Filter = "XML-files|*.xml";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(142, 377);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(120, 23);
            this.button3.TabIndex = 19;
            this.button3.Text = "Save settings to file";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(532, 8);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(20, 11);
            this.btnStart.TabIndex = 20;
            this.btnStart.Text = "...";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // cbStart
            // 
            this.cbStart.AutoSize = true;
            this.cbStart.Location = new System.Drawing.Point(220, 8);
            this.cbStart.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.cbStart.Name = "cbStart";
            this.cbStart.Size = new System.Drawing.Size(185, 17);
            this.cbStart.TabIndex = 21;
            this.cbStart.Text = "Use extended csv file read (2023)";
            this.cbStart.UseVisualStyleBackColor = true;
            // 
            // btnSplits
            // 
            this.btnSplits.Location = new System.Drawing.Point(532, 48);
            this.btnSplits.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSplits.Name = "btnSplits";
            this.btnSplits.Size = new System.Drawing.Size(20, 11);
            this.btnSplits.TabIndex = 22;
            this.btnSplits.Text = "...";
            this.btnSplits.UseVisualStyleBackColor = true;
            this.btnSplits.Click += new System.EventHandler(this.btnSplits_Click);
            // 
            // btnFinish
            // 
            this.btnFinish.Location = new System.Drawing.Point(532, 87);
            this.btnFinish.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnFinish.Name = "btnFinish";
            this.btnFinish.Size = new System.Drawing.Size(20, 11);
            this.btnFinish.TabIndex = 23;
            this.btnFinish.Text = "...";
            this.btnFinish.UseVisualStyleBackColor = true;
            this.btnFinish.Click += new System.EventHandler(this.btnFinish_Click);
            // 
            // btnSCodes
            // 
            this.btnSCodes.Location = new System.Drawing.Point(532, 126);
            this.btnSCodes.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSCodes.Name = "btnSCodes";
            this.btnSCodes.Size = new System.Drawing.Size(20, 11);
            this.btnSCodes.TabIndex = 24;
            this.btnSCodes.Text = "...";
            this.btnSCodes.UseVisualStyleBackColor = true;
            this.btnSCodes.Click += new System.EventHandler(this.btnSCodes_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(394, 165);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Control number of Finish";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(397, 182);
            this.numericUpDown1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(155, 20);
            this.numericUpDown1.TabIndex = 26;
            this.numericUpDown1.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // labelCompUsername
            // 
            this.labelCompUsername.AutoSize = true;
            this.labelCompUsername.Location = new System.Drawing.Point(12, 209);
            this.labelCompUsername.Name = "labelCompUsername";
            this.labelCompUsername.Size = new System.Drawing.Size(55, 13);
            this.labelCompUsername.TabIndex = 27;
            this.labelCompUsername.Text = "Username";
            // 
            // labelCompPassword
            // 
            this.labelCompPassword.AutoSize = true;
            this.labelCompPassword.Location = new System.Drawing.Point(12, 251);
            this.labelCompPassword.Name = "labelCompPassword";
            this.labelCompPassword.Size = new System.Drawing.Size(53, 13);
            this.labelCompPassword.TabIndex = 28;
            this.labelCompPassword.Text = "Password";
            // 
            // txtCompUsername
            // 
            this.txtCompUsername.Location = new System.Drawing.Point(15, 225);
            this.txtCompUsername.Name = "txtCompUsername";
            this.txtCompUsername.Size = new System.Drawing.Size(236, 20);
            this.txtCompUsername.TabIndex = 29;
            // 
            // txtCompPassword
            // 
            this.txtCompPassword.Location = new System.Drawing.Point(15, 268);
            this.txtCompPassword.Name = "txtCompPassword";
            this.txtCompPassword.PasswordChar = '*';
            this.txtCompPassword.Size = new System.Drawing.Size(236, 20);
            this.txtCompPassword.TabIndex = 30;
            // 
            // FrmNewRacomComp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 411);
            this.Controls.Add(this.txtCompPassword);
            this.Controls.Add(this.txtCompUsername);
            this.Controls.Add(this.labelCompPassword);
            this.Controls.Add(this.labelCompUsername);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnSCodes);
            this.Controls.Add(this.btnFinish);
            this.Controls.Add(this.btnSplits);
            this.Controls.Add(this.cbStart);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btn_loadsetting);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.dtZeroTime);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtCompID);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtRadioControls);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtRaceFile);
            this.Controls.Add(this.Resultfile);
            this.Controls.Add(this.txtRawSplits);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtStartlist);
            this.Controls.Add(this.label1);
            this.Name = "FrmNewRacomComp";
            this.Text = "New Racom Connection";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label Resultfile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        internal System.Windows.Forms.TextBox txtStartlist;
        internal System.Windows.Forms.TextBox txtRawSplits;
        internal System.Windows.Forms.TextBox txtRaceFile;
        internal System.Windows.Forms.TextBox txtRadioControls;
        internal System.Windows.Forms.TextBox txtCompID;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.DateTimePicker dtZeroTime;
        public System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button btn_loadsetting;
        private System.Windows.Forms.OpenFileDialog openSettingsDialog;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnStart;
        public System.Windows.Forms.CheckBox cbStart;
        private System.Windows.Forms.Button btnSplits;
        private System.Windows.Forms.Button btnFinish;
        private System.Windows.Forms.Button btnSCodes;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label labelCompUsername;
        private System.Windows.Forms.Label labelCompPassword;
        private System.Windows.Forms.TextBox txtCompUsername;
        private System.Windows.Forms.TextBox txtCompPassword;
    }
}