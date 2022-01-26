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

using Sims2Tools.DBPF.Data;
using Sims2Tools.DBPF.IO;
using Sims2Tools.DBPF.Utils;
using System;

namespace Sims2Tools.DBPF.CPF
{
    public class CpfItem : IDisposable
    {
        private string name;

        private MetaData.DataTypes datatype;
        private byte[] val;

        private Boolean isDirty = false;

        public bool IsDirty => isDirty;
        internal void SetClean() => isDirty = false;

        public CpfItem()
        {
            this.name = "";
            val = new byte[0];
            isDirty = false;
        }

        public CpfItem(string name) : this()
        {
            this.name = name;
        }

        public string Name
        {
            get => name;
        }

        public MetaData.DataTypes DataType
        {
            get => datatype;
            internal set { datatype = value; }
        }

        internal Byte[] Value
        {
            get => val;
            set { val = value; }
        }

        public string StringValue
        {
            get
            {
                switch (DataType)
                {
                    case MetaData.DataTypes.dtSingle:
                        {
                            return AsSingle().ToString();
                        }
                    case MetaData.DataTypes.dtInteger:
                    case Data.MetaData.DataTypes.dtUInteger:
                        {
                            return Helper.Hex8PrefixString((uint)AsInteger());
                        }
                    case MetaData.DataTypes.dtString:
                        {
                            return AsString();
                        }
                    default:
                        {
                            return "";
                        }
                }
            }

            set
            {
                if (DataType != MetaData.DataTypes.dtString) throw new ArgumentException();

                val = Helper.ToBytes(value, 0);

                isDirty = true;
            }
        }

        public uint UIntegerValue
        {
            get
            {
                switch (DataType)
                {
                    case MetaData.DataTypes.dtSingle:
                        {
                            return Convert.ToUInt32(AsSingle());
                        }
                    case MetaData.DataTypes.dtInteger:
                    case Data.MetaData.DataTypes.dtUInteger:
                        {
                            return AsUInteger();
                        }
                    case MetaData.DataTypes.dtString:
                        {
                            uint ret = 0;
                            try
                            {
                                ret = uint.Parse(AsString());
                            }
                            catch (Exception) { }
                            return ret;
                        }
                    default:
                        {
                            return 0;
                        }
                }
            }

            set
            {
                if (DataType != MetaData.DataTypes.dtUInteger) throw new ArgumentException();

                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.MemoryStream());
                bw.Write(value);

                System.IO.BinaryReader br = new System.IO.BinaryReader(bw.BaseStream);
                br.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                val = br.ReadBytes((int)br.BaseStream.Length);

                isDirty = true;
            }
        }

        public int IntegerValue
        {
            get
            {
                switch (DataType)
                {
                    case MetaData.DataTypes.dtSingle:
                        {
                            return Convert.ToInt32(AsSingle());
                        }
                    case MetaData.DataTypes.dtInteger:
                    case Data.MetaData.DataTypes.dtUInteger:
                        {
                            return AsInteger();
                        }
                    case MetaData.DataTypes.dtString:
                        {
                            int ret = 0;
                            try
                            {
                                ret = int.Parse(AsString());
                            }
                            catch (Exception) { }
                            return ret;
                        }
                    default:
                        {
                            return 0;
                        }
                }
            }

            set
            {
                if (DataType != MetaData.DataTypes.dtInteger) throw new ArgumentException();

                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.MemoryStream());
                bw.Write(value);

                System.IO.BinaryReader br = new System.IO.BinaryReader(bw.BaseStream);
                br.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                val = br.ReadBytes((int)br.BaseStream.Length);

                isDirty = true;
            }
        }

        public Single SingleValue
        {
            get
            {
                switch (DataType)
                {
                    case MetaData.DataTypes.dtSingle:
                        {
                            return AsSingle();
                        }
                    case MetaData.DataTypes.dtInteger:
                    case Data.MetaData.DataTypes.dtUInteger:
                        {
                            return AsInteger();
                        }
                    case MetaData.DataTypes.dtString:
                        {
                            Single ret = 0;
                            try
                            {
                                ret = Single.Parse(AsString());
                            }
                            catch (Exception) { }
                            return ret;
                        }
                    default:
                        {
                            return 0;
                        }
                }
            }

            set
            {
                if (DataType != MetaData.DataTypes.dtSingle) throw new ArgumentException();

                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.MemoryStream());
                bw.Write(value);

                System.IO.BinaryReader br = new System.IO.BinaryReader(bw.BaseStream);
                br.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                val = br.ReadBytes((int)br.BaseStream.Length);

                isDirty = true;
            }
        }

        public bool BooleanValue
        {
            get
            {
                switch (DataType)
                {
                    case MetaData.DataTypes.dtSingle:
                        {
                            return (AsSingle() != 0.0);
                        }
                    case MetaData.DataTypes.dtInteger:
                    case Data.MetaData.DataTypes.dtUInteger:
                        {
                            return (AsInteger() != 0);
                        }
                    case MetaData.DataTypes.dtString:
                        {
                            bool ret = false;
                            try
                            {
                                ret = (byte.Parse(AsString()) != 0);
                            }
                            catch (Exception)
                            {
                            }
                            return ret;
                        }
                    case MetaData.DataTypes.dtBoolean:
                        {
                            return AsBoolean();
                        }
                    default:
                        {
                            return false;
                        }
                }
            }

            set
            {
                if (DataType != MetaData.DataTypes.dtBoolean) throw new ArgumentException();

                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.MemoryStream());
                bw.Write(value);

                System.IO.BinaryReader br = new System.IO.BinaryReader(bw.BaseStream);
                br.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                val = br.ReadBytes((int)br.BaseStream.Length);

                isDirty = true;
            }
        }

        public object ObjectValue
        {
            get
            {
                switch (datatype)
                {
                    case Data.MetaData.DataTypes.dtUInteger:
                        {
                            return UIntegerValue;
                        }
                    case Data.MetaData.DataTypes.dtInteger:
                        {
                            return IntegerValue;
                        }
                    case Data.MetaData.DataTypes.dtSingle:
                        {
                            return SingleValue;
                        }
                    case Data.MetaData.DataTypes.dtBoolean:
                        {
                            return BooleanValue;
                        }
                    default:
                        {
                            return StringValue;
                        }
                }
            }
        }

        protected int AsInteger()
        {
            try
            {
                System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.MemoryStream(val));
                return br.ReadInt32();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        protected uint AsUInteger()
        {
            try
            {
                System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.MemoryStream(val));
                return br.ReadUInt32();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        protected bool AsBoolean()
        {
            if (Value.Length < 1) return false;
            else return (Value[0] == 1);
        }

        protected string AsString()
        {
            System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.MemoryStream(Value));
            try
            {
                string ret = "";
                while (br.PeekChar() != -1) ret += br.ReadChar();
                return ret;
            }
            catch (Exception)
            {
                return "";
            }
        }

        protected Single AsSingle()
        {
            System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.MemoryStream(Value));
            try
            {
                return br.ReadSingle();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        internal void Unserialize(DbpfReader reader)
        {
            datatype = (MetaData.DataTypes)reader.ReadUInt32();

            int namelength = reader.ReadInt32();
            name = Helper.ToString(reader.ReadBytes(namelength));

            int valuelength;
            switch (datatype)
            {
                case (MetaData.DataTypes.dtString):
                    {
                        valuelength = reader.ReadInt32();
                        break;
                    }
                case (MetaData.DataTypes.dtBoolean):
                    {
                        valuelength = 1;
                        break;
                    }
                default:
                    {
                        valuelength = 4;
                        break;
                    }
            }
            val = reader.ReadBytes(valuelength);
        }

        internal uint FileSize => (uint)(4 + 4 + name.Length + ((datatype == MetaData.DataTypes.dtString) ? 4 : 0) + val.Length);

        internal void Serialize(DbpfWriter writer)
        {
            // Store the type
            writer.WriteUInt32((uint)datatype);

            //Store the Name 
            writer.WriteUInt32((uint)name.Length);
            writer.WriteString(name);

            //Store the Value
            if (datatype == MetaData.DataTypes.dtString)
            {
                writer.WriteUInt32((uint)val.Length);
                writer.WriteBytes(val);
            }
            else
            {
                writer.WriteBytes(val);
            }
        }

        public override string ToString()
        {
            string ret = $"{Name} ({datatype}) = ";

            switch (this.DataType)
            {
                case Data.MetaData.DataTypes.dtUInteger:
                case Data.MetaData.DataTypes.dtInteger:
                    {
                        ret += Helper.Hex8PrefixString(this.UIntegerValue);
                        break;
                    }
                default:
                    {
                        if (ObjectValue != null) ret += ObjectValue.ToString();
                        break;
                    }
            }

            return ret;
        }

        public void Dispose()
        {
            this.val = new byte[0];
            this.val = null;
            this.name = null;
        }
    }
}
