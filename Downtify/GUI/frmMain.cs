﻿using System;
using System.IO;
using System.Windows.Forms;

namespace Downtify.GUI
{
    public partial class frmMain : Form
    {
        SpotifyDownloader downloader;
        bool shutdown;
        int downloadCount = 0;
        int totalProgressTemp;
        public frmMain()
        {
            InitializeComponent();

            downloader = new SpotifyDownloader();
            downloader.OnLoginResult += OnLoginResult;
            downloader.OnDownloadComplete += downloader_OnDownloadComplete;
            downloader.OnDownloadProgress += downloader_OnDownloadProgress;


        }

        // Very ugly, todo: move parts of this to the downloader class
        private void downloader_OnDownloadComplete(bool successfully)
        {
            var list = new object[listBoxTracks.SelectedItems.Count];
            for (int i = 1; i < listBoxTracks.SelectedItems.Count; i++)
                list[i - 1] = listBoxTracks.SelectedItems[i];

            listBoxTracks.SelectedItems.Clear();

            foreach (var track in list)
                listBoxTracks.SelectedItems.Add(track);

            if (listBoxTracks.SelectedItems.Count == 0)
            {
                listBoxTracks.SelectedItems.Clear();
                this.shutdown = checkBoxShutdown.Checked;
                if (this.shutdown)
                {
                    System.Diagnostics.Process.Start("shutdown", "/s /t 60");
                }
                progressBarTotal.Value = 100;
                MessageBox.Show("DONE");

                EnableControls(true);
                return;
            }



            downloader.Download(((TrackItem)listBoxTracks.SelectedItems[0]).Track);
        }

        private void downloader_OnDownloadProgress(int value)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (value > 100 || value < 0)
                    return;

                progressBar1.Value = value;

                //totalProgressBar
                double maxProgress = 1 / (double)(this.downloadCount) * 100;

                int progress = (int)Math.Round((double)value / 100 * Math.Floor(maxProgress));

                if (progressBarTotal.Value + (progress - this.totalProgressTemp) <= 100)
                {
                    if (progress < this.totalProgressTemp)
                    {
                        progressBarTotal.Value += progress;
                    }
                    else
                    {
                        progressBarTotal.Value += progress - this.totalProgressTemp;
                    }


                }

                this.totalProgressTemp = progress;
            });
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            EnableControls(false);
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(200);
            this.Activate();

            // very ugly too, use config parser (json for example)
            string username = "", password = "";
            foreach (var line in File.ReadAllLines("config.txt"))
            {
                if (line.Contains("#"))
                    continue;

                if (line.Contains("username"))
                    username = line.Split('"')[1].Split('"')[0];
                else if (line.Contains("password"))
                    password = line.Split('"')[1].Split('"')[0];
            }

            downloader.Login(username, password);
        }

        private void OnLoginResult(bool isLoggedIn)
        {
            //MessageBox.Show("OnLoginResult"+isLoggedIn);
            if (!isLoggedIn)
            {
                MessageBox.Show("Error logging in!", "Error");
                Application.Exit();
                return;
            }

            EnableControls(true);
        }

        private void EnableControls(bool enable)
        {
            foreach (var control in this.Controls)
            {

                ((Control)control).Enabled = enable;
            }

        }

        private async void textBoxLink_TextChanged(object sender, EventArgs e)
        {
            var link = textBoxLink.Text;
            try
            {
                EnableControls(false);
                if (link.ToLower().Contains("playlist"))
                {
                    var playlist = await downloader.FetchPlaylist(textBoxLink.Text);
                    for (int i = 0; i < playlist.NumTracks(); i++)
                        listBoxTracks.Items.Add(new TrackItem(playlist.Track(i)));
                    textBoxLink.Clear();
                }
                else if (link.ToLower().Contains("track"))
                {
                    var track = await downloader.FetchTrack(textBoxLink.Text);
                    listBoxTracks.Items.Add(new TrackItem(track));
                    textBoxLink.Clear();
                }
            }
            catch (NullReferenceException)
            {
            }
            finally
            {
                EnableControls(true);
            }
        }

        private void listBoxTracks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBoxTracks.SelectedItems.Count == 0)
                    return;

                var list = new TrackItem[listBoxTracks.SelectedItems.Count];
                listBoxTracks.SelectedItems.CopyTo(list, 0);

                foreach (var track in list)
                    listBoxTracks.Items.Remove(track);
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                var list = new TrackItem[listBoxTracks.Items.Count];
                listBoxTracks.Items.CopyTo(list, 0);

                listBoxTracks.SelectedItems.Clear();
                foreach (var track in list)
                    listBoxTracks.SelectedItems.Add(track);
            }
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            if (listBoxTracks.SelectedItems.Count == 0)
                return;

            if (!downloader.IsDownloadFolderEmpty())
            {
                MessageBox.Show("Please empty the download directory before starting downloading.", "Error");
                return;
            }

            EnableControls(false);
            this.downloadCount = listBoxTracks.SelectedItems.Count;
            progressBarTotal.Value = 0;
            downloader.Download(((TrackItem)listBoxTracks.SelectedItems[0]).Track);
        }

        private void buttonClearDownloads_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Clearing download folder... Are you sure?", "Clear download folder", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {


                var dir = Directory.EnumerateFileSystemEntries(downloader.getDownloadFolder());

                foreach (var entry in dir)
                {
                    Directory.Delete(entry, true);
                }
            }
        }

    }
}
