﻿using System;
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

    public DesignFont(): base(new Storage.Data.Font("uid", "familyname", "fontname", 0, "http://test.com", "http://test.com/preview.png", "http://test.com/family_preview.png")) {
    }
  }
}
