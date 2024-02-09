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

using Sims2Tools.DBPF.BCON;
using Sims2Tools.DBPF.BHAV;
using Sims2Tools.DBPF.Cigen.CGN1;
using Sims2Tools.DBPF.CTSS;
using Sims2Tools.DBPF.GLOB;
using Sims2Tools.DBPF.Images.IMG;
using Sims2Tools.DBPF.Images.JPG;
using Sims2Tools.DBPF.Images.THUB;
using Sims2Tools.DBPF.IO;
using Sims2Tools.DBPF.Neighbourhood.BNFO;
using Sims2Tools.DBPF.Neighbourhood.FAMI;
using Sims2Tools.DBPF.Neighbourhood.FAMT;
using Sims2Tools.DBPF.Neighbourhood.IDNO;
using Sims2Tools.DBPF.Neighbourhood.LTXT;
using Sims2Tools.DBPF.Neighbourhood.NGBH;
using Sims2Tools.DBPF.Neighbourhood.SDNA;
using Sims2Tools.DBPF.Neighbourhood.SDSC;
using Sims2Tools.DBPF.Neighbourhood.SREL;
using Sims2Tools.DBPF.Neighbourhood.SWAF;
using Sims2Tools.DBPF.NREF;
using Sims2Tools.DBPF.OBJD;
using Sims2Tools.DBPF.OBJF;
using Sims2Tools.DBPF.SceneGraph;
using Sims2Tools.DBPF.SceneGraph.BINX;
using Sims2Tools.DBPF.SceneGraph.COLL;
using Sims2Tools.DBPF.SceneGraph.CRES;
using Sims2Tools.DBPF.SceneGraph.GMDC;
using Sims2Tools.DBPF.SceneGraph.GMND;
using Sims2Tools.DBPF.SceneGraph.GZPS;
using Sims2Tools.DBPF.SceneGraph.IDR;
using Sims2Tools.DBPF.SceneGraph.LAMB;
using Sims2Tools.DBPF.SceneGraph.LDIR;
using Sims2Tools.DBPF.SceneGraph.LPNT;
using Sims2Tools.DBPF.SceneGraph.LSPT;
using Sims2Tools.DBPF.SceneGraph.MMAT;
using Sims2Tools.DBPF.SceneGraph.SHPE;
using Sims2Tools.DBPF.SceneGraph.TXMT;
using Sims2Tools.DBPF.SceneGraph.TXTR;
using Sims2Tools.DBPF.SceneGraph.XFCH;
using Sims2Tools.DBPF.SceneGraph.XHTN;
using Sims2Tools.DBPF.SceneGraph.XMOL;
using Sims2Tools.DBPF.SceneGraph.XSTN;
using Sims2Tools.DBPF.SceneGraph.XTOL;
using Sims2Tools.DBPF.SLOT;
using Sims2Tools.DBPF.STR;
using Sims2Tools.DBPF.TPRP;
using Sims2Tools.DBPF.TRCN;
using Sims2Tools.DBPF.TTAB;
using Sims2Tools.DBPF.TTAS;
using Sims2Tools.DBPF.UI;
using Sims2Tools.DBPF.Utils;
using Sims2Tools.DBPF.VERS;
using Sims2Tools.DBPF.XFLR;
using Sims2Tools.DBPF.XFNC;
using Sims2Tools.DBPF.XOBJ;
using Sims2Tools.DBPF.XROF;
using Sims2Tools.DBPF.XWNT;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sims2Tools.DBPF.Package
{
    // See also - https://modthesims.info/wiki.php?title=DBPF/Source_Code and https://modthesims.info/wiki.php?title=DBPF
    public class DBPFFile : IDisposable
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string packagePath;
        private readonly string packageName;
        private readonly string packageNameNoExtn;

        private DBPFHeader header;
        private DBPFResourceIndex resourceIndex;
        private DBPFResourceCache resourceCache;

        private DbpfReader m_Reader;

        public string PackagePath => packagePath;
        public string PackageName => packageName;
        public string PackageNameNoExtn => packageNameNoExtn;

        public uint ResourceCount => header.ResourceIndexCount;

        public bool IsDirty => resourceIndex.IsDirty;

        public DBPFFile(string packagePath)
        {
            this.packagePath = packagePath;

            if (File.Exists(packagePath))
            {
                this.packageName = new FileInfo(packagePath).Name;
                this.packageNameNoExtn = packageName.Substring(0, packageName.LastIndexOf('.'));

                Read(File.OpenRead(packagePath));
            }
            else
            {
                header = new DBPFHeader();
                resourceCache = new DBPFResourceCache();
                resourceIndex = new DBPFResourceIndex(header, resourceCache, null);
            }
        }

        protected void Read(Stream stream)
        {
            this.m_Reader = DbpfReader.FromStream(stream);

            header = new DBPFHeader(m_Reader);

            resourceCache = new DBPFResourceCache();
            resourceIndex = new DBPFResourceIndex(header, resourceCache, m_Reader);
        }

        protected void Write(Stream stream)
        {
            using (DbpfWriter writer = DbpfWriter.FromStream(stream))
            {
                header.Serialize(writer, resourceIndex);

                resourceIndex.Serialize(writer);

                foreach (DBPFEntry entry in resourceIndex.GetAllEntries())
                {
                    if (resourceCache.IsResource(entry))
                    {
                        resourceCache.GetResourceByKey(entry).Serialize(writer);
                    }
                    else if (resourceCache.IsItem(entry))
                    {
                        writer.WriteBytes(resourceCache.GetItemByKey(entry));
                    }
                    else
                    {
                        m_Reader.Seek(SeekOrigin.Begin, entry.FileOffset);
                        writer.WriteBytes(m_Reader.ReadBytes((int)entry.FileSize));
                    }
                }
            }
        }

        public string Update(bool autoBackup)
        {
            string originalName = packagePath;
            string updateName = $"{packagePath}.temp";
            string backupName = $"{packagePath}.bak";

            using (Stream stream = File.OpenWrite(updateName))
            {
                Write(stream);

                stream.Close();

                this.Close();

                if (autoBackup && File.Exists(originalName))
                {
                    File.Copy(originalName, backupName, true);
                }

                try
                {
                    File.Delete(originalName);

                    File.Copy(updateName, originalName, true);
                    File.Delete(updateName);
                }
                catch (Exception)
                {
                    // SimPe propbably has the file open!
                    backupName = null;
                }

                Read(File.OpenRead(originalName));
            }

            return backupName;
        }

        public void Commit(DBPFResource resource)
        {
            resourceIndex.Commit(resource);
        }

        public void Commit(DBPFKey key, byte[] item)
        {
            resourceIndex.Commit(key, item);
        }

        public bool Remove(DBPFResource resource)
        {
            return resourceIndex.Remove(resource);
        }

        private DbpfReader GetDbpfReader(DBPFEntry entry)
        {
            if (entry.UncompressedSize != 0)
            {
                byte[] data = GetItemByEntry(entry);

                return DbpfReader.FromStream(new MemoryStream(data));
            }
            else
            {
                m_Reader.Seek(SeekOrigin.Begin, entry.FileOffset);

                return DbpfReader.FromStream(m_Reader.MyStream, entry.FileSize);
            }
        }

        private byte[] GetItemByEntry(DBPFEntry entry)
        {
            m_Reader.Seek(SeekOrigin.Begin, entry.FileOffset);

            // TODO - just because there is no CLST resource (or no entry in the CLST) does NOT mean the data isn't compressed!
            // TODO - Try to guess if this is compressed data
            // If entry.FileSize > 9 we could have compressed data
            //   Read the first 4 bytes as a DWORD, if the value is the same as entry.FileSize it can still be compressed data
            //   Read the next 2 bytes as a WORD, if the value is 0xFB10 it probably is compressed data
            //   Read the next 3 bytes as a big-endian 24-bit value, would this make sense as a uncompressed data size?

            if (entry.UncompressedSize != 0)
            {
                try
                {
                    return Decompressor.Decompress(m_Reader.ReadBytes((int)entry.FileSize), entry.UncompressedSize);
                }
                catch (Exception)
                {
                    // This is a fall-back that should never happen as the decompressor does the necessary checks
                    logger.Warn($"Failed to decompress {entry}");

                    m_Reader.Seek(SeekOrigin.Begin, entry.FileOffset);
                }
            }

            return m_Reader.ReadBytes((int)entry.FileSize);
        }

        public byte[] GetOriginalItemByEntry(DBPFEntry entry)
        {
            return GetItemByEntry(entry);
        }

        public DBPFEntry GetEntryByTGIR(int tgir)
        {
            return resourceIndex.GetEntryByTGIR(tgir);
        }

        public DBPFEntry GetEntryByKey(DBPFKey key)
        {
            return resourceIndex.GetEntryByKey(key);
        }

        public List<DBPFEntry> GetAllEntries()
        {
            return resourceIndex.GetAllEntries();
        }

        public List<DBPFEntry> GetEntriesByType(TypeTypeID type)
        {
            return resourceIndex.GetEntriesByType(type);
        }

        public String GetFilenameByEntry(DBPFEntry entry)
        {
            if (entry.TypeID == Ui.TYPE)
            {
                return "";
            }
            else
            {
                return Helper.ToString(this.GetDbpfReader(entry).ReadBytes(Math.Min((int)entry.FileSize, 0x40)));
            }
        }

        public DBPFResource GetResourceByName(TypeTypeID typeId, string sgName)
        {
            return GetResourceByKey(SgHelper.KeyFromQualifiedName(sgName, typeId, DBPFData.GROUP_SG_MAXIS));
        }

        public DBPFResource GetResourceByTGIR(int tgir)
        {
            return GetResourceByEntry(GetEntryByTGIR(tgir));
        }

        public DBPFResource GetResourceByKey(DBPFKey key)
        {
            return GetResourceByEntry(GetEntryByKey(key));
        }

        public DBPFResource GetResourceByEntry(DBPFEntry entry)
        {
            if (entry == null) return null;

            if (resourceCache.IsCached(entry))
            {
                return resourceCache.GetResourceByKey(entry);
            }

            DBPFResource res = null;

            if (entry.TypeID == Bcon.TYPE)
            {
                res = new Bcon(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Bhav.TYPE)
            {
                res = new Bhav(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Ctss.TYPE)
            {
                res = new Ctss(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Glob.TYPE)
            {
                res = new Glob(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Objd.TYPE)
            {
                res = new Objd(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Objf.TYPE)
            {
                res = new Objf(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Nref.TYPE)
            {
                res = new Nref(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Slot.TYPE)
            {
                res = new Slot(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Str.TYPE)
            {
                res = new Str(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Tprp.TYPE)
            {
                res = new Tprp(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Trcn.TYPE)
            {
                res = new Trcn(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Ttab.TYPE)
            {
                res = new Ttab(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Ttas.TYPE)
            {
                res = new Ttas(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Vers.TYPE)
            {
                res = new Vers(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xflr.TYPE)
            {
                res = new Xflr(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xfnc.TYPE)
            {
                res = new Xfnc(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xobj.TYPE)
            {
                res = new Xobj(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xrof.TYPE)
            {
                res = new Xrof(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xwnt.TYPE)
            {
                res = new Xwnt(entry, this.GetDbpfReader(entry));
            }
            //
            // Image resources
            //
            else if (entry.TypeID == Img.TYPE)
            {
                res = new Img(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Jpg.TYPE)
            {
                res = new Jpg(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Thub.TYPE)
            {
                res = new Thub(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.Awning] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.Chimney] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.Dormer] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.FenceArch] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.FenceOrHalfwall] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.Floor] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.FoundationOrPool] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.ModularStair] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.Roof] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.Terrain] ||
                     entry.TypeID == Thub.TYPES[(int)Thub.ThubTypeIndex.Wall])
            {
                res = new Thub(entry, this.GetDbpfReader(entry));
            }
            //
            // Neighbourhood resources
            //
            else if (entry.TypeID == Bnfo.TYPE)
            {
                res = new Bnfo(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Fami.TYPE)
            {
                res = new Fami(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Famt.TYPE)
            {
                res = new Famt(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Idno.TYPE)
            {
                res = new Idno(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Ltxt.TYPE)
            {
                res = new Ltxt(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Ngbh.TYPE)
            {
                res = new Ngbh(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Sdna.TYPE)
            {
                res = new Sdna(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Sdsc.TYPE)
            {
                res = new Sdsc(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Srel.TYPE)
            {
                res = new Srel(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Swaf.TYPE)
            {
                res = new Swaf(entry, this.GetDbpfReader(entry));
            }
            //
            // SceneGraph resources
            //
            else if (entry.TypeID == Binx.TYPE)
            {
                res = new Binx(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Coll.TYPE)
            {
                res = new Coll(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Cres.TYPE)
            {
                res = new Cres(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Gmdc.TYPE)
            {
                res = new Gmdc(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Gmnd.TYPE)
            {
                res = new Gmnd(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Gzps.TYPE)
            {
                res = new Gzps(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Idr.TYPE)
            {
                res = new Idr(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Lamb.TYPE)
            {
                res = new Lamb(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Ldir.TYPE)
            {
                res = new Ldir(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Lpnt.TYPE)
            {
                res = new Lpnt(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Lspt.TYPE)
            {
                res = new Lspt(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Mmat.TYPE)
            {
                res = new Mmat(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Shpe.TYPE)
            {
                res = new Shpe(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Txmt.TYPE)
            {
                res = new Txmt(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Txtr.TYPE)
            {
                res = new Txtr(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xfch.TYPE)
            {
                res = new Xfch(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xmol.TYPE)
            {
                res = new Xmol(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xhtn.TYPE)
            {
                res = new Xhtn(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xstn.TYPE)
            {
                res = new Xstn(entry, this.GetDbpfReader(entry));
            }
            else if (entry.TypeID == Xtol.TYPE)
            {
                res = new Xtol(entry, this.GetDbpfReader(entry));
            }
            //
            // Cigen Resources
            //
            else if (entry.TypeID == Cgn1.TYPE)
            {
                res = new Cgn1(entry, this.GetDbpfReader(entry));
            }


            return res;
        }

        public void Close()
        {
            m_Reader?.Close();
        }

        public void Dispose()
        {
            Close();
            m_Reader?.Dispose();
        }
    }
}
