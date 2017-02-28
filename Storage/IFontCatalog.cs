using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storage {
  public interface IFontCatalog {
    IList<Family> Families { get; }

    Task update();
  }
}
