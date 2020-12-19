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
using TiTsEd.Common;
using TiTsEd.Model;
using TiTsEd.View;
using TiTsEd.ViewModel;

namespace TiTsEd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool _canSerializeSizeAndState;

        public MainWindow()
        {
            InitializeComponent();
            RestoreSizeAndState();

            ((FrameworkElement)Content).QueryContinueDrag += OnQueryContinueDrag;
        }

        void OnQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if ((e.KeyStates & DragDropKeyStates.LeftMouseButton) != DragDropKeyStates.LeftMouseButton)
            {
                e.Action = DragAction.Cancel;
            }
        }

        private void CopyToPC_Clicked(object sender, RoutedEventArgs e)
        {
            VM.Instance.Game.CopyCharacterToPC();
            VM.Instance.Game.CharacterSelection = "PC";
        }

#if !DEBUG
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if ((null != VM.Instance) && (!VM.Instance.SaveRequired)) return;

            var result = ConfirmationBox.Show();
            switch (result)
            {
                case ConfirmationResult.Cancel:
                    e.Cancel = true;
                    break;

                case ConfirmationResult.Quit:
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
#endif             



        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            SaveSizeAndState();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SaveSizeAndState();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            SaveSizeAndState();
        }

        void RestoreSizeAndState()
        {
            Top = Settings.Default.Location.Y;
            Left = Settings.Default.Location.X;
            Width = UIHelpers.NormalizeLength(Settings.Default.Size.Width);
            Height = UIHelpers.NormalizeLength(Settings.Default.Size.Height);

            if (Settings.Default.State == WindowState.Minimized) WindowState = WindowState.Normal;
            else WindowState = Settings.Default.State;

            EnsureVisible();
            _canSerializeSizeAndState = true;
        }

        void EnsureVisible()
        {
            var screenTop = SystemParameters.VirtualScreenTop + 50;
            var screenLeft = SystemParameters.VirtualScreenLeft + 50;
            var screenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 50;
            var screenBottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 50;

            Top = Math.Min(Top, screenBottom);
            Left = Math.Min(Left, screenRight);
            Width = UIHelpers.NormalizeLength(Math.Max(Left + Width, screenLeft) - Left);
            Height = UIHelpers.NormalizeLength(Math.Max(Top + Height, screenTop) - Top);
        }

        void SaveSizeAndState()
        {
            if (!_canSerializeSizeAndState) return;

            Settings.Default.Location = RestoreBounds.Location;
            Size size = new Size()
            {
              Width = UIHelpers.NormalizeLength(RestoreBounds.Size.Width)
            , Height = UIHelpers.NormalizeLength(RestoreBounds.Size.Height)
            };
            Settings.Default.Size = size;
            Settings.Default.State = WindowState;
            Settings.Default.Save();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        public NamedVector4Popup ValuesPopup
        {
            get { return valuesPopup; }
        }

        public void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            //VM.Instance.Game.SearchText = @"\u200B";
            //VM.Instance.Game.SearchText = String.Empty;

            var searchBoxControl = UIHelpers.FindChild<SearchBox>(this, "SearchBoxControl");
            if (null != searchBoxControl)
            {
                searchBoxControl.SearchText = String.Empty;
            }

        }
    }
}
