using System.Collections;
using System.Windows.Controls;

namespace TiTsEd.View
{
    /// <summary>
    /// Interaction logic for FlagBox.xaml
    /// </summary>
    public partial class FlagBox : ItemsControl {
        public FlagBox() {
            InitializeComponent();   
        }

        public new IEnumerable ItemsSource {
            get { return base.ItemsSource; }
            set { base.ItemsSource = value; }
        }

    }
}
