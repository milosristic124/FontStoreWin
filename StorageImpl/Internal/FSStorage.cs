using Logging;
using Newtonsoft.Json;
using Protocol.Payloads;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Extensions;

namespace Storage.Impl.Internal {
  internal class FSStorage {
    #region private data
    // storage root directory
    private string _storageRootPath;

    // credentials save file
    private string _credFilePath;

    // font files
    private string _fontRootPath;

    private string _sessionID;
    private string _sessionPath;
    #endregion

    #region private properties
    private string _fontDBPath {
      get {
        return _sessionPath + "fnt.db";
      }
    }

    private string _metadataFilePath {
      get {
        return _sessionPath + "mta.db";
      }
    }
    #endregion

    #region properties
    public int? LastCatalogUpdate { get; set; }
    public int? LastFontStatusUpdate { get; set; }

    public string SessionID {
      get {
        return _sessionID;
      }
      set {
        if (_sessionID != value) {
          _sessionID = value.Trim();

          if (_sessionID == null) {
            _sessionPath = _storageRootPath;
          } else {
            _sessionPath = _storageRootPath + _sessionID;
            if (!_sessionPath.EndsWith("\\")) {
              _sessionPath += "\\";
            }

            if (!Directory.Exists(_sessionPath)) {
              Directory.CreateDirectory(_sessionPath);
            }
          }
        }
      }
    }
    #endregion

    #region ctor
    public FSStorage(string rootPath) {
      LastCatalogUpdate = null;
      LastFontStatusUpdate = null;

      _storageRootPath = rootPath.Trim();
      if (!_storageRootPath.EndsWith("\\")) {
        _storageRootPath += "\\";
      }
      _sessionPath = _storageRootPath;
      _sessionID = null;

      _credFilePath = _storageRootPath + "creds";

      if (!Directory.Exists(_storageRootPath)) {
        Directory.CreateDirectory(_storageRootPath);
      }


      _fontRootPath = string.Format("{0}{1}", _storageRootPath, "fnts\\");
      if (!Directory.Exists(_fontRootPath)) {
        Directory.CreateDirectory(_fontRootPath);
      }

    }

    internal FSStorage(FSStorage other): this(other._storageRootPath) {
    }
    #endregion

    #region methods
    public Task<FamilyCollection> Load(Action<Font> fontLoaded = null) {
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
            Font newFont = new Font(
              uid: fontData.UID,
              familyName: fontData.FamilyName,
              style: fontData.Name,
              downloadUrl: fontData.DownloadUrl,
              sortRank: fontData.SortRank
            );
            newFont.Activated = fontData.Activated;
            collection.AddFont(newFont);
            fontLoaded?.Invoke(newFont);
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
            return new FontData(font);
          });
        });
      })).Then(fontData => {
        return JsonConvert.SerializeObject(new List<FontData>(fontData));
      }).Then(serialization => {
        File.WriteAllText(_fontDBPath, serialization);
      });

      return Task.WhenAll(metadataSaving, fontSaving);
    }

    public Task<string> ReadCredentials() {
      return ReadData(_credFilePath).ContinueWith(stream => {
        using(Stream fileData = stream.Result) {
          if (fileData.Length <= 0) {
            return null;
          }

          using (MemoryStream buffer = new MemoryStream()) {
            fileData.CopyTo(buffer);

            string data = Encoding.UTF8.GetString(buffer.ToArray());
            CredentialsData creds = JsonConvert.DeserializeObject<CredentialsData>(data);
            return creds.Token;
          }
        }
      }, TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    public Task WriteCredential(string token) {
      CredentialsData creds = new CredentialsData() {
        Token = token
      };
      string serializedCreds = JsonConvert.SerializeObject(creds);
      MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedCreds));
      if (stream.CanSeek) // just in case
        stream.Seek(0, SeekOrigin.Begin);

      return WriteData(_credFilePath, stream);
    }

    public Task RemoveCredentials() {
      return RemoveFile(_credFilePath);
    }


    public bool FontFileExists(string uid) {
      return File.Exists(FontFilePath(uid));
    }

    public Task<Stream> ReadFontFile(string uid) {
      return ReadData(FontFilePath(uid));
    }

    public async Task SaveFontFile(string uid, Stream data, CancellationToken token) {
      await WriteData(FontFilePath(uid), data, token);
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

    private Task WriteData(string path, Stream data, CancellationToken? token = null) {
      CancellationToken cancelToken = token ?? CancellationToken.None;
      return Task.Run(delegate {
        try {
          using (data) {
            using (FileStream fileStream = File.Create(path)) {
              data.CopyTo(fileStream);
            }
          }
        } catch (Exception e) {
          Logger.Log("Saving file {0} failed: {1}", path, e);
          if (File.Exists(path)) {
            File.Delete(path);
          }
        }
      }, cancelToken);
    }

    private Task RemoveFile(string path) {
      return Task.Run(delegate {
        File.Delete(path);
      });
    }
    #endregion

    #region private type
    private class CredentialsData {
      [JsonProperty("auth_token")]
      public string Token { get; set; }
    }

    private class StorageData {
      [JsonProperty("last_catalog_update")]
      public int LastCatalogUpdate { get; set; }
      [JsonProperty("last_fonts_update")]
      public int LastFontsUpdate { get; set; }
    }

    private class FontData {
      [JsonProperty("uid")]
      public string UID { get; set; }
      [JsonProperty("family_name")]
      public string FamilyName { get; set; }
      [JsonProperty("name")]
      public string Name { get; set; }
      [JsonProperty("created_at")]
      public int CreatedAt { get; set; }
      [JsonProperty("download_url")]
      public string DownloadUrl { get; set; }
      [JsonProperty("activated")]
      public bool Activated { get; set; }
      [JsonProperty("rank")]
      public int SortRank { get; set; }

      public FontData() {
      }

      public FontData(Font font) {
        UID = font.UID;
        FamilyName = font.FamilyName;
        Name = font.Style;
        DownloadUrl = font.DownloadUrl.AbsoluteUri;
        Activated = font.Activated;
        SortRank = font.SortRank;
      }
    }
    #endregion
  }
}
