using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using NGit;
using NGit.Api;
using NGit.Transport;

namespace DowntifyUpdater
{
    class Program
    {

        static string Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        static string Repo_directory = Environment.CurrentDirectory + "\\repo";
        static string Git_Remote = null;
        static string Downtify_Executable = "Downtify.exe";

        static void Main(string[] args)
        {
            bool forceInstall = false;
            XmlConfiguration config = new XmlConfiguration("config.xml");
            config.LoadConfigurationFile();
            Git_Remote = config.GetConfiguration("update_repo", "https://github.com/Shawak/downtify.git");
            for(int iArg = 0; iArg < args.Length; iArg++)
            {
                if (args[iArg].ToLower().Equals("--git"))
                    if (iArg != args.Length - 1)
                        Git_Remote = args[iArg + 1];
                    else
                        Console.WriteLine("No update_repo specified.");
                if (args[iArg].ToLower().Equals("--force"))
                    forceInstall = true;
            }
            if (!File.Exists(Downtify_Executable))
            {
                Log(Downtify_Executable + " was not found. Please execute this Updater in the Downtify-Folder.");
                Console.ReadKey(true);
                return;
            }
            Console.Title = "Downtify Updater v" + Version;
            Log("Repository Directory: " + Repo_directory);

            if (forceInstall)
            {
                if (Directory.Exists(Repo_directory))
                    DeleteDirectory(Repo_directory);
                Thread.Sleep(500);
                Install();
            }
            else
            {
                if (!Directory.Exists(Repo_directory))
                    Install();
                else
                    Update();
            }

            Build();
            CopyUpdate();
            Process downtify = new Process();
            downtify.StartInfo.FileName = Downtify_Executable;
            downtify.Start();
        }

        static void Install()
        {
            Log("Cloning from " + Git_Remote + " to Repository Directory");
            Directory.CreateDirectory(Repo_directory);
            Git.CloneRepository().SetDirectory(Repo_directory).SetURI(Git_Remote).Call();
        }

        static void Update()
        {
            Log("Checking for updates...");
            Git repository = Git.Open(Repo_directory);
            ICollection<TrackingRefUpdate> refUpdate = repository.Fetch().Call().GetTrackingRefUpdates();
            if (refUpdate.Count > 0)
            {
                Log(String.Format("Downloading {0} Updates...", refUpdate.Count));
                repository.BranchCreate().SetForce(true).SetName("master").SetStartPoint("origin/master").Call();
                repository.Checkout().SetName("master").Call();
            }
            else
            {
                Log("Repository already up2date");
            }
            repository.GetRepository().Close();
            repository.GetRepository().ObjectDatabase.Close();
        }

        static void Build()
        {
            DeleteDirectory(Repo_directory + @"\Downtify\bin");
            Log("Build Downtify...");
            string net_folder = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Microsoft.NET\Framework";
            if (Directory.Exists(net_folder + "64"))
                net_folder += "64";
            string[] subdir = Directory.GetDirectories(net_folder, "v4.*");
            string msbuild = subdir.Length != 0 ? subdir[subdir.Length-1] : null;
            if(msbuild == null)
            {
                Log(".NET Framework 4 is not installed. Please install .NET Framework 4");
                return;
            }
            msbuild += @"\msbuild.exe";

            Process buildProc = new Process();
            buildProc.StartInfo.FileName = msbuild;
            buildProc.StartInfo.CreateNoWindow = true;
            buildProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            buildProc.StartInfo.Arguments = "Downtify.csproj /t:Build";
            buildProc.StartInfo.WorkingDirectory = Repo_directory + @"\Downtify";
            buildProc.Start();
            buildProc.WaitForExit();
            Log("Downtify built.");
        }

        static void CopyUpdate()
        {
            //Copy Binary
            if (File.Exists(Downtify_Executable))
                File.Delete(Downtify_Executable);
            File.Copy(Repo_directory + @"\Downtify\bin\Debug\" + Downtify_Executable, Downtify_Executable);
            //Copy Libraries
            foreach(string dll_file in Directory.GetFiles(Repo_directory + @"\Downtify\bin\Debug", "*.dll", SearchOption.TopDirectoryOnly))
            {
                if (File.Exists(GetFileName(dll_file)))
                    File.Delete(GetFileName(dll_file));
                File.Copy(dll_file, GetFileName(dll_file));
            }
            //Copy Language Files
            foreach (string language_file in Directory.GetFiles(Repo_directory + @"\Downtify\bin\Debug\language", "*.xml", SearchOption.TopDirectoryOnly))
            {
                if(File.Exists("language/" + GetFileName(language_file)))
                    File.Delete("language/" + GetFileName(language_file));
                File.Copy(language_file, "language/" + GetFileName(language_file));
            }
        }

        static string GetFileName(string filepath)
        {
            string[] splitter = filepath.Split('\\');
            return splitter[splitter.Length - 1];
        }

        static void DeleteDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return;
            foreach(string dirname in Directory.GetDirectories(directory))
            {
                DeleteDirectory(dirname);
            }
            foreach(string filename in Directory.GetFiles(directory))
            {
                File.Delete(filename);
            }
            Directory.Delete(directory);
        }

        static void Log(string message)
        {
            string time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            Console.WriteLine(String.Format("[{0}] {1}", time, message));
            StreamWriter sw = new StreamWriter("updater.log", true);
            sw.WriteLine(String.Format("[{0}] {1}", time, message));
            sw.Close();
        }
    }
}
