using FontInstaller;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace UI.ViewModels {
  class FontVM : INotifyPropertyChanged {
    #region private data
    private Storage.Data.Font _model;
    #endregion

    #region properties
    public string UID {
      get {
        return _model.UID;
      }
    }
    public string FamilyName {
      get {
        return _model.FamilyName;
      }
    }

    public string Name {
      get {
        return _model.Style;
      }
    }

    public int SortRank {
      get {
        return _model.SortRank;
      }
    }

    public string PreviewPath {
      get {
        return _model.PreviewPath;
      }
    }
    #endregion

    #region observable properties
    public bool Activated {
      get {
        return _model.Activated;
      }
      set {
        if (!value && _model.Activated) {
          _model.RequestDeactivation();
        }
        else if (value && !_model.Activated) {
          _model.RequestActivation();
        }
      }
    }
    #endregion

    #region events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region ctor
    public FontVM(Storage.Data.Font model) {
      _model = model;
      _model.OnActivationChanged += _model_OnActivationChanged;
    }
    #endregion

    #region event handling
    private void _model_OnActivationChanged(Storage.Data.Font sender) {
      TriggerPropertyChange("Activated");
    }
    #endregion

    #region private methods
    private void TriggerPropertyChange(string propertyName) {
      Action eventTrigger = () => {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      };

      // always ensure the event is triggered on the UI thread
      Application.Current.Dispatcher.BeginInvoke(eventTrigger);
    }
    #endregion
  }
}
