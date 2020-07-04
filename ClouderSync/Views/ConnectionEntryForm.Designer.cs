using ClouderSync.Data;

namespace ClouderSync.Views
{
    partial class ConnectionEntryForm
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
            this.labelHostname = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.editUsername = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.editPassword = new System.Windows.Forms.TextBox();
            this.checkUseKey = new System.Windows.Forms.CheckBox();
            this.dlgOpenKeyFile = new System.Windows.Forms.OpenFileDialog();
            this.btnPickKeyFile = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.editHostname = new System.Windows.Forms.TextBox();
            this.editKeyFile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.editLocalSrcPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.editRemoteSrcPath = new System.Windows.Forms.TextBox();
            this.btnTestConnect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.editPort = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.editLog = new System.Windows.Forms.TextBox();
            this.checkNoTransferExcludedFiles = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // labelHostname
            // 
            this.labelHostname.Location = new System.Drawing.Point(37, 36);
            this.labelHostname.Name = "labelHostname";
            this.labelHostname.Size = new System.Drawing.Size(176, 23);
            this.labelHostname.TabIndex = 1;
            this.labelHostname.Text = "Host name:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(37, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(176, 23);
            this.label2.TabIndex = 4;
            this.label2.Text = "User name:";
            // 
            // editUsername
            // 
            this.editUsername.Location = new System.Drawing.Point(219, 116);
            this.editUsername.Name = "editUsername";
            this.editUsername.Size = new System.Drawing.Size(163, 22);
            this.editUsername.TabIndex = 3;
            // 
            // labelPassword
            // 
            this.labelPassword.Location = new System.Drawing.Point(37, 153);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(176, 23);
            this.labelPassword.TabIndex = 6;
            this.labelPassword.Text = "Password:";
            // 
            // editPassword
            // 
            this.editPassword.Location = new System.Drawing.Point(219, 154);
            this.editPassword.Name = "editPassword";
            this.editPassword.Size = new System.Drawing.Size(163, 22);
            this.editPassword.TabIndex = 5;
            // 
            // checkUseKey
            // 
            this.checkUseKey.AutoSize = true;
            this.checkUseKey.Location = new System.Drawing.Point(40, 192);
            this.checkUseKey.Name = "checkUseKey";
            this.checkUseKey.Size = new System.Drawing.Size(100, 21);
            this.checkUseKey.TabIndex = 7;
            this.checkUseKey.Text = "Use key file";
            this.checkUseKey.UseVisualStyleBackColor = true;
            // 
            // dlgOpenKeyFile
            // 
            this.dlgOpenKeyFile.Filter = "pem files|*.pem|All files|*.*";
            // 
            // btnPickKeyFile
            // 
            this.btnPickKeyFile.Location = new System.Drawing.Point(389, 190);
            this.btnPickKeyFile.Name = "btnPickKeyFile";
            this.btnPickKeyFile.Size = new System.Drawing.Size(30, 23);
            this.btnPickKeyFile.TabIndex = 9;
            this.btnPickKeyFile.Text = "...";
            this.btnPickKeyFile.UseVisualStyleBackColor = true;
            this.btnPickKeyFile.Click += new System.EventHandler(this.btnPickKeyFile_Click);
            // 
            // btnSave
            // 
            this.btnSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSave.Location = new System.Drawing.Point(224, 379);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 27);
            this.btnSave.TabIndex = 10;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(318, 379);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 27);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // editHostname
            // 
            this.editHostname.Location = new System.Drawing.Point(219, 37);
            this.editHostname.Name = "editHostname";
            this.editHostname.Size = new System.Drawing.Size(163, 22);
            this.editHostname.TabIndex = 0;
            // 
            // editKeyFile
            // 
            this.editKeyFile.Location = new System.Drawing.Point(219, 190);
            this.editKeyFile.Name = "editKeyFile";
            this.editKeyFile.Size = new System.Drawing.Size(163, 22);
            this.editKeyFile.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(38, 225);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(176, 23);
            this.label3.TabIndex = 13;
            this.label3.Text = "Local source path:";
            // 
            // editLocalSrcPath
            // 
            this.editLocalSrcPath.Location = new System.Drawing.Point(218, 226);
            this.editLocalSrcPath.Name = "editLocalSrcPath";
            this.editLocalSrcPath.Size = new System.Drawing.Size(163, 22);
            this.editLocalSrcPath.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(39, 262);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(176, 23);
            this.label4.TabIndex = 15;
            this.label4.Text = "Remote source path:";
            // 
            // editRemoteSrcPath
            // 
            this.editRemoteSrcPath.Location = new System.Drawing.Point(219, 263);
            this.editRemoteSrcPath.Name = "editRemoteSrcPath";
            this.editRemoteSrcPath.Size = new System.Drawing.Size(163, 22);
            this.editRemoteSrcPath.TabIndex = 14;
            // 
            // btnTestConnect
            // 
            this.btnTestConnect.BackColor = System.Drawing.Color.DarkOrange;
            this.btnTestConnect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnTestConnect.Location = new System.Drawing.Point(40, 322);
            this.btnTestConnect.Name = "btnTestConnect";
            this.btnTestConnect.Size = new System.Drawing.Size(157, 27);
            this.btnTestConnect.TabIndex = 16;
            this.btnTestConnect.Text = "Test connection";
            this.btnTestConnect.UseVisualStyleBackColor = false;
            this.btnTestConnect.Click += new System.EventHandler(this.btnTestConnect_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(37, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(176, 23);
            this.label1.TabIndex = 18;
            this.label1.Text = "Port number:";
            // 
            // editPort
            // 
            this.editPort.Location = new System.Drawing.Point(219, 76);
            this.editPort.MaxLength = 6;
            this.editPort.Name = "editPort";
            this.editPort.Size = new System.Drawing.Size(163, 22);
            this.editPort.TabIndex = 2;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // editLog
            // 
            this.editLog.Enabled = false;
            this.editLog.Location = new System.Drawing.Point(203, 322);
            this.editLog.MinimumSize = new System.Drawing.Size(200, 48);
            this.editLog.Multiline = true;
            this.editLog.Name = "editLog";
            this.editLog.ReadOnly = true;
            this.editLog.Size = new System.Drawing.Size(216, 48);
            this.editLog.TabIndex = 19;
            // 
            // checkNoTransferExcludedFiles
            // 
            this.checkNoTransferExcludedFiles.AutoSize = true;
            this.checkNoTransferExcludedFiles.Location = new System.Drawing.Point(40, 295);
            this.checkNoTransferExcludedFiles.Name = "checkNoTransferExcludedFiles";
            this.checkNoTransferExcludedFiles.Size = new System.Drawing.Size(392, 21);
            this.checkNoTransferExcludedFiles.TabIndex = 20;
            this.checkNoTransferExcludedFiles.Text = "Do not transfer files and directories not included in project";
            this.checkNoTransferExcludedFiles.UseVisualStyleBackColor = true;
            // 
            // ConnectionEntryForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(626, 408);
            this.Controls.Add(this.checkNoTransferExcludedFiles);
            this.Controls.Add(this.editLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.editPort);
            this.Controls.Add(this.btnTestConnect);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.editRemoteSrcPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.editLocalSrcPath);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnPickKeyFile);
            this.Controls.Add(this.editKeyFile);
            this.Controls.Add(this.checkUseKey);
            this.Controls.Add(this.labelPassword);
            this.Controls.Add(this.editPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.editUsername);
            this.Controls.Add(this.labelHostname);
            this.Controls.Add(this.editHostname);
            this.Name = "ConnectionEntryForm";
            this.Text = "ConnectionSettings";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ConnectionEntryForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox editHostname;
        private System.Windows.Forms.Label labelHostname;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox editUsername;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox editPassword;
        private System.Windows.Forms.CheckBox checkUseKey;
        private System.Windows.Forms.OpenFileDialog dlgOpenKeyFile;
        private System.Windows.Forms.Button btnPickKeyFile;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox editKeyFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox editLocalSrcPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox editRemoteSrcPath;
        private System.Windows.Forms.Button btnTestConnect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox editPort;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.TextBox editLog;
        private System.Windows.Forms.CheckBox checkNoTransferExcludedFiles;
    }
}