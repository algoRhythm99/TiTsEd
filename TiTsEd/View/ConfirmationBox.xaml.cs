using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TiTsEd.View
{
    public enum ConfirmationResult
    {
        Quit,
        Cancel,
    }

    public partial class ConfirmationBox : Window
    {
        ConfirmationResult _result;

        public ConfirmationBox()
        {
            InitializeComponent();
            _result = ConfirmationResult.Cancel;
            image.Source = Imaging.CreateBitmapSourceFromHIcon(SystemIcons.Question.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static new ConfirmationResult Show()
        {
            var box = new ConfirmationBox();
            if ((null != App.Current.MainWindow) && (App.Current.MainWindow.IsActive))
            {
                box.Owner = App.Current.MainWindow;
            }
            box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            box.ShowDialog();
            return box._result;
        }

        void close_Click(object sender, RoutedEventArgs e)
        {
            _result = ConfirmationResult.Quit;
            Close();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            _result = ConfirmationResult.Cancel;
            Close();
        }
    }
}
