using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Utilities.Extensions;

namespace UI.ViewModels {
  class FamilyVM : INotifyPropertyChanged, IComparable<FamilyVM>, IEquatable<FamilyVM> {
    #region private data
    private Storage.Data.Family _model;
    private bool _installRequested = false;
    private bool _uninstallRequested = false;
    private Dictionary<string, bool> _installedFonts;
    private Dictionary<string, bool> _uninstalledFonts;

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

    public string PreviewPath { get; private set; }
    #endregion

    #region observable properties
    public bool FullyActivated {
      get {
        return _model.FullyActivated;
      }
      set {
        _installRequested = false;
        _installedFonts.Clear();
        _uninstallRequested = false;
        _uninstalledFonts.Clear();

        if (value) {
          _installRequested = true;
        } else {
          _uninstallRequested = true;
        }

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
      _installedFonts = new Dictionary<string, bool>();
      _uninstalledFonts = new Dictionary<string, bool>();

      _model = model;
      PreviewPath = _model.Fonts.FirstOrDefault(fnt => fnt.FamilyPreviewPath != null)?.FamilyPreviewPath;

      Fonts = new ObservableCollection<FontVM>(_model.Fonts.Select(fontModel => {
        return CreateFont(fontModel);
      }).OrderBy(rank));

      _model.OnFullyActivatedChanged += _model_OnFullyActivatedChanged;
      _model.OnFontAdded += _model_OnFontAdded;
      _model.OnFontRemoved += _model_OnFontRemoved;
      _model.OnFontUpdated += _model_OnFontUpdated;
    }
    #endregion

    #region methods
    public int CompareTo(FamilyVM other) {
      return _model.Name.CompareTo(other._model.Name);
    }

    public bool Equals(FamilyVM other) {
      return _model.Name == other._model.Name;
    }
    #endregion

    #region private methods
    private FontVM CreateFont(Storage.Data.Font model) {
      FontVM vm = new FontVM(model);
      vm.OnFontVMInstalled += Vm_OnFontVMInstalled;
      vm.OnFontVMUninstalled += Vm_OnFontVMUninstalled;
      return vm;
    }

    private void DeleteFont(FontVM vm) {
      vm.OnFontVMInstalled -= Vm_OnFontVMInstalled;
      vm.OnFontVMUninstalled -= Vm_OnFontVMUninstalled;
    }

    private void TriggerPropertyChanged(string propertyName) {
      ExecuteOnUIThread(() => {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      });
    }

    private void ExecuteOnUIThread(Action action) {
      Application.Current.Dispatcher.BeginInvoke(action);
    }
    #endregion

    #region event handling
    private void Vm_OnFontVMUninstalled(FontVM sender, bool success) {
      if (_uninstallRequested) {
        _uninstalledFonts[sender.UID] = success;

        if (_uninstalledFonts.Values.Count == Fonts.Count) { // all fonts processing done
          _uninstallRequested = false;

          IEnumerable<string> failedUID = _uninstalledFonts.Where(pair => !pair.Value).Select(pair => pair.Key);

          if (failedUID.Count() == Fonts.Count) { // full failure
            App.Current.ShowNotification($"{_model.Name} family uninstallation failed");
          } else if (failedUID.Count() > 0) { // some success
            IEnumerable<FontVM> failedFonts = Fonts.Where(font => failedUID.Contains(font.UID));

            App.Current.ShowNotification($"{_model.Name} family uninstalled");
            foreach (FontVM failedFont in failedFonts) {
              App.Current.ShowNotification($"{_model.Name} {failedFont.Name} uninstallation failed");
            }
          } else { // it's all good man
            App.Current.ShowNotification($"{_model.Name} family uninstalled");
          }
        }
      }
    }

    private void Vm_OnFontVMInstalled(FontVM sender, bool success) {
      if (_installRequested) {
        _installedFonts[sender.UID] = success;

        if (_installedFonts.Values.Count == Fonts.Count) { // all fonts processing done
          _installRequested = false;

          IEnumerable<string> failedUID = _installedFonts.Where(pair => !pair.Value).Select(pair => pair.Key);

          if (failedUID.Count() == Fonts.Count) { // full failure
            App.Current.ShowNotification($"{_model.Name} family installation failed");
          }
          else if (failedUID.Count() > 0) { // some success
            IEnumerable<FontVM> failedFonts = Fonts.Where(font => failedUID.Contains(font.UID));

            App.Current.ShowNotification($"{_model.Name} family installed");
            foreach (FontVM failedFont in failedFonts) {
              App.Current.ShowNotification($"{_model.Name} {failedFont.Name} installation failed");
            }
          }
          else { // it's all good man
            App.Current.ShowNotification($"{_model.Name} family installed");
          }
        }
      }
    }

    private void _model_OnFullyActivatedChanged(Storage.Data.Family sender) {
      TriggerPropertyChanged("FullyActivated");
    }

    private void _model_OnFontRemoved(Storage.Data.Family sender, Storage.Data.Font removedFont) {
      ExecuteOnUIThread(() => {
        FontVM removedVM = Fonts.FirstOrDefault(vm => vm.UID == removedFont.UID);
        if (removedVM != null) {
          Fonts.Remove(removedVM);
          DeleteFont(removedVM);
        }
      });
    }

    private void _model_OnFontAdded(Storage.Data.Family sender, Storage.Data.Font newFontModel) {
      ExecuteOnUIThread(() => {
        Fonts.SortAdd(CreateFont(newFontModel));
      });
    }

    private void _model_OnFontUpdated(Storage.Data.Family sender, Storage.Data.Font removedFont, Storage.Data.Font updatedFont) {
      ExecuteOnUIThread(() => {
        FontVM removedVM = Fonts.FirstOrDefault(vm => vm.UID == removedFont.UID);
        if (removedVM != null) {
          Fonts.Remove(removedVM);
          DeleteFont(removedVM);
        }
        Fonts.SortAdd(CreateFont(updatedFont));
      });
    }
    #endregion
  }
}
