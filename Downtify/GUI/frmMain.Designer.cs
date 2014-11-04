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
            this.buttonClearDownloads = new System.Windows.Forms.Button();
            this.checkBoxShutdown = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressBarTotal = new System.Windows.Forms.ProgressBar();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.textBoxLink = new Downtify.GUI.PlaceholderTextBox();
            this.SuspendLayout();
            // 
            // listBoxTracks
            // 
            this.listBoxTracks.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxTracks.FormattingEnabled = true;
            this.listBoxTracks.ItemHeight = 16;
            this.listBoxTracks.Location = new System.Drawing.Point(11, 40);
            this.listBoxTracks.Name = "listBoxTracks";
            this.listBoxTracks.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxTracks.Size = new System.Drawing.Size(494, 196);
            this.listBoxTracks.TabIndex = 0;
            this.listBoxTracks.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxTracks_KeyDown);
            // 
            // buttonDownload
            // 
            this.buttonDownload.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDownload.Location = new System.Drawing.Point(402, 241);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(102, 23);
            this.buttonDownload.TabIndex = 3;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.buttonDownload_Click);
            // 
            // buttonClearDownloads
            // 
            this.buttonClearDownloads.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonClearDownloads.Location = new System.Drawing.Point(349, 241);
            this.buttonClearDownloads.Name = "buttonClearDownloads";
            this.buttonClearDownloads.Size = new System.Drawing.Size(52, 23);
            this.buttonClearDownloads.TabIndex = 2;
            this.buttonClearDownloads.Text = "Clear";
            this.buttonClearDownloads.UseVisualStyleBackColor = true;
            this.buttonClearDownloads.Click += new System.EventHandler(this.buttonClearDownloads_Click);
            // 
            // checkBoxShutdown
            // 
            this.checkBoxShutdown.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxShutdown.Location = new System.Drawing.Point(349, 270);
            this.checkBoxShutdown.Name = "checkBoxShutdown";
            this.checkBoxShutdown.Size = new System.Drawing.Size(106, 23);
            this.checkBoxShutdown.TabIndex = 4;
            this.checkBoxShutdown.Text = "Shutdown";
            this.checkBoxShutdown.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 241);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(331, 23);
            this.progressBar1.TabIndex = 5;
            // 
            // progressBarTotal
            // 
            this.progressBarTotal.Location = new System.Drawing.Point(12, 270);
            this.progressBarTotal.Name = "progressBarTotal";
            this.progressBarTotal.Size = new System.Drawing.Size(331, 23);
            this.progressBarTotal.TabIndex = 6;
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.buttonSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonSelectAll.Location = new System.Drawing.Point(485, 40);
            this.buttonSelectAll.Margin = new System.Windows.Forms.Padding(0);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(20, 20);
            this.buttonSelectAll.TabIndex = 7;
            this.buttonSelectAll.Text = "☑";
            this.buttonSelectAll.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonSelectAll.UseVisualStyleBackColor = false;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // textBoxLink
            // 
            this.textBoxLink.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLink.Location = new System.Drawing.Point(12, 12);
            this.textBoxLink.Name = "textBoxLink";
            this.textBoxLink.Placeholder = "Put your track or playlist link here";
            this.textBoxLink.Size = new System.Drawing.Size(493, 22);
            this.textBoxLink.TabIndex = 1;
            this.textBoxLink.TextChanged += new System.EventHandler(this.textBoxLink_TextChanged);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(517, 300);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.progressBarTotal);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.buttonClearDownloads);
            this.Controls.Add(this.checkBoxShutdown);
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
		private System.Windows.Forms.CheckBox checkBoxShutdown;
        private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.ProgressBar progressBarTotal;
		private System.Windows.Forms.Button buttonClearDownloads;
        private System.Windows.Forms.Button buttonSelectAll;
    }
}

