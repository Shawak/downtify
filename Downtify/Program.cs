using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Downtify.GUI;
using System.Diagnostics;
using System.Security.Principal;

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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
