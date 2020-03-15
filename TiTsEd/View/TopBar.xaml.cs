using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TiTsEd.Common;
using TiTsEd.Model;
using TiTsEd.ViewModel;
using Microsoft.Win32;

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
            openButton.Style = (Style)Resources["HighlightedSaveButton"];
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
            (new CheckForUpdateBox()).ShowDialog();
        }

        void OnSaveRequiredChanged(object sender, BoolEventArgs saveRequired)
        {
            if (saveRequired.Value) saveButton.Style = (Style)Resources["HighlightedSaveButton"];
            else saveButton.Style = _defaultSaveStyle;
        }

        void OnFileOpened(object sender, EventArgs e)
        {
            openButton.Style = _defaultSaveStyle;
        }


        void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            openMenu.IsOpen = false;

            if (openButton.IsChecked.Value == true)
            {
                SetItems(openMenu, FileManagerVM.GetOpenMenus());
                openMenu.IsOpen = true;
            }

        }


        void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            saveMenu.IsOpen = false;

            if (saveButton.IsChecked.Value == true)
            {
                SetItems(saveMenu, FileManagerVM.GetSaveMenus());
                saveMenu.IsOpen = true;
            }
        }


        void SetItems(ContextMenu menu, IEnumerable<IMenuVM> items)
        {
            SetItems(menu, items, true);
        }

        void SetItems(ItemsControl menu, IEnumerable<IMenuBaseVM> items, bool isRoot)
        {
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
        }

        void SubMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            var item = (IMenuItemVM)menu.Header;
            item.OnClick();
        }

        void Menu_Click(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            var item = (IMenuVM)menu.Header;
            item.OnClick();
        }

    }
}
