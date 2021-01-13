﻿/*
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files
 *
 * William Howard - 2020
 *
 * Parts of this code derived from the SimPE project - https://sourceforge.net/projects/simpe/
 * Parts of this code derived from the SimUnity2 project - https://github.com/LazyDuchess/SimUnity2 
 * Parts of this code may have been decompiled with the JetBrains decompiler
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF.BCON;
using Sims2Tools.DBPF.BHAV;
using Sims2Tools.DBPF.CTSS;
using Sims2Tools.DBPF.GLOB;
using Sims2Tools.DBPF.OBJD;
using Sims2Tools.DBPF.OBJF;
using Sims2Tools.DBPF.STR;
using Sims2Tools.DBPF.TPRP;
using Sims2Tools.DBPF.TRCN;
using Sims2Tools.DBPF.TTAB;
using Sims2Tools.DBPF.TTAS;
using Sims2Tools.DBPF.VERS;
using System;
using System.Collections.Generic;

namespace Sims2Tools.DBPF
{
    public class DBPFData
    {
        public static uint GROUP_GLOBALS = 0x7FD46CD0;
        public static String NAME_GLOBALS = "Globals";

        public static uint GROUP_BEHAVIOR = 0x7FE59FD0;
        public static String NAME_BEHAVIOR = "Behaviour";

        public static uint GROUP_LOCAL = 0xFFFFFFFF;
        public static String NAME_LOCAL = "Local";

        public static uint INSTANCE_OBJD_DEFAULT = 0x41A7;

        private static readonly Dictionary<uint, String> ModTypeNames = new Dictionary<uint, string>();
        private static readonly Dictionary<uint, String> AllTypeNames = new Dictionary<uint, string>();

        static DBPFData()
        {
            ModTypeNames.Add(Bcon.TYPE, Bcon.NAME);
            ModTypeNames.Add(Bhav.TYPE, Bhav.NAME);
            ModTypeNames.Add(Ctss.TYPE, Ctss.NAME);
            ModTypeNames.Add(Glob.TYPE, Glob.NAME);
            ModTypeNames.Add(Objd.TYPE, Objd.NAME);
            ModTypeNames.Add(Objf.TYPE, Objf.NAME);
            ModTypeNames.Add(Str.TYPE, Str.NAME);
            ModTypeNames.Add(Tprp.TYPE, Tprp.NAME);
            ModTypeNames.Add(Trcn.TYPE, Trcn.NAME);
            ModTypeNames.Add(Ttab.TYPE, Ttab.NAME);
            ModTypeNames.Add(Ttas.TYPE, Ttas.NAME);
            ModTypeNames.Add(Vers.TYPE, Vers.NAME);

            foreach (KeyValuePair<uint, String> kvPair in ModTypeNames) { AllTypeNames.Add(kvPair.Key, kvPair.Value); }
        }

        public static Dictionary<uint, String>.KeyCollection AllTypes
        {
            get => AllTypeNames.Keys;
        }

        public static Dictionary<uint, String>.KeyCollection ModTypes
        {
            get => ModTypeNames.Keys;
        }

        public static String TypeName(uint type)
        {
            AllTypeNames.TryGetValue(type, out string typeName);

            return typeName;
        }

        public static uint TypeID(String name)
        {
            foreach(KeyValuePair<uint, String> kvPair in AllTypeNames)
            {
                if (kvPair.Value.Equals(name.ToUpper()))
                {
                    return kvPair.Key;
                }
            }

            return 0x00000000;
        }
    }
}
