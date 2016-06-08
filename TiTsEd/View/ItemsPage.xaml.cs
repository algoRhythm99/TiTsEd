using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TiTsEd.View
{
    /// <summary>
    /// Interaction logic for ItemPage.xaml
    /// </summary>
    public partial class ItemsPage : UserControl
    {
        public ItemsPage()
        {
            InitializeComponent();
        }

        void leftTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            rightPanel.Visibility = (leftTree.SelectedItem == null ? Visibility.Collapsed : Visibility.Visible);
        }
    }
}
