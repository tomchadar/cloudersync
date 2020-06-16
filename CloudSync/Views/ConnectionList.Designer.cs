namespace CloudSync.Views
{
    partial class ConnectionList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionList));
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.btnCreateConnection = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.connectionsGrid = new System.Windows.Forms.DataGridView();
            this.hostname = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.username = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.toolStripConnections = new System.Windows.Forms.ToolStrip();
            this.btnNewConnection = new System.Windows.Forms.ToolStripButton();
            this.btnEditConnection = new System.Windows.Forms.ToolStripButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.connectionsGrid)).BeginInit();
            this.toolStripConnections.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.AutoScroll = true;
            this.ContentPanel.Size = new System.Drawing.Size(620, 289);
            // 
            // btnCreateConnection
            // 
            this.btnCreateConnection.Location = new System.Drawing.Point(69, 238);
            this.btnCreateConnection.Name = "btnCreateConnection";
            this.btnCreateConnection.Size = new System.Drawing.Size(138, 31);
            this.btnCreateConnection.TabIndex = 0;
            this.btnCreateConnection.Text = "Create Connection";
            this.btnCreateConnection.UseVisualStyleBackColor = true;
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(519, 238);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(101, 39);
            this.OK.TabIndex = 1;
            this.OK.Text = "Done";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Desktop;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.connectionsGrid);
            this.panel1.Controls.Add(this.toolStripConnections);
            this.panel1.Location = new System.Drawing.Point(12, -3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(626, 235);
            this.panel1.TabIndex = 2;
            // 
            // connectionsGrid
            // 
            this.connectionsGrid.AllowUserToAddRows = false;
            this.connectionsGrid.AllowUserToDeleteRows = false;
            this.connectionsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.connectionsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.hostname,
            this.username});
            this.connectionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connectionsGrid.Location = new System.Drawing.Point(0, 0);
            this.connectionsGrid.Name = "connectionsGrid";
            this.connectionsGrid.ReadOnly = true;
            this.connectionsGrid.RowTemplate.Height = 24;
            this.connectionsGrid.Size = new System.Drawing.Size(485, 233);
            this.connectionsGrid.TabIndex = 1;
            // 
            // hostname
            // 
            this.hostname.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.hostname.HeaderText = "Hostname";
            this.hostname.Name = "hostname";
            this.hostname.ReadOnly = true;
            // 
            // username
            // 
            this.username.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.username.HeaderText = "Username";
            this.username.Name = "username";
            this.username.ReadOnly = true;
            // 
            // toolStripConnections
            // 
            this.toolStripConnections.Dock = System.Windows.Forms.DockStyle.Right;
            this.toolStripConnections.ImageScalingSize = new System.Drawing.Size(19, 19);
            this.toolStripConnections.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNewConnection,
            this.btnEditConnection});
            this.toolStripConnections.Location = new System.Drawing.Point(485, 0);
            this.toolStripConnections.Name = "toolStripConnections";
            this.toolStripConnections.Size = new System.Drawing.Size(139, 233);
            this.toolStripConnections.TabIndex = 0;
            this.toolStripConnections.Text = "Connection Actions";
            // 
            // btnNewConnection
            // 
            this.btnNewConnection.Image = ((System.Drawing.Image)(resources.GetObject("btnNewConnection.Image")));
            this.btnNewConnection.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnNewConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNewConnection.Name = "btnNewConnection";
            this.btnNewConnection.Size = new System.Drawing.Size(136, 24);
            this.btnNewConnection.Text = "New Connection";
            this.btnNewConnection.Click += new System.EventHandler(this.btnNewConnection_Click);
            // 
            // btnEditConnection
            // 
            this.btnEditConnection.Enabled = false;
            this.btnEditConnection.Image = ((System.Drawing.Image)(resources.GetObject("btnEditConnection.Image")));
            this.btnEditConnection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditConnection.Name = "btnEditConnection";
            this.btnEditConnection.Size = new System.Drawing.Size(136, 24);
            this.btnEditConnection.Text = "Edit Connection";
            // 
            // ConnectionList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 289);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.btnCreateConnection);
            this.Name = "ConnectionList";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connection List";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.connectionsGrid)).EndInit();
            this.toolStripConnections.ResumeLayout(false);
            this.toolStripConnections.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCreateConnection;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStrip toolStripConnections;
        private System.Windows.Forms.ToolStripButton btnNewConnection;
        private System.Windows.Forms.ToolStripButton btnEditConnection;
        private System.Windows.Forms.DataGridView connectionsGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn hostname;
        private System.Windows.Forms.DataGridViewTextBoxColumn username;
    }
}