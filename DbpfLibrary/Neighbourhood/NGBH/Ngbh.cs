﻿/*
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files
 *
 * William Howard - 2020-2022
 *
 * Parts of this code derived from the SimPE project - https://sourceforge.net/projects/simpe/
 * Parts of this code derived from the SimUnity2 project - https://github.com/LazyDuchess/SimUnity2 
 * Parts of this code may have been decompiled with the JetBrains decompiler
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF.IO;
using Sims2Tools.DBPF.Package;
using Sims2Tools.DBPF.Utils;
using System.Collections.Generic;
using System.Xml;

namespace Sims2Tools.DBPF.Neighbourhood.NGBH
{
    public class Ngbh : DBPFResource
    {
        // See https://modthesims.info/wiki.php?title=List_of_Formats_by_Name
        public static readonly TypeTypeID TYPE = (TypeTypeID)0x4E474248;
        public const string NAME = "NGBH";

        uint version;
        public NgbhVersion Version => (NgbhVersion)version;

        byte[] zonename;
        public string ZoneName => Helper.ToString(zonename);

        List<NgbhGlobalSlot> globalSlots;
        List<NgbhInstanceSlot> lotSlots;
        List<NgbhInstanceSlot> familySlots;
        List<NgbhInstanceSlot> simSlots;

        public List<NgbhGlobalSlot> GlobalSlots => globalSlots;

        public List<NgbhInstanceSlot> LotSlots => lotSlots;

        public List<NgbhInstanceSlot> FamilySlots => familySlots;

        public List<NgbhInstanceSlot> SimSlots => simSlots;

        public Ngbh(DBPFEntry entry, DbpfReader reader) : base(entry)
        {
            Unserialize(reader);
        }

        // See https://modthesims.info/wiki.php?title=NGBH
        protected void Unserialize(DbpfReader reader)
        {
            _ = reader.ReadUInt32();

            version = reader.ReadUInt32();
            if (version == (uint)NgbhVersion.Castaway) _ = reader.ReadBytes(0x20);

            _ = reader.ReadUInt32();
            _ = reader.ReadUInt32(); // Neighborhood Height
            _ = reader.ReadUInt32(); // Neighborhood Width

            int textlen = reader.ReadInt32();
            zonename = reader.ReadBytes(textlen);

            if (version >= (uint)NgbhVersion.Nightlife) _ = reader.ReadBytes(0x14);
            else _ = reader.ReadBytes(0x18);

            int blocklen = 2;
            globalSlots = new List<NgbhGlobalSlot>();
            for (int i = 0; i < blocklen; i++)
            {
                globalSlots.Add(new NgbhGlobalSlot(this, reader));
            }

            blocklen = reader.ReadInt32();
            lotSlots = new List<NgbhInstanceSlot>();
            for (int i = 0; i < blocklen; i++)
            {
                lotSlots.Add(new NgbhInstanceSlot(this, reader));
            }

            blocklen = reader.ReadInt32();
            familySlots = new List<NgbhInstanceSlot>();
            for (int i = 0; i < blocklen; i++)
            {
                familySlots.Add(new NgbhInstanceSlot(this, reader));
            }

            blocklen = reader.ReadInt32();
            simSlots = new List<NgbhInstanceSlot>();
            for (int i = 0; i < blocklen; i++)
            {
                simSlots.Add(new NgbhInstanceSlot(this, reader));
            }
        }

        public override XmlElement AddXml(XmlElement parent)
        {
            return AddXml(parent, true, true, true);
        }

        public XmlElement AddXml(XmlElement parent, bool lots, bool families, bool sims)
        {
            XmlElement element = CreateInstElement(parent, NAME);

            element.SetAttribute("zonename", ZoneName);

            XmlElement eleGlobals = CreateElement(element, "global");
            foreach (NgbhGenericSlot item in GlobalSlots)
            {
                item.AddXml(eleGlobals);
            }

            if (lots)
            {
                XmlElement eleLots = CreateElement(element, "lots");
                foreach (NgbhInstanceSlot item in LotSlots)
                {
                    item.AddXml(eleLots, "lotId");
                }
            }

            if (families)
            {
                XmlElement eleFamilies = CreateElement(element, "families");
                foreach (NgbhInstanceSlot item in FamilySlots)
                {
                    item.AddXml(eleFamilies, "familyId");
                }
            }

            if (sims)
            {
                XmlElement eleSims = CreateElement(element, "sims");
                foreach (NgbhInstanceSlot item in SimSlots)
                {
                    item.AddXml(eleSims, "simId");
                }
            }

            return element;
        }
    }
}
