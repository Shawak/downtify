using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using LibGit2Sharp;

namespace DowntifyUpdater
{
    class Program
    {

        static string Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        static string Repo_directory = Environment.CurrentDirectory + "\\repo";
        static string Git_Remote = "https://github.com/Shawak/downtify.git";
        static string Downtify_Executable = "Downtify.exe";

        static void Main(string[] args)
        {
            bool forceInstall = false;
            foreach(string arg in args)
            {
                if (arg.ToLower().Equals("--dev"))
                {
                    Git_Remote = "https://github.com/lordsill/downtify.git";
                    forceInstall = true;
                }
                else if (arg.ToLower().Equals("--force"))
                    forceInstall = true;
            }
            if(!File.Exists(Downtify_Executable))
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
            Repository.Clone(Git_Remote, Repo_directory);
        }

        static void Update()
        {
            Log("Checking for updates...");
            Repository repo_local = new Repository(Repo_directory);
            Remote remote = repo_local.Network.Remotes["origin"];
            repo_local.Network.Fetch(remote);

            Commit currentCommit = repo_local.Head.Tip;
            Commit latestCommit = repo_local.Branches["origin/master"].Commits.First();

            if (currentCommit.Sha != latestCommit.Sha)
            {
                TreeChanges changes = repo_local.Diff.Compare<TreeChanges>(currentCommit.Tree, latestCommit.Tree);
                List<string> updatedPaths = new List<string>();
                Log("Updated files:");
                foreach (TreeEntryChanges c in changes)
                {
                    if (!updatedPaths.Contains(c.Path))
                    {
                        Log(c.Path);
                        updatedPaths.Add(c.Path);
                    }
                }
                repo_local.Checkout(latestCommit, new CheckoutOptions());
            }
            else
            {
                Log("No updated files found");
            }
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
            if (File.Exists(Downtify_Executable))
                File.Delete(Downtify_Executable);
            File.Copy(Repo_directory + @"\Downtify\bin\Debug\" + Downtify_Executable, Downtify_Executable);
            foreach(string dll_file in Directory.GetFiles(Repo_directory + @"\Downtify\bin\Debug", "*.dll", SearchOption.TopDirectoryOnly))
            {
                if (File.Exists(GetFileName(dll_file)))
                    File.Delete(GetFileName(dll_file));
                File.Copy(dll_file, GetFileName(dll_file));
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
