using Downtify.GUI;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Downtify
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (IsRunningOnMono())
            {
                Console.WriteLine("Mono is not supported because libspotify is not compatible with mono by default.");
                return;
            }

            bool isNew;
            var mutex = new Mutex(true, "Downtify", out isNew);

            if (!isNew)
            {
                MessageBox.Show("Application is already running.");
                return;
            }

            Run();
        }

        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        static void Run()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
