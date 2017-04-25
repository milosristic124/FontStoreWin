using System;
using System.Collections.Generic;

namespace UI.Dev {
  class DesignFamilyCollection : List<DesignFamily> {
  }

  class DesignFamily : ViewModels.FamilyVM {
    public new string Name { get; set; }
    public new List<DesignFont> Fonts { get; set; }

    public DesignFamily(): base(new Storage.Data.Family(null, null)) {
    }
  }

  class DesignFont : ViewModels.FontVM {
    public new string Name { get; set; }
    public new bool Activated { get; set; }

    public DesignFont(): base(new Storage.Data.Font("uid", "familyname", "fontname", "http://test.com")) {
    }
  }
}
