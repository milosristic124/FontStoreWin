using System.Collections.Generic;

namespace Storage {
  public class Family {
    public string Name { get; private set; }
    public List<Font> Fonts { get; private set; }

    public Family(string name, List<Font> fonts = null) {
      this.Name = name;

      if (fonts != null)
        this.Fonts = fonts;
      else
        this.Fonts = new List<Font>();
    }

    public bool AddFont(Font font) {
      if (font.FamilyName != this.Name)
        return false;

      int index = this.FontIndex(font.Uid);
      if (index >= 0) {
        this.Fonts.RemoveAt(index);
      }
      this.Fonts.Add(font);

      return true;
    }

    public void RemoveFont(string uid) {
      int index = this.FontIndex(uid);
      if (index >= 0) {
        this.Fonts.RemoveAt(index);
      }
    }

    private int FontIndex(string uid) {
      return this.Fonts.FindIndex((Font existingFont) => {
        return existingFont.Uid == uid;
      });
    }
  }
}
