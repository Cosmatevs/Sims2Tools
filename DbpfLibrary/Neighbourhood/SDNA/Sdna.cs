﻿/*
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files
 *
 * William Howard - 2020-2021
 *
 * Parts of this code derived from the SimPE project - https://sourceforge.net/projects/simpe/
 * Parts of this code derived from the SimUnity2 project - https://github.com/LazyDuchess/SimUnity2 
 * Parts of this code may have been decompiled with the JetBrains decompiler
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF.CPF;
using Sims2Tools.DBPF.IO;
using System.Xml;

namespace Sims2Tools.DBPF.Neighbourhood.SDNA
{
    public class Sdna : Cpf
    {
        // See https://modthesims.info/wiki.php?title=List_of_Formats_by_Name
        public static readonly TypeTypeID TYPE = (TypeTypeID)0xEBFEE33F;
        public const string NAME = "SDNA";
        readonly SdnaGene dominant, recessive;

        public SdnaGene Dominant
        {
            get { return dominant; }
        }

        public SdnaGene Recessive
        {
            get { return recessive; }
        }

        public Sdna(DBPFEntry entry, IoBuffer reader) : base(entry, reader)
        {
            dominant = new SdnaGene(this, 0);
            recessive = new SdnaGene(this, 0x10000000);
        }

        public override XmlElement AddXml(XmlElement parent)
        {
            XmlElement element = CreateInstElement(parent, NAME, "simId");

            Dominant.AddXml(CreateElement(element, "dominant"));
            Recessive.AddXml(CreateElement(element, "recessive"));

            return element;
        }
    }
}
