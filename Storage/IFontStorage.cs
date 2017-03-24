﻿using Protocol.Payloads;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Storage {
  public interface IFontStorage {
    #region properties
    DateTime? LastCatalogUpdate { get; set; }
    DateTime? LastFontStatusUpdate { get; set; }

    bool Loaded { get; }
    bool HasChanged { get; }

    IList<Family> ActivatedFamilies { get; }
    IList<Family> NewFamilies { get; }
    IList<Family> Families { get; }
    #endregion

    #region methods
    Task Load();
    Task Save();

    Font AddFont(FontDescription description);
    void RemoveFont(string uid);
    void ActivateFont(string uid);
    void DeactivateFont(string uid);

    Font FindFont(string uid);

    bool IsFontDownloaded(string uid);
    Task SaveFontFile(string uid, Stream data);
    #endregion
  }
}
