using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TiTsEd.Model;
using TiTsEd.ViewModel;

namespace TiTsEd.View {
    /// <summary>
    /// Interaction logic for ItemPage.xaml
    /// </summary>
    public partial class ItemsPage : UserControl {
        private int lastSelectedIndex = -1;

        public ItemsPage() {
            InitializeComponent();
        }

        void leftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            rightPanel.Visibility = (leftTree.SelectedItem == null ? Visibility.Collapsed : Visibility.Visible);

            //a hack to select the new slot for an item we selected after we refresh our list
            if (leftTree.SelectedItem.GetType() != typeof(ItemSlotVM)) {
                return;
            }

            var selected = (ItemSlotVM)leftTree.SelectedItem;

            //we don't care about slots with items already in them
            if (selected.TypeID != XmlItem.Empty.ID) {
                lastSelectedIndex = -1;
            }

            int index = -1;
            foreach(ItemSlotVM item in ((ItemContainerVM)leftTree.Items[0]).Slots) {
                //ignore items that are empty slots
                if (item.TypeID != XmlItem.Empty.ID) {
                    ++index;
                }
                if (item == selected) {
                    //our slot is empty, so update it one more
                    if (selected.TypeID == XmlItem.Empty.ID) {
                        ++index;
                    }
                    break;
                }
            }

            lastSelectedIndex = index;
        }

        void rightTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            //a hack to select the new slot for an item we selected after we refresh our list
            if (lastSelectedIndex >= 0) {
                //get our inventory item
                var tvi = leftTree.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                //get our index within that inventory item
                tvi = tvi.ItemContainerGenerator.ContainerFromIndex(lastSelectedIndex) as TreeViewItem;
                //tell it to select that inventory item
                MethodInfo selectMethod = typeof(TreeViewItem).GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Instance);
                selectMethod.Invoke(tvi, new object[] { true });
            }
        }
    }
}
