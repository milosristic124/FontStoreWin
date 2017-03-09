using Protocol;
using Storage.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage {
  public delegate void UpdateFinishedHandler();

  public interface IFontStorage {
    List<Family> Families { get; }

    // load persisted family catalog
    Task Load();
    void StartUpdate();

    Font FindFont(string uid);

    event UpdateFinishedHandler OnUpdateFinished;
  }
}
