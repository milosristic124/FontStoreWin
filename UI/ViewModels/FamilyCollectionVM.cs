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
    #endregion

    #region properties
    public ObservableCollection<FamilyVM> Families { get; private set; }
    #endregion

    #region delegates
    public delegate void FontActivationChangedHandler();
    public delegate void FontRemovedHandler();
    public delegate void FontAddedHandler();
    public delegate void FontReplacedHandler();
    #endregion

    #region events
    public event FontActivationChangedHandler OnFontActivationChanged;
    public event FontRemovedHandler OnFontRemoved;
    public event FontAddedHandler OnFontAdded;
    public event FontReplacedHandler OnFontReplaced;
    #endregion

    #region ctor
    public FamilyCollectionVM(FamilyCollection model) {
      _model = model;
      Families = new ObservableCollection<FamilyVM>(_model.Families.Select(familyModel => {
        return new FamilyVM(familyModel);
      }));

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
        Families.Remove(removedVM);
      });
    }

    private void _model_OnFamilyAdded(FamilyCollection sender, Family newFamily) {
      ExecuteOnUIThread(() => {
        Families.Add(new FamilyVM(newFamily));
      });
    }

    private void _model_OnCollectionCleared(FamilyCollection sender) {
      ExecuteOnUIThread(() => {
        Families.Clear();
      });
    }

    private void _model_OnActivationChanged(FamilyCollection sender, Family fontFamily, Font target) {
      ExecuteOnUIThread(() => {
        OnFontActivationChanged?.Invoke();
      });
    }

    private void _model_OnFontUpdated(FamilyCollection sender, Family target, Font removedFont, Font updatedFont) {
      ExecuteOnUIThread(() => {
        OnFontReplaced?.Invoke();
      });
    }

    private void _model_OnFontRemoved(FamilyCollection sender, Family target, Font oldFont) {
      ExecuteOnUIThread(() => {
        OnFontRemoved?.Invoke();
      });
    }

    private void _model_OnFontAdded(FamilyCollection sender, Family target, Font newFont) {
      ExecuteOnUIThread(() => {
        OnFontAdded?.Invoke();
      });
    }
    #endregion

    #region private methods
    private void ExecuteOnUIThread(Action action) {
      Application.Current.Dispatcher.BeginInvoke(action);
    }
    #endregion
  }
}
