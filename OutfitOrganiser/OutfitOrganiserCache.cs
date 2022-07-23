﻿/*
 * Outfit Organiser - a utility for organising Sims 2 outfits (clothing etc)
 *                  - see http://www.picknmixmods.com/Sims2/Notes/OutfitOrganiser/OutfitOrganiser.html
 *
 * Sims2Tools - a toolkit for manipulating The Sims 2 DBPF files
 *
 * William Howard - 2020-2022
 *
 * Permission granted to use this code in any way, except to claim it as your own or sell it
 */

using Sims2Tools.DBPF;
using Sims2Tools.DBPF.CPF;
using Sims2Tools.DBPF.Data;
using Sims2Tools.DBPF.Images.IMG;
using Sims2Tools.DBPF.Package;
using Sims2Tools.DBPF.SceneGraph.BINX;
using Sims2Tools.DBPF.SceneGraph.GZPS;
using Sims2Tools.DBPF.SceneGraph.IDR;
using Sims2Tools.DBPF.SceneGraph.XMOL;
using Sims2Tools.DBPF.SceneGraph.XTOL;
using Sims2Tools.DBPF.STR;
using Sims2Tools.DBPF.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace OutfitOrganiser
{
    public class OutfitDbpfData : IEquatable<OutfitDbpfData>
    {
        private static OrganiserDbpfCache cache;
        public static void SetCache(OrganiserDbpfCache cache)
        {
            OutfitDbpfData.cache = cache;
        }

        private string packagePath;

        private readonly Binx binx;
        private readonly Idr idr;
        private readonly Cpf cpf;
        private readonly Str str;

        private readonly bool isAccessory = false;
        private readonly bool isClothing = false;
        private readonly bool isHair = false;
        private readonly bool isMakeUp = false;
        private readonly bool hasShoe = false;

        public string PackagePath => packagePath;

        public DBPFKey BinxKey => binx;

        public bool IsAccessory => isAccessory;
        public bool IsClothing => isClothing;
        public bool IsHair => isHair;
        public bool IsMakeUp => isMakeUp;
        public bool HasShoe => hasShoe;

        public bool IsDirty => (cpf.IsDirty || str.IsDirty || binx.IsDirty);

        public void SetClean()
        {
            cpf.SetClean();
            str.SetClean();
            binx.SetClean();
        }

        public static OutfitDbpfData Create(OrganiserDbpfFile package, DBPFKey binxKey)
        {
            return Create(package, package.GetEntryByKey(binxKey));
        }

        public static OutfitDbpfData Create(OrganiserDbpfFile package, DBPFEntry binxEntry)
        {
            OutfitDbpfData outfitData = null;

            Binx binx = (Binx)package.GetResourceByEntry(binxEntry);

            if (binx != null)
            {
                Idr idr = (Idr)package.GetResourceByTGIR(Hash.TGIRHash(binx.InstanceID, binx.ResourceID, Idr.TYPE, binx.GroupID));

                if (idr != null)
                {
                    DBPFResource res = package.GetResourceByKey(idr.Items[binx.GetItem("objectidx").UIntegerValue]);

                    if (res != null)
                    {
                        if (res is Gzps || res is Xmol || res is Xtol)
                        {
                            Cpf cpf = res as Cpf;

                            // if (cpf.GetItem("outfit")?.UIntegerValue == cpf.GetItem("parts")?.UIntegerValue)
                            {
                                if (cpf.GetItem("species").UIntegerValue == 0x00000001)
                                {
                                    outfitData = new OutfitDbpfData(package, binx, idr, cpf);
                                }
                                else
                                {
                                    // Non-human, eg dog or cat, what should we do with these?
                                }
                            }
                            /* else
                            {
                                // Report this Pets EP 'eff up!
                            } */
                        }
                    }
                }
            }

            return outfitData;
        }

        private OutfitDbpfData(OrganiserDbpfFile package, Binx binx, Idr idr, Cpf cpf)
        {
            this.packagePath = package.PackagePath;

            this.binx = binx;
            this.idr = idr;
            this.cpf = cpf;

            this.str = (Str)package.GetResourceByKey(idr.Items[binx.GetItem("stringsetidx").UIntegerValue]);

            uint outfit = Outfit;
            isAccessory = (outfit == 0x20);
            isClothing = (outfit == 0x04 || outfit == 0x08 || outfit == 0x10);
            isHair = (outfit == 0x01);
            isMakeUp = (outfit == 0x02);
            hasShoe = (outfit == 0x08 || outfit == 0x10);
        }

        public Cpf ThumbnailOwner => cpf is Xtol ? null : cpf;

        public Image Thumbnail
        {
            get
            {
                if (!(cpf is Xtol)) return null;

                Image thumbnail = null;

                using (OrganiserDbpfFile package = cache.GetOrOpen(packagePath))
                {
                    thumbnail = ((Img)package.GetResourceByKey(idr.Items[binx.GetItem("iconidx").UIntegerValue])).Image;

                    package.Close();
                }

                return thumbnail;
            }
        }

        public void Rename(string fromPackagePath, string toPackagePath)
        {
            Debug.Assert(packagePath.Equals(fromPackagePath));

            packagePath = toPackagePath;
        }

        private void UpdatePackage()
        {
            if (binx.IsDirty) cache.GetOrAdd(packagePath).Commit(binx);
            if (cpf.IsDirty) cache.GetOrAdd(packagePath).Commit(cpf);
            if (str.IsDirty) cache.GetOrAdd(packagePath).Commit(str);
        }

        public uint Outfit
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("outfit");
                uint val = (cpfItem == null) ? 0 : cpfItem.UIntegerValue;

                if (val == 0 && cpf is Xmol) val = 0x20;

                return val;
            }
        }

        public uint Shown
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("flags");
                return (cpfItem == null) ? 0 : (cpfItem.UIntegerValue & 0x01);
            }
            set
            {
                cpf.GetItem("flags").UIntegerValue = (Shown & 0xFFFFFFFE) | (value & 0x00000001);
                UpdatePackage();
            }
        }

        public uint Gender
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("gender");
                return (cpfItem == null) ? 0 : cpfItem.UIntegerValue;
            }
            set
            {
                cpf.GetItem("gender").UIntegerValue = value;
                UpdatePackage();
            }
        }

        public uint Age
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("age");
                return (cpfItem == null) ? 0 : cpfItem.UIntegerValue;
            }
            set
            {
                cpf.GetItem("age").UIntegerValue = value;
                UpdatePackage();
            }
        }

        public uint Category
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("category");
                return (cpfItem == null) ? 0 : cpfItem.UIntegerValue;
            }
            set
            {
                cpf.GetItem("category").UIntegerValue = value;
                UpdatePackage();
            }
        }

        public uint Shoe
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("shoe");
                return (cpfItem == null) ? 0 : cpfItem.UIntegerValue;
            }
            set
            {
                cpf.GetItem("shoe").UIntegerValue = value;
                UpdatePackage();
            }
        }

        public string Hairtone
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("hairtone");
                return (cpfItem == null) ? "" : cpfItem.StringValue;
            }
            set
            {
                cpf.GetItem("hairtone").StringValue = value;
                UpdatePackage();
            }
        }

        public string Title
        {
            get
            {
                CpfItem cpfItem = cpf.GetItem("name");
                return (cpfItem == null) ? "" : cpfItem.StringValue;
            }
        }

        public string Tooltip
        {
            get
            {
                return str.LanguageItems(MetaData.Languages.Default)[0].Title;
            }
            set
            {
                str.LanguageItems(MetaData.Languages.Default)[0].Title = value;
                UpdatePackage();
            }
        }

        public uint SortIndex
        {
            get
            {
                return (uint)binx.GetItem("sortindex").IntegerValue;
            }
            set
            {
                binx.GetItem("sortindex").IntegerValue = (int)value;
                UpdatePackage();
            }
        }

        public bool Equals(OutfitDbpfData other)
        {
            return this.cpf.Equals(other.cpf);
        }
    }

    public class OrganiserDbpfFile : IDisposable
    {
        private readonly DBPFFile package;
        private readonly bool isCached;

        public string PackagePath => package.PackagePath;
        public string PackageName => package.PackageName;
        public string PackageNameNoExtn => package.PackageNameNoExtn;

        public bool IsDirty => package.IsDirty;

        public OrganiserDbpfFile(string packagePath, bool isCached)
        {
            this.package = new DBPFFile(packagePath);
            this.isCached = isCached;
        }

        public List<DBPFEntry> GetEntriesByType(TypeTypeID type) => package.GetEntriesByType(type);
        public DBPFEntry GetEntryByKey(DBPFKey key) => package.GetEntryByKey(key);
        public DBPFResource GetResourceByTGIR(int tgir) => package.GetResourceByTGIR(tgir);
        public DBPFResource GetResourceByKey(DBPFKey key) => package.GetResourceByKey(key);
        public DBPFResource GetResourceByEntry(DBPFEntry entry) => package.GetResourceByEntry(entry);

        public void Commit(DBPFResource resource) => package.Commit(resource);

        public void Update(bool autoBackup) => package.Update(autoBackup);

        public void Close()
        {
            if (!isCached) package.Close();
        }

        public void Dispose()
        {
            if (!isCached) package.Dispose();
        }
    }

    public class OrganiserDbpfCache
    {
        private readonly Dictionary<string, OrganiserDbpfFile> cache = new Dictionary<string, OrganiserDbpfFile>();

        public bool Contains(string packagePath)
        {
            return cache.ContainsKey(packagePath);
        }

        public bool SetClean(string packagePath)
        {
            return cache.Remove(packagePath);
        }

        public OrganiserDbpfFile GetOrOpen(string packagePath)
        {
            if (cache.ContainsKey(packagePath))
            {
                return cache[packagePath];
            }

            return new OrganiserDbpfFile(packagePath, false);
        }

        public OrganiserDbpfFile GetOrAdd(string packagePath)
        {
            if (!cache.ContainsKey(packagePath))
            {
                cache.Add(packagePath, new OrganiserDbpfFile(packagePath, true));
            }

            return cache[packagePath];
        }
    }
}
