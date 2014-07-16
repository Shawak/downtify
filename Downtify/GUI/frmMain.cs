using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

using System.Web;
using System.Net;
using System.IO;

namespace Downtify.GUI
{
    public partial class frmMain : Form
    {
        SpotifyDownloader downloader;
        public static StreamWriter sw;
        public static List<String> files = new List<String>();
        public static String playlistName;
        public static int playlistLength = 0;
        public static int current = 1;

        public frmMain()
        {
            InitializeComponent();

            downloader = new SpotifyDownloader();
            downloader.OnLoginResult += OnLoginResult;
            downloader.OnDownloadComplete += downloader_OnDownloadComplete;
            downloader.OnDownloadProgress += downloader_OnDownloadProgress;
            placeholderTextBox1.Enabled = false;
            placeholderTextBox1.Text = "Downloading: Nothing";
        }

        private void downloader_showError()
        {
            MessageBox.Show("Error Skipped!");
        }

        // Very ugly, todo: move parts of this to the downloader class
        private void downloader_OnPlaylistCreate()
        {
            var list = new object[listBoxTracks.SelectedItems.Count];
            for (int i = 1; i < listBoxTracks.SelectedItems.Count; i++)
                list[i - 1] = listBoxTracks.SelectedItems[i];

            listBoxTracks.SelectedItems.Clear();

            foreach (var track in list)
                listBoxTracks.SelectedItems.Add(track);

            if (listBoxTracks.SelectedItems.Count == 0)
            {
                CreatePlayList(playlistName, files);
                listBoxTracks.SelectedItems.Clear();
                MessageBox.Show("PLAYLIST CREATED!");
                placeholderTextBox1.Text = "Playlist created: " + playlistName + " (" + playlistLength + " Track/s)";
                EnableControls(true);
                return;
            }


            SpotifySharp.Track item = ((TrackItem)listBoxTracks.SelectedItems[0]).Track;
            string album = item.Album().Name();
            album = SpotifyDownloader.filterForFileName(album);
            var dir = SpotifyDownloader.downloadPath + album + "\\";
            String fileName = dir + SpotifyDownloader.GetTrackFullName(item) + ".mp3";


            files.Add(fileName);

            downloader_OnPlaylistCreate();
            SpotifyDownloader.LogString("Track added to Playlist! Path: " + fileName + " Track Name:" + SpotifyDownloader.GetTrackFullName(item));

            return;
        }


        // Very ugly, todo: move parts of this to the downloader class
        private void downloader_OnDownloadComplete()
        {
            current = current + 1;

            var list = new object[listBoxTracks.SelectedItems.Count];
            for (int i = 1; i < listBoxTracks.SelectedItems.Count; i++)
                list[i - 1] = listBoxTracks.SelectedItems[i];

            listBoxTracks.SelectedItems.Clear();

            foreach (var track in list)
                listBoxTracks.SelectedItems.Add(track);

            if (listBoxTracks.SelectedItems.Count == 0)
            {
                listBoxTracks.SelectedItems.Clear();
                MessageBox.Show("DONE");
                EnableControls(true);
                current = 0;
                playlistLength = 0;
                return;
            }


            SpotifySharp.Track item = ((TrackItem)listBoxTracks.SelectedItems[0]).Track;
            string album = item.Album().Name();
            album = SpotifyDownloader.filterForFileName(album);
            var dir = SpotifyDownloader.downloadPath + album + "\\";
            String fileName = dir + SpotifyDownloader.GetTrackFullName(item) +".mp3";


            
            if(File.Exists(fileName)) {
                downloader_OnDownloadComplete();
                SpotifyDownloader.LogString("Track skipped because the track already exists! Path: " + fileName + " Track Name:" + SpotifyDownloader.GetTrackFullName(item));
                return;
            }

            downloader.Download(((TrackItem)listBoxTracks.SelectedItems[0]).Track);
            placeholderTextBox1.Text = "Downloading: " + SpotifyDownloader.GetTrackFullName(item) + " (" + current + "/" + playlistLength + " Track/s)";

        }

        private void CreatePlayList(String name, List<String> files)
        {
            if (String.IsNullOrEmpty(name))
                return;


            // Open a file to write
            string sFileName = name + ".wpl";
            FileStream fs = File.Create(sFileName);
            sw = new StreamWriter(fs);
            try
            {
              
                sw.WriteLine("<?wpl version=\"1.0\"?>");    // File Header
                sw.WriteLine("<smil>");                     // Start of File Tag

                sw.WriteLine("\t<head>");                     // Playlist File Header Information Start Tag
                sw.WriteLine("\t\t<meta name=\"Generator\" content=\"Microsoft Windows Media Player -- 10.0.0.4036\"/>");
                sw.WriteLine("\t\t<title>" + name + "</title>");
                sw.WriteLine("\t</head>");                    // Playlist File Header Information End Tag

                sw.WriteLine("\t<body>");                     // Start of body Tag
                sw.WriteLine("\t\t<seq>");                      // Start of filelist Tag
                foreach(String s in files)
                    ProcessFile(s);

                sw.WriteLine("\t\t</seq>");                      // End of filelist Tag
                sw.WriteLine("\t</body>");                    // End of body Tag
                sw.WriteLine("</smil>");                    // End of File Tag
                sFileName = sFileName + " Successfully created.";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Create Playlist: Error");
                sFileName = sFileName + " Unsuccessful.";
            }
            finally
            {
                sw.Close();
                fs.Close();
            }
        }

        public static void ProcessFile(string path)
        {
            if (String.IsNullOrEmpty(path))
                return;
            string fileLine = "";
            fileLine = "\t\t\t<media src=\"";
            fileLine = fileLine + path + "\"/>";
            sw.WriteLine(fileLine);
        }


        private void downloader_OnDownloadProgress(int value)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (value > 100 || value < 0)
                    return;

                 progressBar1.Value = value;
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
                else if(line.Contains("password"))
                    password = line.Split('"')[1].Split('"')[0];
            }

            downloader.Login(username, password);
        }

        private void OnLoginResult(bool isLoggedIn)
        {
            if (!isLoggedIn)
            {
                MessageBox.Show("Error Login in!");
                Application.Exit();
                return;
            }

            EnableControls(true);
        }

        private void EnableControls(bool enable)
        {
            foreach (var control in this.Controls)
                ((Control)control).Enabled = enable;
        }

        private async  void textBoxLink_TextChanged(object sender, EventArgs e)
        {
            var link = textBoxLink.Text;
            try
            {
                EnableControls(false);
                if (link.ToLower().Contains("playlist"))
                {
                    var playlist = await downloader.FetchPlaylist(textBoxLink.Text);
                    playlistName = playlist.Name();
                    playlistLength = playlist.NumTracks();
                   

                    for (int i = 0; i < playlist.NumTracks(); i++) {
                        if (SpotifyDownloader.canPlay(playlist.Track(i)))
                        {
                            listBoxTracks.Items.Add(new TrackItem(playlist.Track(i)));
                        }
                        else
                        {
                            SpotifyDownloader.LogString("Track " + playlist.Track(i).Name() + " skipped, could not be played!");
                        }
                    }
                    textBoxLink.Clear();
                    placeholderTextBox1.Text = "Loaded Playlist: " + playlist.Name() + " (" + playlist.NumTracks() + " Track/s)";
                }
                else if (link.ToLower().Contains("track"))
                {
                    var track = await downloader.FetchTrack(textBoxLink.Text);
                    listBoxTracks.Items.Add(new TrackItem(track));
                    textBoxLink.Clear();
                    placeholderTextBox1.Text = "Loaded Track: " + track.Name() + " (1 Track)";
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

            EnableControls(false);
            SpotifySharp.Track item = ((TrackItem)listBoxTracks.SelectedItems[0]).Track;
            string album = item.Album().Name();
            album = SpotifyDownloader.filterForFileName(album);
            var dir = SpotifyDownloader.downloadPath + album + "\\";
            String fileName = dir + SpotifyDownloader.GetTrackFullName(item) + ".mp3";

            if (File.Exists(fileName))
            {
                SpotifyDownloader.LogString("Track skipped because the track already exists! Path: " + fileName + " Track Name:" + SpotifyDownloader.GetTrackFullName(item));
                downloader_OnDownloadComplete();
                return;
            }
            placeholderTextBox1.Text = "Downloading: " + SpotifyDownloader.GetTrackFullName(((TrackItem)listBoxTracks.SelectedItems[0]).Track) + " (" + current + "/" + playlistLength + " Track/s)";
            downloader.Download(((TrackItem)listBoxTracks.SelectedItems[0]).Track);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBoxTracks.SelectedItems.Count == 0)
                return;

            EnableControls(false);
            downloader_OnPlaylistCreate();
        }

        private void placeholderTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
