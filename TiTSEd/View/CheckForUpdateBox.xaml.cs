using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TiTsEd.View
{
    public partial class CheckForUpdateBox : Window
    {
        enum UpdateCheckResult
        {
            No,
            Yes,
            Unknown,
        }

        //Task _updateCheckTask;

        public CheckForUpdateBox()
        {
            InitializeComponent();
            Owner = App.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            checkingGrid.Visibility = Visibility.Visible;
            statusGrid.Visibility = Visibility.Collapsed;
        }

        void CheckForUpdateBox_Loaded(object sender, RoutedEventArgs e)
        {
            //_updateCheckTask = Task.Factory.StartNew(new Action(() =>
            Task.Factory.StartNew(new Action(() =>
            {
                // check for an update
                var status = CheckForUpdate();

                // update the UI with the results
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    UpdateStatus(status);
                }), DispatcherPriority.Input);
            }));
        }

        void close_Click(object sender, RoutedEventArgs e)
        {
            //if (_updateCheckTask != null) _updateCheckTask.Dispose();
            Close();
        }

        void requestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

        void UpdateStatus(UpdateCheckResult status)
        {
            switch (status)
            {
                // nothing to do for UpdateCheckResult.Yes, the correct hyperlinked text is in the XAML as the default
                case UpdateCheckResult.No:
                    statusText.Text = "TiTsEd is up to date.";
                    break;
                case UpdateCheckResult.Unknown:
                    statusText.Text = "Check failed. An unexpected problem occurred.";
                    break;
            }
            checkingGrid.Visibility = Visibility.Collapsed;
            statusGrid.Visibility = Visibility.Visible;
        }

        UpdateCheckResult CheckForUpdate()
        {
            HttpWebRequest request;
            HttpWebResponse response;

            // Create the request
            // Old SF: https://sourceforge.net/p/TiTsEd/code/HEAD/tree/latest?format=raw
            // New GH: https://raw.githubusercontent.com/tmedwards/TiTsEd/master/latest
            string fileUrl = @"https://raw.githubusercontent.com/tmedwards/TiTsEd/master/latest";
            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create(fileUrl);
            }
            catch { return UpdateCheckResult.Unknown; }
            if (request == null) return UpdateCheckResult.Unknown;
            request.Method = "GET";

            // Get the response
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch { return UpdateCheckResult.Unknown; }
            if (response == null) return UpdateCheckResult.Unknown;

            // Check for an update
            UpdateCheckResult result = UpdateCheckResult.No;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
                string contents = readStream.ReadToEnd();
                responseStream.Close();
                readStream.Close();

                // Parse the contents and make the comparison
                var latest = ParseVersion(contents);
                var local = Assembly.GetExecutingAssembly().GetName().Version;
                if (latest[0] > local.Major) result = UpdateCheckResult.Yes;
                else if (latest[0] == local.Major)
                    if (latest[1] > local.Minor) result = UpdateCheckResult.Yes;
                    else if (latest[1] == local.Minor)
                        if (latest[2] > local.Build) result = UpdateCheckResult.Yes;
            }

            // Close the response
            response.Close();

            return result;
        }

        int[] ParseVersion(string verString)
        {
            string[] parts = verString.TrimEnd('\r', '\n', ' ').Split('.');
            int[] latestVersion = new int[3];

            try
            {
                for (int i = 0; i < 3; i++) latestVersion[i] = int.Parse(parts[i]);
            }
            catch { /* noop */ }

            return latestVersion;
        }
    }
}
