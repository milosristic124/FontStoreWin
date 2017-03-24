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
    #endregion

    #region properties
    public string Name {
      get {
        return _model.Name;
      }
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
      }));

      _model.OnFullyActivatedChanged += _model_OnFullyActivatedChanged;
      _model.OnFontAdded += _model_OnFontAdded;
      _model.OnFontRemoved += _model_OnFontRemoved;
    }
    #endregion

    #region event handling
    private void _model_OnFullyActivatedChanged(Family sender) {
      TriggerPropertyChanged("FullyActivated");
    }

    private void _model_OnFontRemoved(Family sender, Font removedFont) {
      ExecuteOnUIThread(() => {
        FontVM removedVM = Fonts.FirstOrDefault(vm => vm.UID == removedFont.UID);
        Fonts.Remove(removedVM);
      });
    }

    private void _model_OnFontAdded(Family sender, Font newFontModel) {
      ExecuteOnUIThread(() => {
        Fonts.Add(new FontVM(newFontModel));
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
