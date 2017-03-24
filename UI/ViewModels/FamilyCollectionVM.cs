using Storage.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace UI.ViewModels {
  class FamilyCollectionVM {
    #region private data
    private FamilyCollection _model;
    #endregion

    #region properties
    public ObservableCollection<FamilyVM> Families { get; private set; }
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
    #endregion

    #region private methods
    private void ExecuteOnUIThread(Action action) {
      Application.Current.Dispatcher.BeginInvoke(action);
    }
    #endregion
  }
}
