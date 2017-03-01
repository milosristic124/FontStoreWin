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

namespace Storage.Impl {
  public class FSFontStorage : IFontStorage {
    #region private members
    private string _storageRoot;
    private string _fontFile;
    private string _metaFile;

    private DateTime? _lastCatalogUpdate;
    private DateTime? _lastFontsUpdate;
    private AutoResetEvent _updateFinishedEvent;
    private IConnection _connection;
    #endregion

    #region properties
    public List<Family> Families { get; }
    public bool Loaded { get; private set; }

    public event UpdateFinishedHandler OnUpdateFinished;
    #endregion

    #region ctor
    public FSFontStorage(IConnection connection) : this(connection, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) {
    }

    public FSFontStorage(IConnection connection, string rootPath) {
      Families = new List<Family>();
      Loaded = false;

      _storageRoot = rootPath.Trim();
      if (!_storageRoot.EndsWith("\\")) {
        _storageRoot += "\\";
      }

      _lastCatalogUpdate = null;
      _lastFontsUpdate = null;

      if (!Directory.Exists(_storageRoot)) {
        Directory.CreateDirectory(_storageRoot);
      }

      _fontFile = string.Format("{0}{1}", _storageRoot, "fnt.db");
      _metaFile = string.Format("{0}{1}", _storageRoot, "mta.db");

      _updateFinishedEvent = new AutoResetEvent(false);

      _connection = connection;
    }
    #endregion

    #region public interface
    public Task Load() {
      if (Loaded) {
        return Task.Factory.StartNew(() => { });
      }

      // pop a thread to read metadata
      Task metadataLoading = ReadData(_metaFile).ContinueWith(readTask => {
        string json = readTask.Result;
        if (json != null) {
          StorageData metadata = JsonConvert.DeserializeObject<StorageData>(json);
          _lastCatalogUpdate = metadata.LastCatalogUpdate;
          _lastFontsUpdate = metadata.LastFontsUpdate;
        }
      });

      // pop a thread to read font data
      Task fontLoading = ReadData(_fontFile).ContinueWith(readTask => {
        string json = readTask.Result;
        if (json != null) {
          foreach (FontData fontData in JsonConvert.DeserializeObject<List<FontData>>(json)) {
            AddFont(fontData).Activated = fontData.Activated;
          }
        }
      });

      // when everything is done we have loaded all the data
      return Task.WhenAll(metadataLoading, fontLoading).ContinueWith(_ => {
        Loaded = true;
      });
    }

    public async void StartUpdate() {
      if (!Loaded) {
        throw new InvalidOperationException("FontStorage.Load must be called before attempting setup.");
      }

      _connection.OnUpdateFinished += _connection_OnUpdateFinished;
      _connection.OnFontDesctiptionReceived += _connection_OnFontDesctiptionReceived;
      _connection.OnFontDeleted += _connection_OnFontDeleted;
      _connection.OnFontActivated += _connection_OnFontActivated;
      _connection.OnFontDeactivated += _connection_OnFontDeactivated;

      // pop a new thread for the update
      Task updateTask = Task.Factory.StartNew(async () => {
        // start the catalog update
        _connection.UpdateCatalog(_lastCatalogUpdate);
        // block the update thread until the catalog update finished event is received
        _updateFinishedEvent.WaitOne();

        _lastCatalogUpdate = DateTime.Now;

        // start the fonts status update
        _connection.UpdateFontsStatus(_lastFontsUpdate);
        // block the update thread until the fonts update finished event is received
        _updateFinishedEvent.WaitOne();

        _lastFontsUpdate = DateTime.Now;

        // save all received data
        await Save();
      });

      await updateTask;

      _connection.OnUpdateFinished -= _connection_OnUpdateFinished;

      OnUpdateFinished?.Invoke();
    }

    public Font FindFont(string uid) {
      return FindFamilyByFontUID(uid)?.FindFond(uid);
    }
    #endregion

    #region events handling
    private void _connection_OnUpdateFinished() {
      _updateFinishedEvent.Set();
    }

    private void _connection_OnFontDeleted(string uid) {
      Family family = FindFamilyByFontUID(uid);

      if (family != null) {
        family.Remove(uid);
        if (family.Fonts.Count == 0) {
          Families.Remove(family);
        }
      }
    }

    private void _connection_OnFontDesctiptionReceived(FontDescription description) {
      AddFont(description);
    }

    private void _connection_OnFontDeactivated(string uid) {
      Font font = FindFont(uid);
      if (font != null)
        font.Activated = false;
    }

    private void _connection_OnFontActivated(string uid) {
      Font font = FindFont(uid);
      if (font != null)
        font.Activated = true;
    }
    #endregion

    #region private db management
    private Font AddFont(FontDescription description) {
      Font newFont = new Font(description);
      Family family = FindFamilyByName(newFont.FamilyName);

      if (family != null) {
        family.Add(newFont);
      }
      else {
        family = new Family(newFont.FamilyName, new List<Font> { newFont });
        Families.Add(family);
      }
      return newFont;
    }

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
    private Task Save() {
      if (!Loaded) {
        return Task.Factory.StartNew(() => { });
      }

      Task metadataSaving = Task.Factory.StartNew(() => {
        return JsonConvert.SerializeObject(new StorageData() {
          LastCatalogUpdate = _lastCatalogUpdate.Value,
          LastFontsUpdate = _lastFontsUpdate.Value
        });
      }).ContinueWith(serialization => {
        File.WriteAllText(_metaFile, serialization.Result);
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
      })).ContinueWith(collectTask => {
        return JsonConvert.SerializeObject(new List<FontData>(collectTask.Result));
      }).ContinueWith(serialization => {
        File.WriteAllText(_fontFile, serialization.Result);
      });

      return Task.WhenAll(metadataSaving, fontSaving);
    }

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
