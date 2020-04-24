using System.Diagnostics;
using SpotifySharp;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WaveLib;
using Yeti.Lame;
using Yeti.MMedia.Mp3;
using SpotifyWebApi;
using SpotifyWebApi.Model.Enum;
using SpotifyWebApi.Auth;
using System.Net;
using SpotifyWebApi.Model.Auth;

namespace Downtify
{
    public class TrackItem
    {
        public Track Track { get; private set; }

        public TrackItem(Track track)
        {
            Track = track;
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

    public class SpotifyWeb {

        ISpotifyWebApi _spotifyWebApi;
        Token _spotifyWebApiToken;
        string _clientId;
        string _clientSecret;
        AuthParameters _authParameters;


        public SpotifyWeb(string clientId, string clientSecret)
        {
            this._clientId = clientId;
            this._clientSecret = clientSecret;
        }

        private AuthParameters getAuthParameters()
        {
            return new AuthParameters
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scopes = Scope.All,
            };
        }

        private Token GetISpotifyWebApiToken()
        {
            return ClientCredentials.GetToken(_authParameters);
        }

        private ISpotifyWebApi CreatISpotifyWebApi()
        {
            return new SpotifyWebApi.SpotifyWebApi(_spotifyWebApiToken);
        }

        private void RefreshToken(Boolean refreshOnlyIfExpired)
        {
            if (!refreshOnlyIfExpired || (refreshOnlyIfExpired && _spotifyWebApiToken.IsExpired)) {

                //=====================
                // ValidationException("Refresh token was null or empty!") is always thrown, since string.IsNullOrEmpty(oldToken.RefreshToken) is alwyas true.
                //_spotifyWebApiToken = SpotifyWebApi.Auth.AuthorizationCode.RefreshToken(_authParameters, _spotifyWebApiToken);
                //_spotifyWebApi = CreatISpotifyWebApi();
                //=====================


                Auth(); // Always create new token until refreshing is fixed
            }

        }

        public void Auth()
        {
            // Autenticate _spotifyWebApi
            _authParameters = getAuthParameters();
            _spotifyWebApiToken = GetISpotifyWebApiToken();
            _spotifyWebApi = CreatISpotifyWebApi();
        }

        public ISpotifyWebApi GetISpotifyWebApi()
        {
            RefreshToken(true);
            return _spotifyWebApi;
        }

        // forceRefresh = true means that a brand new token must be created.
        // forceRefresh = false behaves like GetISpotifyWebApi() (no params)
        public ISpotifyWebApi GetISpotifyWebApi(Boolean forceRefresh)
        {
            RefreshToken(!forceRefresh);
            return _spotifyWebApi;
        }
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

        public bool Loaded
        {
            get { return _session.User().IsLoaded(); }
        }

        SpotifySession _session;
        Track _downloadingTrack;
        Mp3Writer _wr;
        SynchronizationContext _syncContext;
        SpotifyWeb _spotifyWeb;

        static string _appPath = AppDomain.CurrentDomain.BaseDirectory;
        static string _tmpPath = _appPath + "cache\\";
        static string _downloadPath = _appPath + "download\\";


        int _counter;

        public SpotifyDownloader()
        {
            if (!Directory.Exists(_tmpPath))
                Directory.CreateDirectory(_tmpPath);

            if (!Directory.Exists(_downloadPath))
                Directory.CreateDirectory(_downloadPath);

            var config = new SpotifySessionConfig()
            {
                ApiVersion = 12,
                CacheLocation = _tmpPath,
                SettingsLocation = _tmpPath,
                ApplicationKey = File.ReadAllBytes("spotify_appkey.key"),
                UserAgent = "downtify",
                Listener = this
            };

            _syncContext = SynchronizationContext.Current;
            _session = SpotifySession.Create(config);
        }

        private void InvokeProcessEvents()
        {
            _syncContext.Post(obj =>
            {
                int limit = 0;
                _session.ProcessEvents(ref limit);
            }, null);
        }

        public void Login(string username, string password)
        {
            _session.Login(username, password, true, null);
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

            // Autenticate _spotifyWeb
            _spotifyWeb = new SpotifyWeb(GUI.frmMain.configuration.GetConfiguration("clientId"), GUI.frmMain.configuration.GetConfiguration("clientSecret"));
            _spotifyWeb.Auth();

            // SpotifySharp log in
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
                OnLoginResult?.Invoke(true);
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

        public override int MusicDelivery(SpotifySession session, AudioFormat format, IntPtr frames, int numFrames)
        {
            if (numFrames == 0)
                return 0;

            var size = numFrames * format.channels * 2;
            var data = new byte[size];
            Marshal.Copy(frames, data, 0, size);

            _wr.Write(data);

            if (OnDownloadProgress != null)
            {
                _counter++;
                var duration = _downloadingTrack.Duration();
                // Todo: Find out how to calculate this correctly,
                // so far 46.4 is used to calculate the process
                // but there should be a way to calculate this
                // with the given variables
                var process = (int)Math.Round((double)100 / duration * (46.4 * _counter), 0);
                OnDownloadProgress(process);
            }

            return numFrames;
            // return base.MusicDelivery(session, format, frames, num_frames);
        }

        public override void StreamingError(SpotifySession session, SpotifyError error)
        {
            base.StreamingError(session, error);
        }

        public override void PlayTokenLost(SpotifySession session)
        {
            System.Windows.Forms.MessageBox.Show(GUI.frmMain.lang.GetString("error/connection_lost"));
            base.PlayTokenLost(session);
        }

        public override async void EndOfTrack(SpotifySession session)
        {
            session.PlayerPlay(false);
            _wr.Close();

            // Move File
            var dir = _downloadPath + escape(GetTrackArtistsNames(_downloadingTrack)) + "\\";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var fileName = getUpdatedTrackName(_downloadingTrack); ;
            if (GetDownloadType() == DownloadType.OVERWRITE && File.Exists(fileName))
                File.Delete(fileName);
            File.Move("downloading", fileName);

            // Tags
            var file = TagLib.File.Create(fileName);
            file.Tag.Title = _downloadingTrack.Name();
            file.Tag.Performers = new[] { GetTrackArtistsNames(_downloadingTrack) };
            file.Tag.Disc = (uint)_downloadingTrack.Disc();
            file.Tag.Year = (uint)_downloadingTrack.Album().Year();
            file.Tag.Track = (uint)_downloadingTrack.Index();
            file.Tag.Album = _downloadingTrack.Album().Name();
            file.Tag.Comment = Link.CreateFromTrack(_downloadingTrack, 0).AsString();

            // Download img
            Bitmap bmp = await DownloadImage(_downloadingTrack, 0);


            // Set img
            var pic = new TagLib.Picture();
            pic.Type = TagLib.PictureType.FrontCover;
            pic.Description = "Cover";
            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
            var ms = new MemoryStream();
            if (bmp != null)
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                ms.Position = 0;
                pic.Data = TagLib.ByteVector.FromStream(ms);
                file.Tag.Pictures = new TagLib.IPicture[] { pic };
            }

            // Save
            file.Save();

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
                while (!action())
                {
                }
            });
            return true;
        }

        public async Task<Playlist> FetchPlaylist(string linkStr)
        {
            var link = Link.CreateFromString(linkStr);
            var playlist = Playlist.Create(_session, link);
            await WaitForBool(playlist.IsLoaded);
            for (int i = 0; i < playlist.NumTracks(); i++)
                await WaitForBool(playlist.Track(i).IsLoaded);

            return playlist;
        }

        public async Task<AlbumBrowse> FetchAlbum(string linkStr)
        {
            var link = Link.CreateFromString(linkStr);
            var album = AlbumBrowse.Create(_session, link.AsAlbum(), AlbumBrowseCallBack, _session.UserData);
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
            if (!CanPlay(track))
            {
                if (OnDownloadComplete != null)
                    OnDownloadComplete(false);
                return;
            }

            _downloadingTrack = track;
            var fileName = getUpdatedTrackName(_downloadingTrack);

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
            _wr = new Mp3Writer(stream, waveFormat, beConfig);
            _session.PlayerLoad(track);
            _session.PlayerPlay(true);
            _session.SetVolumeNormalization(bool.Parse(GUI.frmMain.configuration.GetConfiguration("volume_normalization")));
            if (OnDownloadProgress != null)
                OnDownloadProgress(0);
        }

        string escape(string filepath)
        {
            foreach (var c in new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))
                filepath = filepath.Replace(c, '_');
            return filepath.Substring(0, filepath.Length < 100 ? filepath.Length : 100);
        }

        bool CanPlay(Track track)
        {
            bool ret;
            try
            {
                _session.PlayerLoad(track);
                _session.PlayerPlay(true);
                ret = true;
            }
            catch
            {
                ret = false;
            }
            finally
            {
                _session.PlayerPlay(false);
                _session.PlayerUnload();
            }
            return ret;
        }

        private string getUpdatedTrackName(Track track) 
        {
            _counter = 0;
            var dir = _downloadPath + escape(GetTrackArtistsNames(track)) + "\\";
            var fileExt = ".mp3";
            var fileName = dir + escape(GetTrackFullName(track));
            int fileCount = 0;
            if (File.Exists(fileName + fileExt))
            {
                // if it's not the same song (not the same uri), but both songs have the same name
                if (TagLib.File.Create(fileName + fileExt).Tag.Comment != Link.CreateFromTrack(track, 0).AsString())
                {
                    do
                    {
                        fileCount++;
                    }
                    while (File.Exists(fileName + "(" + fileCount.ToString() + ")" + fileExt));

                    // append counter
                    fileName += "(" + fileCount.ToString() + ")";
                }

            }

            // append extention
            return fileName + fileExt;
        }

        private void AlbumBrowseCallBack(AlbumBrowse browse, object userdata)
        {
            //Implentation not required, but method must exist.
        }

        private DownloadType GetDownloadType()
        {
            var typeStr = GUI.frmMain.configuration.GetConfiguration("file_exists").ToUpper();
            DownloadType type;
            try
            {
                type = (DownloadType)Enum.Parse(typeof(DownloadType), typeStr);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                type = DownloadType.SKIP;
            }
            return type;
        }

        public static bool IsSpotifyUrl(string str)
        {
            return (IsSpotifyTrackAlbumUrl(str) || IsSpotifyPlaylistUrl(str));
        }

        public static bool IsSpotifyPlaylistUrl(string str)
        {
            return (new Regex(@"(https|http)://open.spotify.com/user/([A-Za-z]|[0-9]|_)*/playlist/([A-Za-z]|[0-9]|_)*")).Matches(str).Count != 0;
        }

        public static bool IsSpotifyTrackAlbumUrl(string str)
        {
            return (new Regex(@"(https|http)://open.spotify.com/(album|track)/([A-Za-z]|[0-9]|_)*")).Matches(str).Count != 0;
        }

        public static string SpotifyUrlToUri(string url)
        {
            if (url == null)
                return null;

            var elements = url.Split('/');

            if (IsSpotifyPlaylistUrl(url))
            {
                return "spotify:user:" + elements[4] + ":playlist:" + elements[6];
            }

            if (IsSpotifyTrackAlbumUrl(url))
            {
                return "spotify:" + elements[3] + ":" + elements[4];
            }


            return url;
        }

        public async Task<string> GetImageOfUrl(Track track, ImageSize size)
        {
            // Download img
            var imageID = track.Album().Cover(size);
            var image = SpotifySharp.Image.Create(_session, imageID);
            await WaitForBool(image.IsLoaded);

            var link = Link.CreateFromImage(image);
            return link.AsString();

        }

        public async Task<Bitmap> DownloadImage(Track track, int size)
        {
            if (size < 0 || size > 3)
            {
                size = 0;
            }

            // Download img
            var trackInto = await _spotifyWeb.GetISpotifyWebApi().Track.GetTrack(SpotifyWebApi.Model.Uri.SpotifyUri.Make(Link.CreateFromTrack(track, 0).AsString()));
            var imgUrl = trackInto.Album.Images[size].Url;

            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imgUrl);
            return new Bitmap(stream);
        }
    }
}
