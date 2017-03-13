using Protocol.Payloads;
using Storage.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage {
  public interface IFontStorage {
    List<Family> Families { get; }
    DateTime? LastCatalogUpdate { get; set; }
    DateTime? LastFontStatusUpdate { get; set; }
    bool Loaded { get; }
    bool HasChanged { get; }

    Task Load();
    Task Save();

    Font AddFont(FontDescription description);
    void RemoveFont(string uid);
    void ActivateFont(string uid);
    void DeactivateFont(string uid);

    Font FindFont(string uid);
  }
}
