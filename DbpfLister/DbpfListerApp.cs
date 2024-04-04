﻿/*
 * DBPF Lister - a utility for testing the DBPF Library
 *
 * William Howard - 2020-2024
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools;
using System;
using System.Windows.Forms;

namespace DbpfLister
{
    static class DbpfListerApp
    {
        public static string AppName = "DBPF Lister";

        public static int AppVersionMajor = 0;
        public static int AppVersionMinor = 1;

#if DEBUG
        private static readonly int AppVersionDebug = 0;
#endif

        private static readonly string AppVersionType = "b"; // a - alpha, b - beta, r - release

#if DEBUG
        public static readonly string AppTitle = $"{AppName} V{AppVersionMajor}.{AppVersionMinor}.{AppVersionDebug}{AppVersionType}";
#else
        public static readonly string AppTitle = $"{AppName} V{AppVersionMajor}.{AppVersionMinor}{AppVersionType}";
#endif

#if DEBUG
        public static string AppProduct = $"{AppName} Version {AppVersionMajor}.{AppVersionMinor}.{AppVersionDebug}{AppVersionType} (debug)";
#else
        public static string AppProduct = $"{AppName} Version {AppVersionMajor}.{AppVersionMinor}{AppVersionType}";
#endif

        public static string RegistryKey = Sims2ToolsLib.RegistryKey + @"\DbpfLister";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DbpfListerForm());
        }
    }
}
