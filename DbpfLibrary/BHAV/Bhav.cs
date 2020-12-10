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

using Sims2Tools.DBPF.IO;
using Sims2Tools.DBPF.Utils;
using System.Collections.Generic;
using System.Xml;

namespace Sims2Tools.DBPF.BHAV
{
    public class Bhav : DBPFResource
    {
        // See https://modthesims.info/wiki.php?title=List_of_Formats_by_Name
        public const uint TYPE = 0x42484156;
        public const string NAME = "BHAV";

        private readonly BhavHeader header;

        private List<Instruction> items;

        public Bhav(DBPFEntry entry, IoBuffer reader) : base(entry)
        {
            this.header = new BhavHeader();

            Unserialize(reader);
        }

        public BhavHeader Header => this.header;

        public List<Instruction> Instructions => this.items;

        protected void Unserialize(IoBuffer reader)
        {
            this.filename = reader.ReadBytes(0x40);

            this.header.Unserialize(reader);
            this.items = new List<Instruction>();
            while (this.items.Count < Header.InstructionCount)
                this.items.Add(new Instruction(reader, header.Format));
        }

        public override void AddXml(XmlElement parent)
        {
            XmlElement element = CreateResElement(parent, NAME);
            element.SetAttribute("format", Helper.Hex4PrefixString(Header.Format));
            element.SetAttribute("params", Helper.Hex2PrefixString(Header.ArgCount));
            element.SetAttribute("locals", Helper.Hex2PrefixString(Header.LocalVarCount));
            element.SetAttribute("headerFlag", Helper.Hex2PrefixString(Header.HeaderFlag));
            element.SetAttribute("cacheFlags", Helper.Hex2PrefixString(Header.CacheFlags));
            element.SetAttribute("treeType", Helper.Hex2PrefixString(Header.TreeType));
            element.SetAttribute("treeVersion", Helper.Hex4PrefixString(Header.TreeVersion));

            for (int i = 0; i < Header.InstructionCount; ++i)
            {
                Instruction item = Instructions[i];

                XmlElement inst = CreateElement(element, "instruction");
                inst.SetAttribute("nodeVersion", Helper.Hex4PrefixString(item.NodeVersion));
                inst.SetAttribute("opCode", Helper.Hex4PrefixString(item.OpCode));
                inst.SetAttribute("trueTarget", Helper.Hex4PrefixString(item.TrueTarget));
                inst.SetAttribute("falseTarget", Helper.Hex4PrefixString(item.FalseTarget));

                XmlElement ops = CreateElement(inst, "operands");
                for (int j = 0; j < 16; j++)
                {
                    ops.SetAttribute("operand" + j.ToString(), Helper.Hex2PrefixString(item.Operands[j]));
                }
            }
        }
    }
}
