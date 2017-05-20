using HundredMilesSoftware.UltraID3Lib;
using SpotifySharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WaveLib;
using Yeti.Lame;
using Yeti.MMedia.Mp3;

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

    public enum DownloadType
    {
        SKIP,
        OVERWRITE
    }

    public class SpotifyDownloader : SpotifySessionListener
    {
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

        public delegate void OnDownloadCompleteHandler(bool successfully);
        public event OnDownloadCompleteHandler OnDownloadComplete;

        public bool Loaded { get { return session.User().IsLoaded(); } }

        SpotifySession session;
        Track downloadingTrack;
        Mp3Writer wr;
        SynchronizationContext syncContext;

        string tmpPath;
        string downloadPath;

        public SpotifyDownloader()
        {
            tmpPath = Downtify.GUI.frmMain.configuration.GetConfiguration("cache", "cache/");
            downloadPath = Downtify.GUI.frmMain.configuration.GetConfiguration("download", "download/");

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
        }

        private void InvokeProcessEvents()
        {
            syncContext.Post(obj =>
            {
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
            Task.Factory.StartNew(() => InvokeProcessEvents());
            base.NotifyMainThread(session);
        }

        public override async void LoggedIn(SpotifySession session, SpotifyError error)
        {
            if (error != SpotifyError.Ok)
            {
                if (OnLoginResult != null)
                    OnLoginResult(false);
                return;
            }

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
            if (num_frames == 0)
                return 0;

            var size = num_frames * format.channels * 2;
            var data = new byte[size];
            Marshal.Copy(frames, data, 0, size);

            wr.Write(data);

            if (OnDownloadProgress != null)
            {
                counter++;
                var duration = downloadingTrack.Duration();
                // Todo: Find out how to calculate this correctly,
                // so far 46.4 is used to calculate the process
                // but there should be a way to calculate this
                // with the given variables
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
            System.Windows.Forms.MessageBox.Show(Downtify.GUI.frmMain.lang.GetString("error/connection_lost"));
            base.PlayTokenLost(session);
        }

        public override async void EndOfTrack(SpotifySession session)
        {
            session.PlayerPlay(false);
            wr.Close();

            // Move File
            var dir = downloadPath + escape(downloadingTrack.Album().Name()) + "\\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var fileName = dir + escape(GetTrackFullName(downloadingTrack)) + ".mp3";
            if(GetDownloadType() == DownloadType.OVERWRITE && File.Exists(fileName))
                File.Delete(fileName);
            File.Move("downloading", fileName);

            // Tag
            var u = new UltraID3();
            u.Read(fileName);
            u.Artist = GetTrackArtistsNames(downloadingTrack);
            u.Title = downloadingTrack.Name();
            u.Album = downloadingTrack.Album().Name();
            u.TrackNum = (short)downloadingTrack.Index();

            var imageID = downloadingTrack.Album().Cover(ImageSize.Large);
            var image = SpotifySharp.Image.Create(session, imageID);
            await WaitForBool(image.IsLoaded);

            var tc = TypeDescriptor.GetConverter(typeof(Bitmap));
            var bmp = (Bitmap)tc.ConvertFrom(image.Data());

            var pictureFrame = new ID3v23PictureFrame(bmp, PictureTypes.CoverFront, "image", TextEncodingTypes.ISO88591);
            u.ID3v2Tag.Frames.Add(pictureFrame);

            u.Write();

            base.EndOfTrack(session);

            if (OnDownloadProgress != null)
                OnDownloadProgress(100);

            if (OnDownloadComplete != null)
                OnDownloadComplete(true);
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
            await WaitForBool(playlist.IsLoaded);
            for (int i = 0; i < playlist.NumTracks(); i++)
                await WaitForBool(playlist.Track(i).IsLoaded);
            return playlist;
        }

        public async Task<AlbumBrowse> FetchAlbum(string linkStr)
        {
            var link = Link.CreateFromString(linkStr);
            var album = AlbumBrowse.Create(session, link.AsAlbum(), AlbumBrowseCallBack, session.UserData);
            await WaitForBool(album.IsLoaded);
            for (int i = 0; i < album.NumTracks(); i++)
                await WaitForBool(album.Track(i).IsLoaded);
            return album;
        }
        
        public async Task<Track> FetchTrack(string linkStr)
        {
            var link = Link.CreateFromString(linkStr);
            var track = link.AsTrack();
            await WaitForBool(track.IsLoaded);
            return track;
        }

        public void Download(Track track)
        {
            if (!canPlay(track))
            {
                if (OnDownloadComplete != null)
                    OnDownloadComplete(false);
                return;
            }

            counter = 0;
            downloadingTrack = track;

            var dir = downloadPath + escape(downloadingTrack.Album().Name()) + "\\";
            var fileName = dir + escape(GetTrackFullName(downloadingTrack)) + ".mp3";
            if (GetDownloadType() == DownloadType.SKIP && File.Exists(fileName))
            {
                if (OnDownloadProgress != null)
                    OnDownloadProgress(100);

                if (OnDownloadComplete != null)
                    OnDownloadComplete(true);
                return;
            }

            var stream = new FileStream("downloading", FileMode.Create);
            var waveFormat = new WaveFormat(44100, 16, 2);
            var beConfig = new BE_CONFIG(waveFormat, 320);
            wr = new Mp3Writer(stream, waveFormat, beConfig);
            session.PlayerLoad(track);
            session.PlayerPlay(true);
            if (OnDownloadProgress != null)
                OnDownloadProgress(0);
        }

        string escape(string filepath)
        {
            foreach(var c in new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))
                filepath = filepath.Replace(c, '_');
            return filepath.Substring(0, filepath.Length < 100 ? filepath.Length : 100);
        }

        bool canPlay(Track track)
        {
            bool ret;
            try
            {
                session.PlayerLoad(track);
                session.PlayerPlay(true);
                ret = true;
            }
            catch
            {
                ret = false;
            }
            finally
            {
                session.PlayerPlay(false);
                session.PlayerUnload();
            }
            return ret;
        }

        private void AlbumBrowseCallBack(AlbumBrowse browse, object userdata)
        {
            //Implentation not required, but method must exist.
        }

        private DownloadType GetDownloadType()
        {
            var typeStr = Downtify.GUI.frmMain.configuration.GetConfiguration("file_exists", "SKIP").ToUpper();
            DownloadType type;
            try
            {
                type = (DownloadType)Enum.Parse(typeof(DownloadType), typeStr);
            }
            catch (Exception e)
            {
                type = DownloadType.SKIP;
            }
            return type;
        }
    }
}
