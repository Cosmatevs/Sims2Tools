﻿/*
 * Object Relocator - a utility for moving objects in the Buy/Build Mode catalogues
 *
 * William Howard - 2020-2023
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools;
using System;
using System.Windows.Forms;

namespace ObjectRelocator
{
    static class ObjectRelocatorApp
    {
        public static readonly String AppName = "Object Relocator";

        public static readonly int AppVersionMajor = 3;
        public static readonly int AppVersionMinor = 3;

#if DEBUG
        private static readonly int AppVersionDebug = 0;
#endif

        private static readonly string AppVersionType = "r"; // a - alpha, b - beta, r - release

#if DEBUG
        public static readonly string AppProduct = $"{AppName} Version {AppVersionMajor}.{AppVersionMinor}.{AppVersionDebug}{AppVersionType} (debug)";
#else
        public static readonly string AppProduct = $"{AppName} Version {AppVersionMajor}.{AppVersionMinor}{AppVersionType}";
#endif

        public static readonly String RegistryKey = Sims2ToolsLib.RegistryKey + @"\ObjectRelocator";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ObjectRelocatorForm());
        }
    }
}
