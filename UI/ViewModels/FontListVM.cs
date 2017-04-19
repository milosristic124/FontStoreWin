using System.Windows.Data;

namespace UI.ViewModels {
  class FontListVM {
    #region properties
    public FamilyCollectionVM InstalledVM { get; set; }
    public FamilyCollectionVM NewVM { get; set; }
    public FamilyCollectionVM AllVM { get; set; }

    public ListCollectionView SearchCollection { get; set; }
    #endregion
  }
}
