using System;
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
using TiTsEd.ViewModel;

namespace TiTsEd.View
{
    /// <summary>
    /// Interaction logic for CodexPage.xaml
    /// </summary>
    public partial class CodexPage : UserControl
    {
        public CodexPage()
        {
            InitializeComponent();
        }

        private void AllUnknown_Click(object sender, RoutedEventArgs e)
        {
            VM.Instance.Game.AllCodexUnknown();
        }

        private void AllUnlocked_Click(object sender, RoutedEventArgs e)
        {
            VM.Instance.Game.AllCodexUnlocked();
        }

        private void AllViewed_Click(object sender, RoutedEventArgs e)
        {
            VM.Instance.Game.AllCodexViewed();
        }
    }
}
