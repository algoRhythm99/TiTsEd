using System.Windows;
using System.Windows.Controls;
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
