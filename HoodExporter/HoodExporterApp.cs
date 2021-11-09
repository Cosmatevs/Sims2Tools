﻿/*
 * Hood Exporter - a utility for exporting a Sims 2 'hood as XML
 *               - see http://www.picknmixmods.com/Sims2/Notes/HoodExporter/HoodExporter.html
 *
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files
 *
 * William Howard - 2020-2021
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using System;
using System.Windows.Forms;

namespace HoodExporter
{
    static class HoodExporterApp
    {
        public static String AppName = "Hood Exporter";

        public static int AppVersionMajor = 1;
        public static int AppVersionMinor = 2;
        public static String AppVersionType = "b"; // a - alpha, b - beta, r - release

#if DEBUG
        public static String AppVersionBuild = " (debug)";
#else
        public static String AppVersionBuild = "";
#endif

        public static String AppProduct = $"{AppName} Version {AppVersionMajor}.{AppVersionMinor}{AppVersionType}{AppVersionBuild}";

        public static String RegistryKey = Sims2Tools.Sims2ToolsLib.RegistryKey + @"\HoodExporter";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HoodExporterForm());
        }
    }
}
