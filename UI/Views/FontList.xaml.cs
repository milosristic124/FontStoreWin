using Storage;
using Storage.Data;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using UI.Utilities;

namespace UI.Views {
  /// <summary>
  /// Interaction logic for FontList.xaml
  /// </summary>
  public partial class FontList : Window, IView {
    #region private type
    private enum FilterMode {
      None,
      Installed,
      New
    }
    #endregion

    #region data
    private Protocol.Payloads.UserData _userData;
    private FilterMode _filterMode;

    private FamilyCollection _collection;
    private ViewModels.FamilyCollectionVM _installedVM;
    private ViewModels.FamilyCollectionVM _newVM;
    private ViewModels.FamilyCollectionVM _allVM;

    //private ViewModels.FamilyCollectionVM _collectionVM;
    //private ListCollectionView _collectionView;
    private ListCollectionView _searchCollection;
    #endregion

    #region properties
    public UIElement DragHandle {
      get {
        if (IsInitialized) {
          return HeaderGrid;
        }
        return null;
      }
    }

    public IStorage Storage { get; set; }
    #endregion

    #region events
    public event OnExitHandler OnExit;
    public event OnLogoutHandler OnLogout;
    public event OnAboutClickedHandler OnAboutClicked;
    #endregion

    #region ctor
    public FontList(Protocol.Payloads.UserData userData) {
      _userData = userData;

      InitializeComponent();

      NameLabel.Content = string.Format("{0} {1}", _userData.FirstName, _userData.LastName);
    }
    #endregion

    #region methods
    public void InvokeOnUIThread(Action action) {
      try {
        Dispatcher.Invoke(action);
      }
      catch (Exception) { }
    }

    public void UpdateCounters() {
      AllCountLabel.Content = string.Format("({0})", Storage.FamilyCollection.Families.Count);
      NewCountLabel.Content = string.Format("({0})", Storage.NewFamilies.Count);
      InstalledCountLabel.Content = string.Format("({0})", Storage.ActivatedFamilies.Count);
    }

    public void LoadingState(bool isLoading) {
      if (isLoading) {
        Loader.Visibility = Visibility.Visible;
        InstalledCountLabel.Visibility = Visibility.Collapsed;
        NewCountLabel.Visibility = Visibility.Collapsed;
        AllCountLabel.Visibility = Visibility.Collapsed;

        FamilyTree.Visibility = Visibility.Hidden;
        FamilyTree.ItemsSource = null;

        _searchCollection = null;

        _collection?.Clear();
        _collection = null;

        _installedVM?.Dispose();
        _installedVM = null;

        _newVM?.Dispose();
        _newVM = null;

        _allVM?.Dispose();
        _allVM = null;


        //if (_collectionVM != null) {
        //  UnregisterCollectionEvents();
        //  _collectionVM.Dispose();
        //  _collectionVM = null;
        //  _collectionView = null;
        //  _searchCollection = null;
        //}
      }
      else {
        Loader.Visibility = Visibility.Collapsed;
        InstalledCountLabel.Visibility = Visibility.Visible;
        NewCountLabel.Visibility = Visibility.Visible;
        AllCountLabel.Visibility = Visibility.Visible;

        UpdateCounters();

        _filterMode = CurrentFilterMode();

        _collection = Storage.FamilyCollection;
        _allVM = new ViewModels.FamilyCollectionVM(_collection);
        _installedVM = new ViewModels.FamilyCollectionVM(_collection, family => family.HasActivatedFont);
        _newVM = new ViewModels.FamilyCollectionVM(_collection, family => family.HasNewFont);

        _searchCollection = new ListCollectionView(_allVM.Families);
        _searchCollection.Filter = SearchFilter;
        RefreshSearchResults();
        SearchFamilyTree.ItemsSource = _searchCollection;

        FamilyTree.ItemsSource = GetContentVM(_filterMode).Families;

        //_collectionVM = new ViewModels.FamilyCollectionVM(Storage.FamilyCollection);
        //RegisterCollectionEvents();
        //_collectionView = new ListCollectionView(_collectionVM.Families);
        //_collectionView.Filter = GetFilter(_filterMode);

        //_searchCollection = new ListCollectionView(_collectionVM.Families);
        //_searchCollection.Filter = SearchFilter;

        //RefreshAllResults();

        //SearchFamilyTree.ItemsSource = _searchCollection;
        //FamilyTree.ItemsSource = _collectionView;

        if (SearchButton.IsChecked ?? false) {
          FamilyTree.Visibility = Visibility.Collapsed;
          SearchPanel.Visibility = Visibility.Visible;
          UpdateSearchResult();
        } else {
          FamilyTree.Visibility = Visibility.Visible;
          SearchPanel.Visibility = Visibility.Collapsed;
        }

      }
    }

    public void Terminated(string message) {
      MessageBox.Show(this, message, "Fontstore - Connection closed", MessageBoxButton.OK);
    }

    public void Disconnected() {
      Loader.Visibility = Visibility.Visible;
      if (MessageBox.Show(this, "The application has been disconnected.\nFontstore will try to reconnect automatically.",
                          "Fontstore - Connection lost",
                          MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) {
        OnLogout?.Invoke();
      }
    }
    #endregion

    #region private methods
    private void SetContentVM(ViewModels.FamilyCollectionVM collection) {
      InvokeOnUIThread(delegate {
        FamilyTree.ItemsSource = collection.Families;
      });
    }

    private ViewModels.FamilyCollectionVM GetContentVM(FilterMode mode) {
      switch(mode) {
        case FilterMode.Installed:
          return _installedVM;

        case FilterMode.New:
          return _newVM;

        case FilterMode.None:
        default:
          return _allVM;
      }
    }

    //private void RegisterCollectionEvents() {
    //  if (_collectionVM != null) {
    //    _collectionVM.OnFontActivationChanged += RefreshAllResults;
    //    _collectionVM.OnFontAdded += RefreshAllResults;
    //    _collectionVM.OnFontRemoved += RefreshAllResults;
    //    _collectionVM.OnFontReplaced += RefreshAllResults;
    //  }
    //}

    //private void UnregisterCollectionEvents() {
    //  if (_collectionVM != null) {
    //    _collectionVM.OnFontActivationChanged -= RefreshAllResults;
    //    _collectionVM.OnFontAdded -= RefreshAllResults;
    //    _collectionVM.OnFontRemoved -= RefreshAllResults;
    //    _collectionVM.OnFontReplaced -= RefreshAllResults;
    //  }
    //}
    #endregion

    #region event handling
    #endregion

    #region UI event handling
    private void MenuButton_Click(object sender, RoutedEventArgs e) {
      MenuButton.ContextMenu.IsEnabled = true;
      MenuButton.ContextMenu.PlacementTarget = MenuButton;
      MenuButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
      MenuButton.ContextMenu.IsOpen = true;
    }

    private void Account_Click(object sender, RoutedEventArgs e) {
      ViewsUtility.NavigateToUri(new Uri(_userData.Urls.Account));
      e.Handled = true;
    }

    private void Settings_Click(object sender, RoutedEventArgs e) {
      ViewsUtility.NavigateToUri(new Uri(_userData.Urls.Settings));
      e.Handled = true;
    }

    private void Visit_Click(object sender, RoutedEventArgs e) {
      ViewsUtility.NavigateToUri(new Uri("http://fontstore.com"));
      e.Handled = true;
    }

    private void Help_Click(object sender, RoutedEventArgs e) {
      ViewsUtility.NavigateToUri(new Uri("http://fontstore.com/faqs"));
      e.Handled = true;
    }

    private void About_Click(object sender, RoutedEventArgs e) {
      OnAboutClicked?.Invoke();
    }

    private void Logout_Click(object sender, RoutedEventArgs e) {
      OnLogout?.Invoke();
    }

    private void Quit_Click(object sender, RoutedEventArgs e) {
      OnExit?.Invoke();
    }
    #endregion

    #region filtering event handling
    private void InstalledButton_Checked(object sender, RoutedEventArgs e) {
      _filterMode = FilterMode.Installed;
      SetContentVM(_installedVM);
      //if (_collectionView != null) {
      //  _filterMode = FilterMode.Installed;
      //  _collectionView.Filter = InstalledFilter;
      //  _collectionView.Refresh();
      //}
    }

    private void NewButton_Checked(object sender, RoutedEventArgs e) {
      _filterMode = FilterMode.New;
      SetContentVM(_newVM);
      //if (_collectionView != null) {
      //  _filterMode = FilterMode.New;
      //  _collectionView.Filter = NewFilter;
      //  _collectionView.Refresh();
      //}
    }

    private void AllButton_Checked(object sender, RoutedEventArgs e) {
      _filterMode = FilterMode.None;
      SetContentVM(_allVM);
      //if (_collectionView != null) {
      //  _filterMode = FilterMode.None;
      //  _collectionView.Filter = null;
      //  _collectionView.Refresh();
      //}
    }
    #endregion

    #region filtering methods
    //private bool InstalledFilter(object item) {
    //  ViewModels.FamilyVM fam = item as ViewModels.FamilyVM;
    //  return fam.HasActivatedFont;
    //}

    //private bool NewFilter(object item) {
    //  ViewModels.FamilyVM fam = item as ViewModels.FamilyVM;
    //  return fam.HasNewFont;
    //}

    private FilterMode CurrentFilterMode() {
      if (InstalledButton.IsChecked ?? false) {
        return FilterMode.Installed;
      }
      else if (NewButton.IsChecked ?? false) {
        return FilterMode.New;
      }
      return FilterMode.None;
    }

    //private Predicate<object> GetFilter(FilterMode mode) {
    //  switch(mode) {
    //    case FilterMode.Installed:
    //      return InstalledFilter;

    //    case FilterMode.New:
    //      return NewFilter;

    //    case FilterMode.None:
    //    default:
    //      return null;
    //  }
    //}

    //private void RefreshFilterResults() {
    //  _collectionView?.Refresh();
    //}

    //private void RefreshAllResults() {
    //  RefreshFilterResults();
    //  RefreshSearchResults();
    //}
    #endregion

    #region search handling
    private void SearchButton_Checked(object sender, RoutedEventArgs e) {
      _searchCollection?.Refresh();
      UpdateSearchResult();
      FamilyTree.Visibility = Visibility.Collapsed;
      SearchPanel.Visibility = Visibility.Visible;
    }

    private void SearchButton_Unchecked(object sender, RoutedEventArgs e) {
      FamilyTree.Visibility = Visibility.Visible;
      SearchPanel.Visibility = Visibility.Collapsed;
      SearchInput.Text = null;
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
      _searchCollection?.Refresh();
      UpdateSearchResult();
    }

    private bool SearchFilter(object item) {
      ViewModels.FamilyVM fam = item as ViewModels.FamilyVM;

      string searchedTxt = SearchInput.Text?.Trim()?.ToLower();
      if (searchedTxt == null || searchedTxt == "") {
        return true;
      } else {
        return fam.Name.ToLower().Contains(searchedTxt);
      }
    }

    private void SearchInput_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key == Key.Enter) {
        e.Handled = true;
        _searchCollection?.Refresh();
        UpdateSearchResult();
      }
    }

    private void UpdateSearchResult() {
      if (_searchCollection?.IsEmpty ?? true) {
        SearchResultArea.Visibility = Visibility.Hidden;
      } else {
        SearchResultArea.Visibility = Visibility.Visible;
        SearchResultCount.Content = _searchCollection.Count;
      }
    }

    private void RefreshSearchResults() {
      _searchCollection?.Refresh();
    }
    #endregion
  }
}
