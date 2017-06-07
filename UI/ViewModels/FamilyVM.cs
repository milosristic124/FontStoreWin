using Storage.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace UI.ViewModels {
  class FamilyVM : INotifyPropertyChanged {
    #region private data
    private Family _model;

    private Func<FontVM, int> rank = (fnt) => {
      return fnt.SortRank;
    };
    #endregion

    #region properties
    public string Name {
      get {
        return _model.Name;
      }
    }

    public bool HasNewFont {
      get { return _model.HasNewFont; }
    }

    public bool HasActivatedFont {
      get { return _model.HasActivatedFont; }
    }
    #endregion

    #region observable properties
    public bool FullyActivated {
      get {
        return _model.FullyActivated;
      }
      set {
        _model.FullyActivated = value;
      }
    }

    public ObservableCollection<FontVM> Fonts { get; private set; }
    #endregion

    #region event
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region ctor
    public FamilyVM(Storage.Data.Family model) {
      _model = model;

      Fonts = new ObservableCollection<FontVM>(_model.Fonts.Select(fontModel => {
        return new FontVM(fontModel);
      }).OrderBy(rank));

      _model.OnFullyActivatedChanged += _model_OnFullyActivatedChanged;
      _model.OnFontAdded += _model_OnFontAdded;
      _model.OnFontRemoved += _model_OnFontRemoved;
      _model.OnFontUpdated += _model_OnFontUpdated;
    }
    #endregion

    #region event handling
    private void _model_OnFullyActivatedChanged(Family sender) {
      TriggerPropertyChanged("FullyActivated");
    }

    private void _model_OnFontRemoved(Family sender, Font removedFont) {
      ExecuteOnUIThread(() => {
        FontVM removedVM = Fonts.FirstOrDefault(vm => vm.UID == removedFont.UID);
        if (removedVM != null) {
          Fonts.Remove(removedVM);
        }
      });
    }

    private void _model_OnFontAdded(Family sender, Font newFontModel) {
      ExecuteOnUIThread(() => {
        Fonts.Add(new FontVM(newFontModel));
        Fonts = (ObservableCollection<FontVM>)Fonts.OrderBy(rank);
      });
    }

    private void _model_OnFontUpdated(Family sender, Font removedFont, Font updatedFont) {
      ExecuteOnUIThread(() => {
        FontVM removedVM = Fonts.FirstOrDefault(vm => vm.UID == removedFont.UID);
        if (removedVM != null) {
          Fonts.Remove(removedVM);
        }
        Fonts.Add(new FontVM(updatedFont));
      });
    }
    #endregion

    #region private methods
    private void TriggerPropertyChanged(string propertyName) {
      ExecuteOnUIThread(() => {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      });
    }

    private void ExecuteOnUIThread(Action action) {
      Application.Current.Dispatcher.BeginInvoke(action);
    }
    #endregion
  }
}
