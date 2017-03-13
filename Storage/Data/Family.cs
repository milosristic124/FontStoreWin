using System.Collections.Generic;

namespace Storage.Data {
  public class Family {
    public string Name { get; private set; }
    public List<Font> Fonts { get; private set; }

    public Family(string name, List<Font> fonts = null) {
      Name = name;
      Fonts = fonts;

      if (Fonts == null) {
        Fonts = new List<Font>();
      }
    }

    public void Add(Font font) {
      int index = FontIndex(font.UID);

      if (index >= 0) {
        Fonts.RemoveAt(index);
      }

      Fonts.Add(font);
    }

    public void Remove(string uid) {
      int index = FontIndex(uid);

      if (index >= 0) {
        Fonts.RemoveAt(index);
      }
    }

    public Font FindFont(string uid) {
      return Fonts.Find(font => {
        return font.UID == uid;
      });
    }

    private int FontIndex(string uid) {
      return Fonts.FindIndex(font => {
        return font.UID == uid;
      });
    }
  }
}
