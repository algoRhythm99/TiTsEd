using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TiTsEd.Common;

namespace TiTsEd.View
{
    public enum ExceptionBoxResult
    {
        OK,
        Continue,
        Cancel,
        Quit,
    }

    public enum ExceptionBoxButtons
    {
        OK,
        Continue,
        Cancel,
        Quit,
    }

    public partial class ExceptionBox : Window
    {
        string _exceptionMsg;
        ExceptionBoxResult _result;

        public ExceptionBox()
        {
            InitializeComponent();
            SetFontFamily(folderText);
            SetFontFamily(exceptionText);
        }

        // Some users have corrupted fonts and WPF crashes in such a case
        // So we set the font family here, protected.
        void SetFontFamily(TextBlock block)
        {
            if (TrySetFontFamily(block, "Consolas")) return;
            if (TrySetFontFamily(block, "Courier New")) return;
            if (TrySetFontFamily(block, "Courier")) return;
            if (TrySetFontFamily(block, "Segoe UI")) return;
            if (TrySetFontFamily(block, "Segoe")) return;
        }

        bool TrySetFontFamily(TextBlock block, string familyName)
        {
            var family = new FontFamily(familyName);
            var typeFace = new Typeface(family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            GlyphTypeface glyphTypeFace;
            return typeFace.TryGetGlyphTypeface(out glyphTypeFace);
        }

        public bool IsWarning { get; set; }
        public bool ShowReportInstructions { get; set; }
        public string ExceptionMessage
        {
            get { return _exceptionMsg; }
            set
            {
                var os = Environment.OSVersion;
                string dataVersion = TiTsEd.ViewModel.VM.Instance != null ? TiTsEd.ViewModel.VM.Instance.FileVersion : "Unknown";
                if (String.IsNullOrWhiteSpace(dataVersion))
                {
                    dataVersion = "Unknown";
                }

                // if possible, make TiTsEd's and TiTs's versions an integral part of the exception message,
                // so we don't have to rely on users' claims of being up to date anymore
                _exceptionMsg = String.Format("[{0}:{1}:{2}:{3}]\n{4}",
                    System.Reflection.Assembly.GetEntryAssembly().Location,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    os.VersionString,
                    dataVersion,
                    value);
            }
        }
        public string Message { get; set; }
        public string Path { get; set; }

        public static new ExceptionBoxResult Show()
        {
            var box = new ExceptionBox();
            return box.ShowDialog();
        }

        public ExceptionBoxResult ShowDialog(params ExceptionBoxButtons[] buttons)
        {
            if (App.Current.MainWindow != this)
            {
                Owner = App.Current.MainWindow;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            text.Text = Message;

            if (IsWarning) image.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Warning.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            else image.Source = Imaging.CreateBitmapSourceFromHIcon(System.Drawing.SystemIcons.Error.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            // This isn't currently tied to ExceptionMessage because there are some instances where exceptions are shown
            // that probably are not very report worthy... decisions, decisions
            if (ShowReportInstructions) reportingGrid.Visibility = Visibility.Visible;
            else reportingGrid.Visibility = Visibility.Collapsed;

            if (String.IsNullOrEmpty(Path)) folderGrid.Visibility = Visibility.Collapsed;
            else if (!String.IsNullOrEmpty(Path)) folderText.Text = Path;

            if (String.IsNullOrEmpty(ExceptionMessage)) exceptionGrid.Visibility = Visibility.Collapsed;
            else exceptionText.Text = ExceptionMessage;

            if (!String.IsNullOrEmpty(ExceptionMessage) || !IsWarning || ShowReportInstructions) linkBar.Visibility = Visibility.Visible;
            else linkBar.Visibility = Visibility.Collapsed;

            Button lastButton = null;
            foreach (var choice in buttons)
            {
                lastButton = new Button();
                lastButton.Content = choice.ToString();
                switch (choice)
                {
                    case ExceptionBoxButtons.Quit:
                        lastButton.Click += quit_Click;
                        _result = ExceptionBoxResult.Quit;
                        break;

                    case ExceptionBoxButtons.Cancel:
                        lastButton.Click += cancel_Click;
                        _result = ExceptionBoxResult.Cancel;
                        break;

                    case ExceptionBoxButtons.Continue:
                        lastButton.Click += continue_Click;
                        _result = ExceptionBoxResult.Continue;
                        break;

                    case ExceptionBoxButtons.OK:
                        lastButton.Click += ok_Click;
                        _result = ExceptionBoxResult.OK;
                        break;

                    default:
                        lastButton.Click += quit_Click;
                        _result = ExceptionBoxResult.Quit;
                        break;
                }
                buttonStack.Children.Add(lastButton);
            }
            lastButton.IsDefault = true;

            base.ShowDialog();
            return _result;
        }

        void openFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = Path;
            if (String.IsNullOrEmpty(folderPath)) folderPath = System.IO.Path.GetDirectoryName(Path);
            while (!Directory.Exists(folderPath)) folderPath = System.IO.Path.GetDirectoryName(folderPath);
            Process.Start(folderPath);
        }

        void copyException_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, ExceptionMessage);
        }

        void ok_Click(object sender, RoutedEventArgs e)
        {
            _result = ExceptionBoxResult.OK;
            Close();
        }

        void continue_Click(object sender, RoutedEventArgs e)
        {
            _result = ExceptionBoxResult.Continue;
            Close();
        }

        void cancel_Click(object sender, RoutedEventArgs e)
        {
            _result = ExceptionBoxResult.Cancel;
            Close();
        }

        void quit_Click(object sender, RoutedEventArgs e)
        {
            _result = ExceptionBoxResult.Quit;
            Close();
        }

        void requestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void openLogfile_Click(object sender, RoutedEventArgs e)
        {
            var logFile = Logger.GetLogFilePath();
            if (File.Exists(logFile))
            {
                System.Diagnostics.Process.Start(logFile);
            }
        }
    }
}
