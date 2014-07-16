namespace Downtify.GUI
{
    partial class frmMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBoxTracks = new System.Windows.Forms.ListBox();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button1 = new System.Windows.Forms.Button();
            this.placeholderTextBox1 = new System.Windows.Forms.TextBox();
            this.textBoxLink = new Downtify.GUI.PlaceholderTextBox();
            this.SuspendLayout();
            // 
            // listBoxTracks
            // 
            this.listBoxTracks.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxTracks.FormattingEnabled = true;
            this.listBoxTracks.ItemHeight = 16;
            this.listBoxTracks.Location = new System.Drawing.Point(12, 40);
            this.listBoxTracks.Name = "listBoxTracks";
            this.listBoxTracks.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxTracks.Size = new System.Drawing.Size(896, 372);
            this.listBoxTracks.TabIndex = 0;
            this.listBoxTracks.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxTracks_KeyDown);
            // 
            // buttonDownload
            // 
            this.buttonDownload.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDownload.Location = new System.Drawing.Point(794, 427);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(113, 23);
            this.buttonDownload.TabIndex = 2;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(11, 427);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(776, 23);
            this.progressBar1.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(776, 456);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(131, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Generate Playlist";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // placeholderTextBox1
            // 
            this.placeholderTextBox1.Enabled = false;
            this.placeholderTextBox1.HideSelection = false;
            this.placeholderTextBox1.Location = new System.Drawing.Point(11, 456);
            this.placeholderTextBox1.Name = "placeholderTextBox1";
            this.placeholderTextBox1.ReadOnly = true;
            this.placeholderTextBox1.Size = new System.Drawing.Size(758, 20);
            this.placeholderTextBox1.TabIndex = 6;
            // 
            // textBoxLink
            // 
            this.textBoxLink.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLink.Location = new System.Drawing.Point(12, 12);
            this.textBoxLink.Name = "textBoxLink";
            this.textBoxLink.Placeholder = "Put your track or playlist link here";
            this.textBoxLink.Size = new System.Drawing.Size(895, 22);
            this.textBoxLink.TabIndex = 1;
            this.textBoxLink.TextChanged += new System.EventHandler(this.textBoxLink_TextChanged);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(919, 479);
            this.Controls.Add(this.placeholderTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.textBoxLink);
            this.Controls.Add(this.listBoxTracks);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Downtify";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxTracks;
        private Downtify.GUI.PlaceholderTextBox textBoxLink;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox placeholderTextBox1;
    }
}

