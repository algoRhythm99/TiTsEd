using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using TiTsEd.Common;
using TiTsEd.ViewModel;

namespace TiTsEd.View
{
    /// <summary>
    /// Interaction logic for TopBar.xaml
    /// </summary>
    public partial class TopBar : UserControl
    {
        Style _defaultSaveStyle;

        public TopBar()
        {
            InitializeComponent();

            _defaultSaveStyle = openButton.Style;
#if !DEBUG
            openButton.Style = (Style) Resources["HighlightedSaveButton"];
#endif

            openMenu.PlacementTarget = openButton;
            saveMenu.PlacementTarget = saveButton;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                VM.Instance.SaveRequiredChanged += OnSaveRequiredChanged;
                VM.Instance.FileOpened += OnFileOpened;
            }
        }

        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            Logger.Trace("CheckForUpdates_Click: Begin");

            var updateDialog = new CheckForUpdateBox();

            updateDialog.ShowDialog();

            Logger.Trace("CheckForUpdates_Click: End");

        }

        void OnSaveRequiredChanged(object sender, BoolEventArgs saveRequired)
        {
            if (saveRequired.Value)
            {
                saveButton.Style = (Style) Resources["HighlightedSaveButton"];
            }
            else
            {
                saveButton.Style = _defaultSaveStyle;
            }
        }

        void OnFileOpened(object sender, EventArgs e)
        {
            openButton.Style = _defaultSaveStyle;
        }


        void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Trace("OpenButton_Click");

            openMenu.IsOpen = false;

            if (openButton.IsChecked.Value == true)
            {
                Logger.Log("OpenButton_Click: Before GetOpenMenus");
                SetItems(openMenu, FileManagerVM.GetOpenMenus());
                openMenu.IsOpen = true;
            }

            Logger.Log("OpenButton_Click: End");
        }


        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Trace("SaveButton_Click: Begin");

            saveMenu.IsOpen = false;

            if (saveButton.IsChecked.Value == true)
            {
                SetItems(saveMenu, FileManagerVM.GetSaveMenus());
                saveMenu.IsOpen = true;
            }

            Logger.Trace("SaveButton_Click: End");
        }


        void SetItems(ContextMenu menu, IEnumerable<IMenuVM> items)
        {
            SetItems(menu, items, true);
        }

        void SetItems(ItemsControl menu, IEnumerable<IMenuBaseVM> items, bool isRoot)
        {
            Logger.Trace("SetItems");

            List<string> itemList = new List<string>();
            if (null != items)
            {
                foreach (IMenuBaseVM m in items)
                {
                    foreach (IMenuItemVM mi in m.Children)
                    {
                        itemList.Add(mi.Label);
                    }
                }
            }

            Logger.Trace(String.Format("SetItems([{0}], {1})", String.Join(",", itemList.ToArray()), isRoot));

            menu.Items.Clear();
            bool needSeparator = false;
            foreach (var item in items)
            {
                needSeparator |= item.HasSeparatorBefore;
                if (!item.IsVisible) continue;

                if (needSeparator) menu.Items.Add(new Separator());
                needSeparator = false;

                var subMenu = new MenuItem();
                subMenu.DataContext = item;
                subMenu.Header = item;
                if (isRoot)
                {
                    subMenu.Style = (Style)FindResource("RootMenuStyle");
                    SetItems(subMenu, item.Children, false);
                }
                menu.Items.Add(subMenu);
            }
            Logger.Trace("SetItems: End");
        }

        void SubMenu_Click(object sender, RoutedEventArgs e)
        {
            Logger.Trace("SubMenu_Click: Begin");

            var menu = (MenuItem)sender;
            var item = (IMenuItemVM)menu.Header;
            item.OnClick();

            Logger.Trace("SubMenu_Click: End");
        }

        void Menu_Click(object sender, RoutedEventArgs e)
        {
            Logger.Trace("Menu_Click: Begin");

            var menu = (MenuItem)sender;
            var item = (IMenuVM)menu.Header;
            item.OnClick();

            Logger.Trace("Menu_Click: End");
        }

    }
}
