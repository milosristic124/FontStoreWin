using FontInstaller;
using Storage;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace UI.ViewModels {
  class FontVM : INotifyPropertyChanged, IComparable<FontVM>, IEquatable<FontVM> {
    #region private data
    private Storage.Data.Font _model;
    private bool _installRequested = false;
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
          _installRequested = true;
          _model.RequestDeactivation();
        }
        else if (value && !_model.Activated) {
          _installRequested = true;
          _model.RequestActivation();
        }
      }
    }
    #endregion

    #region delegates
    public delegate void FontVMInstallationHandler(FontVM sender, bool success);
    #endregion

    #region events
    public event PropertyChangedEventHandler PropertyChanged;
    public event FontVMInstallationHandler OnFontVMInstalled;
    public event FontVMInstallationHandler OnFontVMUninstalled;
    #endregion

    #region ctor
    public FontVM(Storage.Data.Font model) {
      _model = model;
      _model.OnActivationChanged += _model_OnActivationChanged;
      _model.OnFontInstalled += _model_OnFontInstalled;
      _model.OnFontUninstalled += _model_OnFontUninstalled;
    }
    #endregion

    #region methods
    public int CompareTo(FontVM other) {
      return _model.SortRank - other._model.SortRank;
    }

    public bool Equals(FontVM other) {
      return _model.SortRank == other._model.SortRank;
    }
    #endregion

    #region event handling
    private void _model_OnActivationChanged(Storage.Data.Font sender) {
      TriggerPropertyChange("Activated");
    }

    private void _model_OnFontUninstalled(Storage.Data.Font sender, bool success) {
      if (_installRequested) {
        if (success)
          App.Current.ShowNotification($"{sender.FamilyName} {sender.Style} uninstalled");
        else
          App.Current.ShowNotification($"{sender.FamilyName} {sender.Style} uninstallation failed");
      } else {
        OnFontVMUninstalled?.Invoke(this, success);
      }
      _installRequested = false;
    }

    private void _model_OnFontInstalled(Storage.Data.Font sender, bool success) {
      if (_installRequested) {
        if (success)
          App.Current.ShowNotification($"{sender.FamilyName} {sender.Style} installed");
        else
          App.Current.ShowNotification($"{sender.FamilyName} {sender.Style} installation failed");
      } else {
        OnFontVMInstalled?.Invoke(this, success);
      }
      _installRequested = false;
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
