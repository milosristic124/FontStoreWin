using Storage.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace UI.ViewModels {
  class FamilyCollectionVM: IDisposable {
    #region private data
    private FamilyCollection _model;
    private Predicate<FamilyVM> _filter;
    #endregion

    #region properties
    public ObservableCollection<FamilyVM> Families { get; private set; }
    #endregion

    #region delegates
    //public delegate void FontActivationChangedHandler();
    //public delegate void FontRemovedHandler();
    //public delegate void FontAddedHandler();
    //public delegate void FontReplacedHandler();
    #endregion

    #region events
    //public event FontActivationChangedHandler OnFontActivationChanged;
    //public event FontRemovedHandler OnFontRemoved;
    //public event FontAddedHandler OnFontAdded;
    //public event FontReplacedHandler OnFontReplaced;
    #endregion

    #region ctor
    public FamilyCollectionVM(FamilyCollection model, Predicate<FamilyVM> filter = null) {
      _model = model;
      _filter = filter;

      IEnumerable<FamilyVM> _vms = _model.Families.Select(familyModel => {
        return new FamilyVM(familyModel);
      });

      if (_filter != null) {
        Families = new ObservableCollection<FamilyVM>(_vms.Where(family => {
          return _filter(family);
        }));
      } else {
        Families = new ObservableCollection<FamilyVM>(_vms);
      }

      //Families = new ObservableCollection<FamilyVM>(_model.Families.Select(familyModel => {
      //  return new FamilyVM(familyModel);
      //}));

      _model.OnCollectionCleared += _model_OnCollectionCleared;
      _model.OnFamilyAdded += _model_OnFamilyAdded;
      _model.OnFamilyRemoved += _model_OnFamilyRemoved;

      _model.OnFontAdded += _model_OnFontAdded;
      _model.OnFontRemoved += _model_OnFontRemoved;
      _model.OnFontUpdated += _model_OnFontUpdated;
      _model.OnActivationChanged += _model_OnActivationChanged;
    }

    public void Dispose() {
      _model.OnCollectionCleared -= _model_OnCollectionCleared;
      _model.OnFamilyAdded -= _model_OnFamilyAdded;
      _model.OnFamilyRemoved -= _model_OnFamilyRemoved;

      _model.OnFontAdded -= _model_OnFontAdded;
      _model.OnFontRemoved -= _model_OnFontRemoved;
      _model.OnFontUpdated -= _model_OnFontUpdated;
      _model.OnActivationChanged -= _model_OnActivationChanged;
    }
    #endregion

    #region event handling
    private void _model_OnFamilyRemoved(FamilyCollection sender, Family removedFamily) {
      ExecuteOnUIThread(() => {
        FamilyVM removedVM = Families.FirstOrDefault(vm => vm.Name == removedFamily.Name);
        if (removedVM != null) {
          Families.Remove(removedVM);
        }
      });
    }

    private void _model_OnFamilyAdded(FamilyCollection sender, Family newFamily) {
      ExecuteOnUIThread(() => {
        FamilyVM existingVm = Families.FirstOrDefault(fam => fam.Name == newFamily.Name);
        if (existingVm == null) {
          FamilyVM vm = new FamilyVM(newFamily);
          if (!FilterOut(vm)) {
            Families.Add(vm);
          }
        }
      });
    }

    private void _model_OnCollectionCleared(FamilyCollection sender) {
      ExecuteOnUIThread(() => {
        Families.Clear();
      });
    }

    private void _model_OnActivationChanged(FamilyCollection sender, Family fontFamily, Font target) {
      ExecuteOnUIThread(() => {
        FamilyVM existingVm = Families.FirstOrDefault(fam => fam.Name == fontFamily.Name);
        FamilyVM vm = existingVm;
        if (vm == null) {
          vm = new FamilyVM(fontFamily);
        }

        // family was not in the collection && family should be in the collection
        if (existingVm == null && !FilterOut(vm)) {
          Families.Add(vm);
        }
        // family was in the collection && family should not be in the collection
        else if (existingVm != null && FilterOut(existingVm)) {
          Families.Remove(existingVm);
        }
        //OnFontActivationChanged?.Invoke();
      });
    }

    private void _model_OnFontUpdated(FamilyCollection sender, Family target, Font removedFont, Font updatedFont) {
      ExecuteOnUIThread(() => {
        FamilyVM existingVm = Families.FirstOrDefault(fam => fam.Name == target.Name);
        FamilyVM vm = existingVm;
        if (vm == null) {
          vm = new FamilyVM(target);
        }

        // family was not in the collection && family should be in the collection
        if (existingVm == null && !FilterOut(vm)) {
          Families.Add(vm);
        }
        // family was in the collection && family should not be in the collection
        else if (existingVm != null && FilterOut(vm)) {
          Families.Remove(existingVm);
        }
        //OnFontReplaced?.Invoke();
      });
    }

    private void _model_OnFontRemoved(FamilyCollection sender, Family target, Font oldFont) {
      ExecuteOnUIThread(() => {
        FamilyVM existingVm = Families.FirstOrDefault(fam => fam.Name == target.Name);
        FamilyVM vm = existingVm;
        if (vm == null) {
          vm = new FamilyVM(target);
        }

        // family was not in the collection && family should be in the collection
        if (existingVm == null && !FilterOut(vm)) {
          Families.Add(vm);
        }
        // family was in the collection && family should not be in the collection
        else if (existingVm != null && FilterOut(existingVm)) {
          Families.Remove(existingVm);
        }
        //OnFontRemoved?.Invoke();
      });
    }

    private void _model_OnFontAdded(FamilyCollection sender, Family target, Font newFont) {
      ExecuteOnUIThread(() => {
        FamilyVM existingVm = Families.FirstOrDefault(fam => fam.Name == target.Name);
        FamilyVM vm = existingVm;
        if (vm == null) {
          vm = new FamilyVM(target);
        }

        // family was not in the collection && family should be in the collection
        if (existingVm == null && !FilterOut(vm)) {
          Families.Add(vm);
        }
        // family was in the collection && family should not be in the collection
        else if (existingVm != null && FilterOut(existingVm)) {
          Families.Remove(existingVm);
        }
        //OnFontAdded?.Invoke();
      });
    }
    #endregion

    #region private methods
    private bool FilterOut(FamilyVM vm) {
      if (_filter == null) // no filter, nothing is filtered out
        return false;

      return !_filter(vm);
    }

    private void ExecuteOnUIThread(Action action) {
      Application.Current.Dispatcher.BeginInvoke(action);
    }
    #endregion
  }
}
