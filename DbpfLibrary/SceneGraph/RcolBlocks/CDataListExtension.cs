﻿/*
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files
 *
 * William Howard - 2020-2023
 *
 * Parts of this code derived from the SimPE project - https://sourceforge.net/projects/simpe/
 * Parts of this code derived from the SimUnity2 project - https://github.com/LazyDuchess/SimUnity2 
 * Parts of this code may have been decompiled with the JetBrains decompiler
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF.IO;
using Sims2Tools.DBPF.SceneGraph.RCOL;
using Sims2Tools.DBPF.SceneGraph.RcolBlocks.SubBlocks;
using System;

namespace Sims2Tools.DBPF.SceneGraph.RcolBlocks
{
    public class CDataListExtension : AbstractRcolBlock
    {
        public static readonly TypeBlockID TYPE = (TypeBlockID)0x6A836D56;
        public static String NAME = "cDataListExtension";

        private readonly Extension ext;

        public Extension Extension
        {
            get { return ext; }
        }


        // Needed by reflection to create the class
        public CDataListExtension(Rcol parent) : base(parent)
        {
            ext = new Extension(null);
            version = 0x01;
            BlockID = TYPE;
            BlockName = NAME;
        }

        public CDataListExtension(Rcol parent, string name) : this(parent)
        {
            BlockName = NAME;
            ext.VarName = name;
        }

        public override void Unserialize(DbpfReader reader)
        {
            version = reader.ReadUInt32();

            string blkName = reader.ReadString();
            TypeBlockID blkId = reader.ReadBlockId();

            ext.Unserialize(reader, version);
            ext.BlockName = blkName;
            ext.BlockID = blkId;
        }

        public override uint FileSize
        {
            get
            {
                long size = 4;

                size += (ext.BlockName.Length + 1) + 4 + ext.FileSize;

                return (uint)size;
            }
        }

        public override void Serialize(DbpfWriter writer)
        {
            writer.WriteUInt32(version);

            writer.WriteString(ext.BlockName);
            writer.WriteBlockId(ext.BlockID);
            ext.Serialize(writer, version);
        }

        public override void Dispose()
        {
        }
    }
}
