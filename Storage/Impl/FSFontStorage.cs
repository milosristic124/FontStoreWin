using Newtonsoft.Json;
using Protocol;
using Protocol.Payloads;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Extensions;

namespace Storage.Impl {
  public class FSFontStorage : IFontStorage {
    #region private members
    private string _storageRoot;
    private string _fontFile;
    private string _metaFile;
    #endregion

    #region properties
    public List<Family> Families { get; }
    public DateTime? LastCatalogUpdate { get; private set; }
    public DateTime? LastFontStatusUpdate { get; private set; }

    public bool Loaded { get; private set; }
    public bool HasChanged { get; private set; }
    #endregion

    #region ctor
    public FSFontStorage() : this(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) {
    }

    public FSFontStorage(string rootPath) {
      Families = new List<Family>();
      Loaded = false;
      HasChanged = false;

      _storageRoot = rootPath.Trim();
      if (!_storageRoot.EndsWith("\\")) {
        _storageRoot += "\\";
      }

      LastCatalogUpdate = null;
      LastFontStatusUpdate = null;

      if (!Directory.Exists(_storageRoot)) {
        Directory.CreateDirectory(_storageRoot);
      }

      _fontFile = string.Format("{0}{1}", _storageRoot, "fnt.db");
      _metaFile = string.Format("{0}{1}", _storageRoot, "mta.db");

      //_updateFinishedEvent = new AutoResetEvent(false);

      //_connection = connection;
    }
    #endregion

    #region methods
    public Task Load() {
      if (Loaded) {
        return Task.Factory.StartNew(() => { });
      }

      LastCatalogUpdate = null;
      LastFontStatusUpdate = null;
      Families.Clear();

      // pop a thread to read metadata
      Task metadataLoading = ReadData(_metaFile).Then(json => {
        if (json != null) {
          StorageData metadata = JsonConvert.DeserializeObject<StorageData>(json);
          LastCatalogUpdate = metadata.LastCatalogUpdate;
          LastFontStatusUpdate = metadata.LastFontsUpdate;
        }
      });

      // pop a thread to read font data
      Task fontLoading = ReadData(_fontFile).Then(json => {
        if (json != null) {
          foreach (FontData fontData in JsonConvert.DeserializeObject<List<FontData>>(json)) {
            AddFont(fontData).Activated = fontData.Activated;
          }
        }
      });

      // when everything is done we have loaded all the data
      return Task.WhenAll(metadataLoading, fontLoading).Then(() => {
        Loaded = true;
      });
    }

    public Task Save() {
      if (!HasChanged) {
        return Task.Factory.StartNew(() => { });
      }

      if (!LastCatalogUpdate.HasValue) {
        LastCatalogUpdate = DateTime.Now;
      }
      if (!LastFontStatusUpdate.HasValue) {
        LastFontStatusUpdate = DateTime.Now;
      }


      Task metadataSaving = Task.Factory.StartNew(() => {
        return JsonConvert.SerializeObject(new StorageData() {
          LastCatalogUpdate = LastCatalogUpdate.Value,
          LastFontsUpdate = LastFontStatusUpdate.Value
        });
      }).Then(serialization => {
        File.WriteAllText(_metaFile, serialization);
      });

      Task fontSaving = Task.WhenAll(Families.SelectMany(family => {
        return family.Fonts.Select(font => {
          return Task.Factory.StartNew(() => {
            return new FontData() {
              UID = font.UID,
              FamilyName = font.FamilyName,
              Name = font.Name,
              IsNew = font.IsNew,
              DownloadUrl = font.DownloadUrl.AbsoluteUri,
              Activated = font.Activated
            };
          });
        });
      })).Then(fontData => {
        return JsonConvert.SerializeObject(new List<FontData>(fontData));
      }).Then(serialization => {
        File.WriteAllText(_fontFile, serialization);
      });

      return Task.WhenAll(metadataSaving, fontSaving);
    }

    public Font AddFont(FontDescription description) {
      Font newFont = new Font(description);
      Family family = FindFamilyByName(newFont.FamilyName);

      if (family != null) {
        family.Add(newFont);
      }
      else {
        family = new Family(newFont.FamilyName, new List<Font> { newFont });
        Families.Add(family);
      }

      HasChanged = true;
      return newFont;
    }

    public void RemoveFont(string uid) {
      Family family = FindFamilyByFontUID(uid);

      if (family != null) {
        family.Remove(uid);
        if (family.Fonts.Count == 0) {
          Families.Remove(family);
        }
      }
      HasChanged = true;
    }

    public void ActivateFont(string uid) {
      Font font = FindFont(uid);
      if (font != null) {
        font.Activated = true;
      }
      HasChanged = true;
    }

    public void DeactivateFont(string uid) {
      Font font = FindFont(uid);
      if (font != null) {
        font.Activated = false;
      }
      HasChanged = true;
    }

    public Font FindFont(string uid) {
      return FindFamilyByFontUID(uid)?.FindFond(uid);
    }
    #endregion

    #region private db management
    private Family FindFamilyByName(string familyName) {
      return Families.Find(family => {
        return family.Name == familyName;
      });
    }

    private Family FindFamilyByFontUID(string uid) {
      return Families.Find(family => {
        return family.FindFond(uid) != null;
      });
    }
    #endregion

    #region FS
    private Task<string> ReadData(string path) {
      return Task.Factory.StartNew<string>(() => {
        if (File.Exists(path)) {
          return File.ReadAllText(path, Encoding.UTF8);
        }
        else {
          return null;
        }
      });
    }

    private class StorageData {
      [JsonProperty("last_catalog_update")]
      public DateTime LastCatalogUpdate { get; set; }
      [JsonProperty("last_fonts_update")]
      public DateTime LastFontsUpdate { get; set; }
    }

    private class FontData: FontDescription {
      [JsonProperty("activated")]
      public bool Activated { get; set; }
    }
    #endregion
  }
}
