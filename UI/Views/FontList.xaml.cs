﻿using Storage;
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
    #region data
    private Protocol.Payloads.UserData _userData;

    private ViewModels.FamilyCollectionVM _collectionVM;
    private ListCollectionView _collectionView;
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

        if (_collectionVM != null) {
          UnregisterCollectionEvents();
          _collectionVM.Dispose();
          _collectionVM = null;
          _collectionView = null;
          _searchCollection = null;
        }
      }
      else {
        Loader.Visibility = Visibility.Collapsed;
        InstalledCountLabel.Visibility = Visibility.Visible;
        NewCountLabel.Visibility = Visibility.Visible;
        AllCountLabel.Visibility = Visibility.Visible;

        UpdateCounters();

        _collectionVM = new ViewModels.FamilyCollectionVM(Storage.FamilyCollection);
        RegisterCollectionEvents();
        _collectionView = new ListCollectionView(_collectionVM.Families);
        _collectionView.Filter = CurrentFilter();

        _searchCollection = new ListCollectionView(_collectionVM.Families);
        _searchCollection.Filter = SearchFilter;

        RefreshFiltering();

        SearchFamilyTree.ItemsSource = _searchCollection;
        FamilyTree.ItemsSource = _collectionView;

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
    private void RegisterCollectionEvents() {
      if (_collectionVM != null) {
        _collectionVM.OnFontActivationChanged += RefreshFiltering;
        _collectionVM.OnFontAdded += RefreshFiltering;
        _collectionVM.OnFontRemoved += RefreshFiltering;
        _collectionVM.OnFontReplaced += RefreshFiltering;
      }
    }

    private void UnregisterCollectionEvents() {
      if (_collectionVM != null) {
        _collectionVM.OnFontActivationChanged -= RefreshFiltering;
        _collectionVM.OnFontAdded -= RefreshFiltering;
        _collectionVM.OnFontRemoved -= RefreshFiltering;
        _collectionVM.OnFontReplaced -= RefreshFiltering;
      }
    }
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
      if (_collectionView != null) {
        _collectionView.Filter = InstalledFilter;
        _collectionView.Refresh();
      }
    }

    private void NewButton_Checked(object sender, RoutedEventArgs e) {
      if (_collectionView != null) {
        _collectionView.Filter = NewFilter;
        _collectionView.Refresh();
      }
    }

    private void AllButton_Checked(object sender, RoutedEventArgs e) {
      if (_collectionView != null) {
        _collectionView.Filter = null;
        _collectionView.Refresh();
      }
    }
    #endregion

    #region filtering methods
    private bool InstalledFilter(object item) {
      ViewModels.FamilyVM fam = item as ViewModels.FamilyVM;
      return fam.HasActivatedFont;
    }

    private bool NewFilter(object item) {
      ViewModels.FamilyVM fam = item as ViewModels.FamilyVM;
      return fam.HasNewFont;
    }

    private Predicate<object> CurrentFilter() {
      if (InstalledButton.IsChecked ?? false) {
        return InstalledFilter;
      }
      else if (NewButton.IsChecked ?? false) {
        return NewFilter;
      }
      return null;
    }

    private void RefreshFiltering() {
      _collectionView?.Refresh();
      _searchCollection?.Refresh();
    }
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
    #endregion
  }
}
