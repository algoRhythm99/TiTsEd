using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TiTsEd.View {
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
