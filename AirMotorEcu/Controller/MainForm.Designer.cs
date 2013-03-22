namespace AirMotorEcuController
{
    partial class MainForm
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
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._connectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._disconnectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._statusBar = new System.Windows.Forms.StatusStrip();
            this._connectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this._packetReceived = new System.Windows.Forms.ToolStripStatusLabel();
            this._time = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this._rpms = new System.Windows.Forms.Label();
            this._airOn = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this._airOnValue = new System.Windows.Forms.Label();
            this._airOffValue = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._airOff = new System.Windows.Forms.TrackBar();
            this._exhaustOffValue = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this._exhaustOff = new System.Windows.Forms.TrackBar();
            this._exhaustOnValue = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this._exhaustOn = new System.Windows.Forms.TrackBar();
            this._sendParameters = new System.Windows.Forms.Button();
            this._timer = new System.Windows.Forms.Timer(this.components);
            this._writeSettings = new System.Windows.Forms.Button();
            this._menuStrip.SuspendLayout();
            this._statusBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._airOn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._airOff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._exhaustOff)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._exhaustOn)).BeginInit();
            this.SuspendLayout();
            // 
            // _menuStrip
            // 
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "_menuStrip";
            this._menuStrip.Size = new System.Drawing.Size(359, 24);
            this._menuStrip.TabIndex = 0;
            this._menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._connectMenuItem,
            this._disconnectMenuItem,
            this.toolStripSeparator1,
            this._exitMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // _connectMenuItem
            // 
            this._connectMenuItem.Name = "_connectMenuItem";
            this._connectMenuItem.Size = new System.Drawing.Size(137, 22);
            this._connectMenuItem.Text = "&Connect";
            this._connectMenuItem.Click += new System.EventHandler(this._connectMenuItem_Click);
            // 
            // _disconnectMenuItem
            // 
            this._disconnectMenuItem.Name = "_disconnectMenuItem";
            this._disconnectMenuItem.Size = new System.Drawing.Size(137, 22);
            this._disconnectMenuItem.Text = "&Disconnect";
            this._disconnectMenuItem.Click += new System.EventHandler(this._disconnectMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(134, 6);
            // 
            // _exitMenuItem
            // 
            this._exitMenuItem.Name = "_exitMenuItem";
            this._exitMenuItem.Size = new System.Drawing.Size(137, 22);
            this._exitMenuItem.Text = "E&xit";
            this._exitMenuItem.Click += new System.EventHandler(this._exitMenuItem_Click);
            // 
            // _statusBar
            // 
            this._statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._connectionStatus,
            this._packetReceived,
            this._time});
            this._statusBar.Location = new System.Drawing.Point(0, 345);
            this._statusBar.Name = "_statusBar";
            this._statusBar.Size = new System.Drawing.Size(359, 22);
            this._statusBar.TabIndex = 1;
            this._statusBar.Text = "statusStrip1";
            // 
            // _connectionStatus
            // 
            this._connectionStatus.Name = "_connectionStatus";
            this._connectionStatus.Size = new System.Drawing.Size(192, 17);
            this._connectionStatus.Spring = true;
            this._connectionStatus.Text = "UNKNOWN";
            this._connectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _packetReceived
            // 
            this._packetReceived.Name = "_packetReceived";
            this._packetReceived.Size = new System.Drawing.Size(83, 17);
            this._packetReceived.Text = "PacketReceived";
            // 
            // _time
            // 
            this._time.Name = "_time";
            this._time.Size = new System.Drawing.Size(69, 17);
            this._time.Text = "Current Time";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "RPMs";
            // 
            // _rpms
            // 
            this._rpms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._rpms.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._rpms.Location = new System.Drawing.Point(15, 46);
            this._rpms.Name = "_rpms";
            this._rpms.Size = new System.Drawing.Size(112, 38);
            this._rpms.TabIndex = 3;
            this._rpms.Text = "0";
            this._rpms.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _airOn
            // 
            this._airOn.Location = new System.Drawing.Point(136, 49);
            this._airOn.Maximum = 3000;
            this._airOn.Name = "_airOn";
            this._airOn.Orientation = System.Windows.Forms.Orientation.Vertical;
            this._airOn.Size = new System.Drawing.Size(45, 176);
            this._airOn.TabIndex = 4;
            this._airOn.TickFrequency = 100;
            this._airOn.TickStyle = System.Windows.Forms.TickStyle.Both;
            this._airOn.Value = 10;
            this._airOn.Scroll += new System.EventHandler(this._airOn_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(133, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Air On";
            // 
            // _airOnValue
            // 
            this._airOnValue.AutoSize = true;
            this._airOnValue.Location = new System.Drawing.Point(143, 228);
            this._airOnValue.Name = "_airOnValue";
            this._airOnValue.Size = new System.Drawing.Size(13, 13);
            this._airOnValue.TabIndex = 6;
            this._airOnValue.Text = "0";
            // 
            // _airOffValue
            // 
            this._airOffValue.AutoSize = true;
            this._airOffValue.Location = new System.Drawing.Point(194, 228);
            this._airOffValue.Name = "_airOffValue";
            this._airOffValue.Size = new System.Drawing.Size(13, 13);
            this._airOffValue.TabIndex = 9;
            this._airOffValue.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(184, 33);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Air Off";
            // 
            // _airOff
            // 
            this._airOff.Location = new System.Drawing.Point(187, 49);
            this._airOff.Maximum = 3000;
            this._airOff.Name = "_airOff";
            this._airOff.Orientation = System.Windows.Forms.Orientation.Vertical;
            this._airOff.Size = new System.Drawing.Size(45, 176);
            this._airOff.TabIndex = 7;
            this._airOff.TickFrequency = 100;
            this._airOff.TickStyle = System.Windows.Forms.TickStyle.Both;
            this._airOff.Value = 700;
            this._airOff.Scroll += new System.EventHandler(this._airOff_Scroll);
            // 
            // _exhaustOffValue
            // 
            this._exhaustOffValue.AutoSize = true;
            this._exhaustOffValue.Location = new System.Drawing.Point(296, 228);
            this._exhaustOffValue.Name = "_exhaustOffValue";
            this._exhaustOffValue.Size = new System.Drawing.Size(13, 13);
            this._exhaustOffValue.TabIndex = 15;
            this._exhaustOffValue.Text = "0";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(286, 33);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Ex. Off";
            // 
            // _exhaustOff
            // 
            this._exhaustOff.Location = new System.Drawing.Point(289, 49);
            this._exhaustOff.Maximum = 3000;
            this._exhaustOff.Name = "_exhaustOff";
            this._exhaustOff.Orientation = System.Windows.Forms.Orientation.Vertical;
            this._exhaustOff.Size = new System.Drawing.Size(45, 176);
            this._exhaustOff.TabIndex = 13;
            this._exhaustOff.TickFrequency = 100;
            this._exhaustOff.TickStyle = System.Windows.Forms.TickStyle.Both;
            this._exhaustOff.Value = 1400;
            this._exhaustOff.Scroll += new System.EventHandler(this._exhaustOff_Scroll);
            // 
            // _exhaustOnValue
            // 
            this._exhaustOnValue.AutoSize = true;
            this._exhaustOnValue.Location = new System.Drawing.Point(246, 228);
            this._exhaustOnValue.Name = "_exhaustOnValue";
            this._exhaustOnValue.Size = new System.Drawing.Size(13, 13);
            this._exhaustOnValue.TabIndex = 12;
            this._exhaustOnValue.Text = "0";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(235, 33);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Ex. On";
            // 
            // _exhaustOn
            // 
            this._exhaustOn.Location = new System.Drawing.Point(238, 49);
            this._exhaustOn.Maximum = 3000;
            this._exhaustOn.Name = "_exhaustOn";
            this._exhaustOn.Orientation = System.Windows.Forms.Orientation.Vertical;
            this._exhaustOn.Size = new System.Drawing.Size(45, 176);
            this._exhaustOn.TabIndex = 10;
            this._exhaustOn.TickFrequency = 100;
            this._exhaustOn.TickStyle = System.Windows.Forms.TickStyle.Both;
            this._exhaustOn.Value = 701;
            this._exhaustOn.Scroll += new System.EventHandler(this._exhaustOn_Scroll);
            // 
            // _sendParameters
            // 
            this._sendParameters.Location = new System.Drawing.Point(136, 254);
            this._sendParameters.Name = "_sendParameters";
            this._sendParameters.Size = new System.Drawing.Size(112, 23);
            this._sendParameters.TabIndex = 16;
            this._sendParameters.Text = "Send Parameters";
            this._sendParameters.UseVisualStyleBackColor = true;
            this._sendParameters.Click += new System.EventHandler(this._sendParameters_Click);
            // 
            // _timer
            // 
            this._timer.Enabled = true;
            this._timer.Interval = 500;
            this._timer.Tick += new System.EventHandler(this._timer_Tick);
            // 
            // _writeSettings
            // 
            this._writeSettings.Location = new System.Drawing.Point(12, 87);
            this._writeSettings.Name = "_writeSettings";
            this._writeSettings.Size = new System.Drawing.Size(100, 23);
            this._writeSettings.TabIndex = 17;
            this._writeSettings.Text = "Log Readings";
            this._writeSettings.UseVisualStyleBackColor = true;
            this._writeSettings.Click += new System.EventHandler(this._writeSettings_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 367);
            this.Controls.Add(this._writeSettings);
            this.Controls.Add(this._sendParameters);
            this.Controls.Add(this._exhaustOffValue);
            this.Controls.Add(this.label6);
            this.Controls.Add(this._exhaustOff);
            this.Controls.Add(this._exhaustOnValue);
            this.Controls.Add(this.label8);
            this.Controls.Add(this._exhaustOn);
            this.Controls.Add(this._airOffValue);
            this.Controls.Add(this.label4);
            this.Controls.Add(this._airOff);
            this.Controls.Add(this._airOnValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._airOn);
            this.Controls.Add(this._rpms);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._statusBar);
            this.Controls.Add(this._menuStrip);
            this.MainMenuStrip = this._menuStrip;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this._statusBar.ResumeLayout(false);
            this._statusBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._airOn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._airOff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._exhaustOff)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._exhaustOn)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _connectMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _disconnectMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.StatusStrip _statusBar;
        private System.Windows.Forms.ToolStripStatusLabel _connectionStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label _rpms;
        private System.Windows.Forms.TrackBar _airOn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label _airOnValue;
        private System.Windows.Forms.Label _airOffValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar _airOff;
        private System.Windows.Forms.Label _exhaustOffValue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TrackBar _exhaustOff;
        private System.Windows.Forms.Label _exhaustOnValue;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar _exhaustOn;
        private System.Windows.Forms.Button _sendParameters;
        private System.Windows.Forms.ToolStripStatusLabel _packetReceived;
        private System.Windows.Forms.Timer _timer;
        private System.Windows.Forms.ToolStripStatusLabel _time;
        private System.Windows.Forms.Button _writeSettings;
    }
}