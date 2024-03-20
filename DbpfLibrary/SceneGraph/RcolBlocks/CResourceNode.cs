﻿/*
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files
 *
 * William Howard - 2020-2024
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
using Sims2Tools.DBPF.Utils;
using System;
using System.Collections.Generic;

namespace Sims2Tools.DBPF.SceneGraph.RcolBlocks
{
    public class ResourceNodeItem
    {
        short unknown1;
        public short Unknown1
        {
            get { return unknown1; }
        }

        int unknown2;
        public int Unknown2
        {
            get { return unknown2; }
        }

        /// <summary>
        /// Unserializes a BinaryStream into the Attributes of this Instance
        /// </summary>
        /// <param name="reader">The Stream that contains the FileData</param>
        public void Unserialize(DbpfReader reader)
        {
            unknown1 = reader.ReadInt16();
            unknown2 = reader.ReadInt32();
        }
    }

    public class CResourceNode : AbstractCresChildren
    {
        public static readonly TypeBlockID TYPE = (TypeBlockID)0xE519C933;
        public static string NAME = "cResourceNode";

        byte typecode;
        public byte TypeCode
        {
            get { return typecode; }
        }

        CObjectGraphNode ogn;
        public CObjectGraphNode GraphNode
        {
            get { return ogn; }
        }

        CompositionTreeNode ctn;
        public CompositionTreeNode TreeNode
        {
            get { return ctn; }
        }

        ResourceNodeItem[] items;
        public ResourceNodeItem[] Items
        {
            get { return items; }
        }

        // Needed by reflection to create the class
        public CResourceNode(Rcol parent) : base(parent)
        {
            ogn = new CObjectGraphNode(null);
            ctn = new CompositionTreeNode(null);
            items = new ResourceNodeItem[0];

            Version = 0x07;
            typecode = 0x01;
            BlockID = TYPE;
            BlockName = NAME;
        }



        public override string GetName()
        {
            return ogn.FileName;
        }
        /// <summary>
        /// Returns a List of all Child Blocks referenced by this Element
        /// </summary>
        public override List<int> ChildBlocks
        {
            get
            {
                List<int> l = new List<int>();
                foreach (ResourceNodeItem rni in items)
                {
                    l.Add((rni.Unknown2 >> 24) & 0xff);
                }
                return l;
            }
        }

        /// <summary>
        /// Unserializes a BinaryStream into the Attributes of this Instance
        /// </summary>
        /// <param name="reader">The Stream that contains the FileData</param>
        public override void Unserialize(DbpfReader reader)
        {
            Version = reader.ReadUInt32();
            typecode = reader.ReadByte();

            _ = reader.ReadString();
            TypeBlockID myid = reader.ReadBlockId();

            if (typecode == 0x01)
            {
                NameResource.Unserialize(reader);
                NameResource.BlockID = myid;

                _ = reader.ReadString();
                myid = reader.ReadBlockId();
                ctn.Unserialize(reader);
                ctn.BlockID = myid;

                _ = reader.ReadString();
                myid = reader.ReadBlockId();
                ogn.Unserialize(reader);
                ogn.BlockID = myid;

                items = new ResourceNodeItem[reader.ReadByte()];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = new ResourceNodeItem();
                    items[i].Unserialize(reader);
                }
                _ = reader.ReadInt32();
            }
            else if (typecode == 0x00)
            {
                ogn.Unserialize(reader);
                ogn.BlockID = myid;

                items = new ResourceNodeItem[1];
                items[0] = new ResourceNodeItem();
                items[0].Unserialize(reader);
            }
            else
            {
                throw new Exception("Unknown ResourceNode " + Helper.Hex4PrefixString(Version) + ", " + Helper.Hex2PrefixString(typecode));
            }
            _ = reader.ReadInt32();
        }

        public override void Dispose()
        {
            ogn = null;
            ctn = null;
            items = new ResourceNodeItem[0];
        }
    }
}
