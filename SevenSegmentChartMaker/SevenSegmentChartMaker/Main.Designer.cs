namespace SevenSegmentChartMaker
{
    partial class Main
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
            this._character = new System.Windows.Forms.ComboBox();
            this._save = new System.Windows.Forms.Button();
            this._load = new System.Windows.Forms.Button();
            this._sevenSegment = new SevenSegmentChartMaker.SevenSegment();
            this.SuspendLayout();
            // 
            // _character
            // 
            this._character.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._character.FormattingEnabled = true;
            this._character.Location = new System.Drawing.Point(12, 12);
            this._character.Name = "_character";
            this._character.Size = new System.Drawing.Size(121, 21);
            this._character.TabIndex = 0;
            this._character.SelectedIndexChanged += new System.EventHandler(this._character_SelectedIndexChanged);
            // 
            // _save
            // 
            this._save.Location = new System.Drawing.Point(179, 51);
            this._save.Name = "_save";
            this._save.Size = new System.Drawing.Size(75, 23);
            this._save.TabIndex = 2;
            this._save.Text = "Save...";
            this._save.UseVisualStyleBackColor = true;
            this._save.Click += new System.EventHandler(this._save_Click);
            // 
            // _load
            // 
            this._load.Location = new System.Drawing.Point(179, 80);
            this._load.Name = "_load";
            this._load.Size = new System.Drawing.Size(75, 23);
            this._load.TabIndex = 3;
            this._load.Text = "Load...";
            this._load.UseVisualStyleBackColor = true;
            this._load.Click += new System.EventHandler(this._load_Click);
            // 
            // _sevenSegment
            // 
            this._sevenSegment.Location = new System.Drawing.Point(42, 39);
            this._sevenSegment.MaximumSize = new System.Drawing.Size(57, 97);
            this._sevenSegment.MinimumSize = new System.Drawing.Size(57, 97);
            this._sevenSegment.Name = "_sevenSegment";
            this._sevenSegment.Size = new System.Drawing.Size(57, 97);
            this._sevenSegment.TabIndex = 1;
            this._sevenSegment.Value = ((byte)(64));
            this._sevenSegment.Change += new System.EventHandler(this._sevenSegment_Change);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(286, 154);
            this.Controls.Add(this._load);
            this.Controls.Add(this._save);
            this.Controls.Add(this._sevenSegment);
            this.Controls.Add(this._character);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Main";
            this.Text = "Seven Segment Chart Maker";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox _character;
        private SevenSegment _sevenSegment;
        private System.Windows.Forms.Button _save;
        private System.Windows.Forms.Button _load;
    }
}

