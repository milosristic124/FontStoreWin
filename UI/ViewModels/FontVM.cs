using Storage.Data;
using System;
using System.ComponentModel;
using System.Windows;

namespace UI.ViewModels {
  class FontVM : INotifyPropertyChanged {
    #region private data
    private Font _model;
    #endregion

    #region properties
    public string UID {
      get {
        return _model.UID;
      }
    }
    public string Name {
      get {
        return _model.Name;
      }
    }
    #endregion

    #region observable properties
    public bool Activated {
      get {
        return _model.Activated;
      }
      set {
        _model.Activated = value;
      }
    }
    #endregion

    #region events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region ctor
    public FontVM(Font model) {
      _model = model;
      _model.OnActivationChanged += _model_OnActivationChanged;
    }
    #endregion

    #region event handling
    private void _model_OnActivationChanged(Font sender) {
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
