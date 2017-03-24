using System;
using System.Collections.Generic;

namespace UI.Dev {
  class DesignFamilyCollection : List<DesignFamily> {
  }

  class DesignFamily : Storage.Data.Family {
    public new string Name { get; set; }
    public new List<DesignFont> Fonts { get; set; }

    public DesignFamily(): base(null, null) {
    }
  }

  class DesignFont : Storage.Data.Font {
    public new string Name { get; set; }
    public new bool IsNew { get; set; }
    public new bool Activated { get; set; }

    public DesignFont(): base("uid", "familyname", "fontname", new Uri("http://test.com"), 0) {
    }
  }
}
