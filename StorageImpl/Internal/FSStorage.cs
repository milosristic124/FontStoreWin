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

namespace Storage.Impl.Internal {
  internal class FSStorage {
    #region private data
    // storage root directory
    private string _storageRootPath;

    // font files
    private string _fontRootPath;

    // db files
    private string _metadataFilePath;
    private string _fontDBPath;
    #endregion

    #region properties
    public DateTime? LastCatalogUpdate { get; set; }
    public DateTime? LastFontStatusUpdate { get; set; }
    #endregion

    #region ctor
    public FSStorage(string rootPath) {
      LastCatalogUpdate = null;
      LastFontStatusUpdate = null;

      _storageRootPath = rootPath.Trim();
      if (!_storageRootPath.EndsWith("\\")) {
        _storageRootPath += "\\";
      }

      if (!Directory.Exists(_storageRootPath)) {
        Directory.CreateDirectory(_storageRootPath);
      }


      _fontRootPath = string.Format("{0}{1}", _storageRootPath, "fnts\\");
      if (!Directory.Exists(_fontRootPath)) {
        Directory.CreateDirectory(_fontRootPath);
      }

      _fontDBPath = string.Format("{0}{1}", _storageRootPath, "fnt.db");
      _metadataFilePath = string.Format("{0}{1}", _storageRootPath, "mta.db");
    }
    #endregion

    #region methods
    public Task<FamilyCollection> Load() {
      LastCatalogUpdate = null;
      LastFontStatusUpdate = null;
      FamilyCollection collection = new FamilyCollection();

      // pop a thread to read metadata
      Task metadataLoading = ReadText(_metadataFilePath).Then(json => {
        if (json != null) {
          StorageData metadata = JsonConvert.DeserializeObject<StorageData>(json);
          LastCatalogUpdate = metadata.LastCatalogUpdate;
          LastFontStatusUpdate = metadata.LastFontsUpdate;
        }
      });

      // pop a thread to read font data
      Task fontLoading = ReadText(_fontDBPath).Then(json => {
        if (json != null) {
          foreach (FontData fontData in JsonConvert.DeserializeObject<List<FontData>>(json)) {
            Font newFont = new Font(fontData);
            newFont.Activated = fontData.Activated;
            collection.AddFont(newFont);
          }
        }
      });

      // when everything is done we have loaded all the data
      return Task.WhenAll(metadataLoading, fontLoading).Then(() => {
        return collection;
      });
    }

    public Task Save(FamilyCollection collection) {

      Task metadataSaving;
      if (LastCatalogUpdate.HasValue && LastFontStatusUpdate.HasValue) {
        metadataSaving = Task.Run(() => {
          return JsonConvert.SerializeObject(new StorageData() {
            LastCatalogUpdate = LastCatalogUpdate.Value,
            LastFontsUpdate = LastFontStatusUpdate.Value
          });
        }).Then(serialization => {
          File.WriteAllText(_metadataFilePath, serialization);
        });
      } else {
        metadataSaving = Task.Run(() => { });
      }

      Task fontSaving = Task.WhenAll(collection.Families.SelectMany(family => {
        return family.Fonts.Select(font => {
          return Task.Run(() => {
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
        File.WriteAllText(_fontDBPath, serialization);
      });

      return Task.WhenAll(metadataSaving, fontSaving);
    }

    public Task<Stream> ReadFontFile(string uid) {
      return ReadData(FontFilePath(uid));
    }

    public async Task SaveFontFile(string uid, Stream data) {
      await WriteData(FontFilePath(uid), data);
    }

    public async Task RemoveFontFile(string uid) {
      await RemoveFile(FontFilePath(uid));
    }
    #endregion

    #region private methods
    private string FontFilePath(string uid) {
      return string.Format("{0}{1}", _fontRootPath, uid);
    }

    private Task<Stream> ReadData(string path) {
      return Task.Run<Stream>(() => {
        if (!File.Exists(path)) {
          return new MemoryStream();
        } else {
          return File.OpenRead(path);
        }
      });
    }

    private Task<string> ReadText(string path) {
      return Task.Run(() => {
        if (File.Exists(path)) {
          return File.ReadAllText(path, Encoding.UTF8);
        }
        else {
          return null;
        }
      });
    }

    private Task WriteData(string path, Stream data) {
      return Task.Run(delegate {
        using (FileStream fileStream = File.Create(path)) {
          data.CopyTo(fileStream);
        }
      });
    }

    private Task RemoveFile(string path) {
      return Task.Run(delegate {
        File.Delete(path);
      });
    }
    #endregion

    #region private type
    private class StorageData {
      [JsonProperty("last_catalog_update")]
      public DateTime LastCatalogUpdate { get; set; }
      [JsonProperty("last_fonts_update")]
      public DateTime LastFontsUpdate { get; set; }
    }

    private class FontData : FontDescription {
      [JsonProperty("activated")]
      public bool Activated { get; set; }
    }
    #endregion
  }
}
