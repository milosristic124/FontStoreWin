using Protocol;
using Storage.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage {
  public delegate void UpdateFinishedHandler();

  public interface IFontStorage {
    List<Family> Families { get; }

    Task Load();
    void StartUpdate();

    Font FindFont(string uid);

    event UpdateFinishedHandler OnUpdateFinished;
  }
}
