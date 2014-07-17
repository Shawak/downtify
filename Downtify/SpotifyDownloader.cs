using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using SpotifySharp;
using HundredMilesSoftware.UltraID3Lib;

using WaveLib;
using Yeti.Lame;
using Yeti.MMedia;
using Yeti.MMedia.Mp3;
using System.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;

namespace Downtify
{
    public class TrackItem
    {
        public Track Track { get; private set; }

        public TrackItem(Track track)
        {
            this.Track = track;
        }

        public override string ToString()
        {
            return SpotifyDownloader.GetTrackFullName(Track);
        }
    }

    public class SpotifyDownloader : SpotifySessionListener
    {

        public static String playlistName = "";
        static String[] replace = { ":", "*", "?", "<", ">", "|", "/", "\"", "\\" };

        public static string GetTrackArtistsNames(Track track)
        {
            var ret = "";
            for (int i = 0; i < track.NumArtists(); i++)
                ret += (i != 0 ? ", " : "") + track.Artist(i).Name();
            return ret;
        }

        public static string GetTrackFullName(Track track)
        {
            return GetTrackArtistsNames(track) + " - " + track.Name();
        }

        // TODO: Make these become "real events"
        public delegate void OnLoginHandler(bool isLoggedIn);
        public event OnLoginHandler OnLoginResult;

        public delegate void OnDownloadProgressHandler(int value);
        public event OnDownloadProgressHandler OnDownloadProgress;

        public delegate void OnDownloadCompleteHandler();
        public event OnDownloadCompleteHandler OnDownloadComplete;

        public bool Loaded { get { return session.User().IsLoaded(); } }

        static SpotifySession session;
        Track downloadingTrack;
        Mp3Writer wr;
        SynchronizationContext syncContext;

        static string appPath = AppDomain.CurrentDomain.BaseDirectory;
        static string tmpPath = appPath + "cache\\";
        public static string downloadPath = appPath + "download\\";

        #region key

        // This key is not mine. It was found on a public website. Please don't misuse.

        static byte[] key = new byte[]
			{
				0x01, 0x56, 0xFC, 0x14, 0x35, 0x86, 0x20, 0xF1, 0x69, 0xC6, 0x9B, 0x7C, 0xD3, 0x11, 0xAB, 0x56,
				0x3E, 0x1F, 0xF3, 0xB1, 0x58, 0xD4, 0x07, 0xF3, 0x51, 0xCF, 0xC1, 0x1D, 0xF8, 0xCF, 0x49, 0x73,
				0x9F, 0xFC, 0x66, 0x02, 0xA1, 0xCE, 0x82, 0x08, 0xBE, 0xF3, 0x89, 0xAA, 0xBD, 0x75, 0x42, 0x19,
				0x60, 0x45, 0xBF, 0x39, 0x70, 0x8C, 0x6E, 0xA9, 0x37, 0xE1, 0x5B, 0x54, 0xD9, 0x29, 0x1D, 0xEE,
				0xBF, 0x2B, 0x11, 0xD2, 0xF0, 0x28, 0xF3, 0xD4, 0x1D, 0x26, 0x99, 0xA6, 0x8A, 0xC8, 0xA8, 0xAE,
				0xC1, 0x98, 0x87, 0x4B, 0x4A, 0xB9, 0xD6, 0x6A, 0x90, 0x51, 0xA0, 0x4D, 0x4D, 0xA5, 0xCB, 0x66,
				0xC8, 0x5D, 0x3F, 0xE8, 0x1B, 0x6E, 0x22, 0xFF, 0x4F, 0xA5, 0x5C, 0x06, 0x14, 0x25, 0xD0, 0x74,
				0xBD, 0x81, 0x48, 0xDE, 0x47, 0x69, 0x4D, 0xF4, 0xE5, 0x6E, 0xB8, 0x26, 0x3B, 0x06, 0xFE, 0x0D,
				0x84, 0x55, 0x3F, 0x37, 0x67, 0x11, 0x14, 0xF3, 0x4A, 0x17, 0xC0, 0x50, 0x9D, 0x48, 0x9D, 0x95,
				0x93, 0xB4, 0x27, 0xB6, 0x27, 0x51, 0x99, 0xCA, 0xA7, 0xB3, 0xE9, 0x1C, 0x3B, 0x89, 0x2A, 0xE7,
				0x18, 0xFF, 0xF6, 0xB6, 0xAE, 0xB2, 0x17, 0x5A, 0x33, 0x61, 0x08, 0x9D, 0xE3, 0x03, 0xFD, 0x7D,
				0x12, 0x68, 0x24, 0x6D, 0xCF, 0x6F, 0xA8, 0x87, 0x06, 0x27, 0xED, 0x4A, 0xB7, 0x13, 0x23, 0xAA,
				0x62, 0xA2, 0x21, 0xC0, 0x0E, 0x2F, 0xF3, 0x47, 0x1D, 0xFD, 0x3D, 0x06, 0x10, 0x7D, 0xA2, 0xFB,
				0x63, 0xF9, 0x04, 0x20, 0x20, 0xE7, 0x28, 0x6B, 0x6F, 0xD6, 0x7A, 0x61, 0x33, 0x76, 0x2A, 0xA4,
				0x3E, 0xEE, 0x40, 0xE8, 0x07, 0x99, 0xDA, 0xEA, 0x63, 0x65, 0x21, 0x22, 0x30, 0x0A, 0xF1, 0xD5,
				0x46, 0xAA, 0x8C, 0x06, 0x57, 0xB7, 0xB4, 0x8A, 0xDE, 0xFE, 0xA9, 0xB8, 0xA3, 0x03, 0xF0, 0xDB,
				0x4C, 0x38, 0xC0, 0x57, 0xC1, 0x47, 0xBD, 0xC7, 0x24, 0x7E, 0xBB, 0x37, 0xD2, 0xFA, 0x4D, 0x5F,
				0x03, 0x23, 0xC6, 0x53, 0xD9, 0x43, 0xCA, 0xDF, 0x84, 0x72, 0x1A, 0x06, 0xF1, 0x93, 0xAB, 0x2A,
				0x52, 0xAB, 0xEB, 0x79, 0x9F, 0x74, 0xBF, 0xE7, 0xAC, 0x95, 0xCB, 0x63, 0xCE, 0x18, 0x08, 0x99,
				0x19, 0x17, 0x36, 0x9D, 0x9C, 0x7E, 0x82, 0xDC, 0x83, 0xDC, 0xA8, 0x8D, 0x30, 0x2D, 0xF4, 0xC7,
				0xD6
			};

        #endregion

        public SpotifyDownloader()
        {
            if (!Directory.Exists(tmpPath))
                Directory.CreateDirectory(tmpPath);

            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            var config = new SpotifySessionConfig()
            {
                ApiVersion = 12,
                CacheLocation = tmpPath,
                SettingsLocation = tmpPath,
                ApplicationKey = File.ReadAllBytes("spotify_appkey.key"),
                UserAgent = "downtify",
                Listener = this
            };

            syncContext = SynchronizationContext.Current;
            session = SpotifySession.Create(config);
            Task.Factory.StartNew(() => InvokeProcessEvents());
        }

        private void InvokeProcessEvents()
        {
            syncContext.Post(obj => {
                int limit = 0;
                session.ProcessEvents(ref limit);
            }, null);
        }

        public void Login(string username, string password)
        {
            session.Login(username, password, true, null);
        }

        public override void NotifyMainThread(SpotifySession session)
        {
            InvokeProcessEvents();
            base.NotifyMainThread(session);
        }

        public override async void LoggedIn(SpotifySession session, SpotifyError error)
        {
            base.LoggedIn(session, error);
            await WaitForBool(session.User().IsLoaded);
            session.PreferredBitrate(BitRate._320k);
        }

        public override void ConnectionError(SpotifySession session, SpotifyError error)
        {
            base.ConnectionError(session, error);
        }

        public override void CredentialsBlobUpdated(SpotifySession session, string blob)
        {
            base.CredentialsBlobUpdated(session, blob);
        }

        public override void ConnectionstateUpdated(SpotifySession session)
        {
            if (session.Connectionstate() == ConnectionState.LoggedIn)
                if (OnLoginResult != null)
                    OnLoginResult(true);
            Console.WriteLine(session.Connectionstate().ToString());
            base.ConnectionstateUpdated(session);
        }

        public override void MetadataUpdated(SpotifySession session)
        {
            base.MetadataUpdated(session);
        }

        public override void UserinfoUpdated(SpotifySession session)
        {
            base.UserinfoUpdated(session);
        }

        public override void LogMessage(SpotifySession session, string data)
        {
            Console.Write(data);
            base.LogMessage(session, data);
        }

        public override void MessageToUser(SpotifySession session, string message)
        {
            base.MessageToUser(session, message);
        }

        public override void OfflineError(SpotifySession session, SpotifyError error)
        {
            base.OfflineError(session, error);
        }

        public override void LoggedOut(SpotifySession session)
        {
            base.LoggedOut(session);
        }

        public override void OfflineStatusUpdated(SpotifySession session)
        {
            base.OfflineStatusUpdated(session);
        }

        public override void ScrobbleError(SpotifySession session, SpotifyError error)
        {
            base.ScrobbleError(session, error);
        }

        public override void PrivateSessionModeChanged(SpotifySession session, bool is_private)
        {
            base.PrivateSessionModeChanged(session, is_private);
        }

        public override void GetAudioBufferStats(SpotifySession session, out AudioBufferStats stats)
        {
            base.GetAudioBufferStats(session, out stats);
        }

        int counter;

        public override int MusicDelivery(SpotifySession session, AudioFormat format, IntPtr frames, int num_frames)
        {
            try { 
            if (num_frames == 0)
                return 0;

            var size = num_frames * format.channels * 2;
            var data = new byte[size];
            Marshal.Copy(frames, data, 0, size);

            wr.Write(data);
            }
            catch (Exception e) { }

            if(OnDownloadProgress != null)
            {
                counter++;
                var duration = downloadingTrack.Duration();
                var process = (int)Math.Round((double)100 / duration * (46.4 * counter), 0);
                OnDownloadProgress(process);
            }

            return num_frames;
            // return base.MusicDelivery(session, format, frames, num_frames);
        }

        public override void StreamingError(SpotifySession session, SpotifyError error)
        {
            base.StreamingError(session, error);
        }

        public override void PlayTokenLost(SpotifySession session)
        {
            System.Windows.Forms.MessageBox.Show("Connection Lost");
            base.PlayTokenLost(session);
        }

        public override async void EndOfTrack(SpotifySession session)
        {
            session.PlayerPlay(false);
            wr.Close();

            // Move File
            string album = downloadingTrack.Album().Name();
            album = filterForFileName(album);

            var dir = downloadPath + album + "\\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string song = GetTrackFullName(downloadingTrack);
            song = filterForFileName(song);

            var fileName = dir + song + ".mp3";

            try
            {
                File.Move("downloading", fileName);
                FileInfo fileInfo = new FileInfo(fileName);
                String path = fileInfo.DirectoryName;
            }
            catch (Exception e) {

                File.Delete("downloading");
                LogString("Track deleted because the track already exists! Path: " + fileName + " Track Name:" + SpotifyDownloader.GetTrackFullName(downloadingTrack));
                
                base.EndOfTrack(session);

                if (OnDownloadProgress != null)
                    OnDownloadProgress(100);

                if (OnDownloadComplete != null)
                    OnDownloadComplete();
                
                return;
            }

            try
            {
                // Tag
                var u = new UltraID3();
                //u.GetMPEGTrackInfo();
                u.Read(fileName);
                u.Artist = GetTrackArtistsNames(downloadingTrack);
                u.Title = downloadingTrack.Name();
                u.Album = downloadingTrack.Album().Name();

                var imageID = downloadingTrack.Album().Cover(ImageSize.Large);
                var image = SpotifySharp.Image.Create(session, imageID);
                await WaitForBool(image.IsLoaded);

                var tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                var bmp = (Bitmap)tc.ConvertFrom(image.Data());

                var pictureFrame = new ID3v23PictureFrame(bmp, PictureTypes.CoverFront, "image", TextEncodingTypes.ISO88591);
                u.ID3v2Tag.Frames.Add(pictureFrame);

                u.Write();

                base.EndOfTrack(session);
            }
            catch (Exception e) { };

            LogString("Track downloaded and saved! Path: " + fileName + " Track Name:" + SpotifyDownloader.GetTrackFullName(downloadingTrack));

            if (OnDownloadProgress != null)
                OnDownloadProgress(100);

            if (OnDownloadComplete != null)
                OnDownloadComplete();
        }

        // VERY ugly hack, use the AddCallbacks method instead!
        private async Task<bool> WaitForBool(Func<bool> action)
        {
            await Task.Factory.StartNew(() =>
            {
                while (!action()) { };
            });
            return true;
        }

        public async Task<Playlist> FetchPlaylist(string linkStr)
        {
            var link = Link.CreateFromString(linkStr);
            var playlist = Playlist.Create(session, link);
            playlistName = playlist.Name();
            await WaitForBool(playlist.IsLoaded);
            for (int i = 0; i < playlist.NumTracks(); i++)
                await WaitForBool(playlist.Track(i).IsLoaded);
            return playlist;
        }

        public async Task<Track> FetchTrack(string linkStr)
        {
            var link = Link.CreateFromString(linkStr);
            var track = link.AsTrack();
            await WaitForBool(track.IsLoaded);
            return track;
        }

        public Track FetchTrackString(string linkStr)
        {
            var link = Link.CreateFromString(linkStr);
            Track track = link.AsTrack();
            return track;
        }


        public static Boolean canPlay(Track track)
        {
            try
            {
                session.PlayerLoad(track);
                session.PlayerPlay(true);
                session.PlayerPlay(false);
                session.PlayerUnload();
                return true;
            }
            catch (Exception e)
            {
                session.PlayerPlay(false);
                session.PlayerUnload();
                return false;   
            }
        }

        public void Download(Track track)
        {
            try
            {
                counter = 0;
                downloadingTrack = track;
                var stream = new FileStream("downloading", FileMode.Create);
                var waveFormat = new WaveFormat(44100, 16, 2);
                var beConfig = new BE_CONFIG(waveFormat, 320);
                wr = new Mp3Writer(stream, waveFormat, beConfig);
                session.PlayerLoad(track);
                session.PlayerPlay(true);
                if (OnDownloadProgress != null)
                    OnDownloadProgress(0);

            }
            catch (Exception e)
            {
                LogString("Error when playing/downloading!" + " Track Name:" + SpotifyDownloader.GetTrackFullName(downloadingTrack));
            }
        }


    public static string filterForFileName(string _fileName) { // Replace invalid file name characters \ /:*?"<>
    foreach(String s in replace)
        _fileName =  _fileName.Replace(s, "");
    return _fileName; 
    }


    public static void LogString(string message)
    {
        using (StreamWriter w = File.AppendText("log.txt"))
        {
            Log(message, w);
        }

        using (StreamReader r = File.OpenText("log.txt"))
        {
            DumpLog(r);
        }
    }
    public static void Log(string logMessage, TextWriter w)
    {
        try { 
        w.Write("\r\nLog Entry : ");
        w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
            DateTime.Now.ToLongDateString());
        w.WriteLine("  :");
        w.WriteLine("  :{0}", logMessage);
        w.WriteLine("-------------------------------");
        w.WriteLine(" ");
        w.Close();
        }
        catch (Exception e)
        {
            
        }
    }

    public static void DumpLog(StreamReader r)
    {
        string line;
        while ((line = r.ReadLine()) != null)
        {
            Console.WriteLine(line);
        }
    }

    }
}
