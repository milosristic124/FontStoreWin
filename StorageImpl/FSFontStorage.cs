using Newtonsoft.Json;
using Protocol.Payloads;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Extensions;

namespace Storage.Impl {
  public class FSFontStorage : IFontStorage {
    #region private members
    private string _storageRoot;
    private string _fontFile;
    private string _metaFile;
    private string _fontRoot;
    #endregion

    #region properties
    public DateTime? LastCatalogUpdate { get; set; }
    public DateTime? LastFontStatusUpdate { get; set; }

    public bool Loaded { get; private set; }
    public bool HasChanged { get; private set; }

    public IList<Family> ActivatedFamilies {
      get {
        return FamilyCollection.Filtered(family => {
          return family.HasActivatedFont;
        }).ToList();
      }
    }
    public IList<Family> NewFamilies {
      get {
        return FamilyCollection.Filtered((family) => {
          return family.HasNewFont;
        }).ToList();
      }
    }
    public FamilyCollection FamilyCollection { get; private set; }
    #endregion

    #region ctor
    public FSFontStorage() : this(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/Fontstore") {
    }

    public FSFontStorage(string rootPath) {
      FamilyCollection = new FamilyCollection();
      Loaded = false;
      HasChanged = false;

      LastCatalogUpdate = null;
      LastFontStatusUpdate = null;

      _storageRoot = rootPath.Trim();
      if (!_storageRoot.EndsWith("\\")) {
        _storageRoot += "\\";
      }

      if (!Directory.Exists(_storageRoot)) {
        Directory.CreateDirectory(_storageRoot);
      }

      _fontRoot = string.Format("{0}{1}", _storageRoot, "fnts\\");
      if (!Directory.Exists(_fontRoot)) {
        Directory.CreateDirectory(_fontRoot);
      }

      _fontFile = string.Format("{0}{1}", _storageRoot, "fnt.db");
      _metaFile = string.Format("{0}{1}", _storageRoot, "mta.db");
    }
    #endregion

    #region methods
    public Task Load() {
      if (Loaded) {
        return Task.Factory.StartNew(() => { });
      }

      LastCatalogUpdate = null;
      LastFontStatusUpdate = null;
      FamilyCollection.Clear();

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

      LastCatalogUpdate = DateTime.Now;
      LastFontStatusUpdate = DateTime.Now;


      Task metadataSaving = Task.Factory.StartNew(() => {
        return JsonConvert.SerializeObject(new StorageData() {
          LastCatalogUpdate = LastCatalogUpdate.Value,
          LastFontsUpdate = LastFontStatusUpdate.Value
        });
      }).Then(serialization => {
        File.WriteAllText(_metaFile, serialization);
      });

      Task fontSaving = Task.WhenAll(FamilyCollection.Families.SelectMany(family => {
        return family.Fonts.Select(font => {
          return Task.Factory.StartNew(() => {
            return new FontData() {
              UID = font.UID,
              FamilyName = font.FamilyName,
              Name = font.Name,
              CreatedAt = font.Description.CreatedAt,
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
      FamilyCollection.AddFont(newFont);

      HasChanged = true;
      return newFont;
    }

    public void RemoveFont(string uid) {
      FamilyCollection.RemoveFont(uid);
      HasChanged = true;
    }

    public void ActivateFont(string uid) {
      Font font = FamilyCollection.FindFont(uid);
      if (font != null) {
        font.Activated = true;
        HasChanged = true;
      }
    }

    public void DeactivateFont(string uid) {
      Font font = FamilyCollection.FindFont(uid);
      if (font != null) {
        font.Activated = false;
        HasChanged = true;
      }
    }

    public Font FindFont(string uid) {
      return FamilyCollection.FindFont(uid);
    }

    public bool IsFontDownloaded(string uid) {
      return File.Exists(FontFilePath(uid));
    }

    public Task SaveFontFile(string uid, Stream data) {
      return WriteData(FontFilePath(uid), data);
    }
    #endregion

    #region FS
    private Task<string> ReadData(string path) {
      return Task.Factory.StartNew(() => {
        if (File.Exists(path)) {
          return File.ReadAllText(path, Encoding.UTF8);
        }
        else {
          return null;
        }
      });
    }

    private string FontFilePath(string uid) {
      return string.Format("{0}{1}", _fontRoot, uid);
    }

    private Task WriteData(string path, Stream data) {
      return Task.Factory.StartNew(delegate {
        using (FileStream fileStream = File.Create(path)) {
          data.CopyTo(fileStream);
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
