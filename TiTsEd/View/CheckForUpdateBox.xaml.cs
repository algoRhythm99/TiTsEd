using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
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
            Close();
        }

        void requestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            Close();
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
                default:
                    break;
            }
            checkingGrid.Visibility = Visibility.Collapsed;
            statusGrid.Visibility = Visibility.Visible;
        }

        UpdateCheckResult CheckForUpdate()
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;

            // Create the request
            string fileUrl = @"https://raw.githubusercontent.com/Chase-san/TiTsEd/master/latest.txt";
            try
            {
                request = (HttpWebRequest) HttpWebRequest.Create(fileUrl);
            }
            catch { return UpdateCheckResult.Unknown; }
            if (request == null)
            {
                return UpdateCheckResult.Unknown;
            }
            request.Method = "GET";

            UpdateCheckResult result = UpdateCheckResult.No;
            // Get the response
            try
            {
                response = (HttpWebResponse) request.GetResponse();

                if (response == null)
                {
                    return UpdateCheckResult.Unknown;
                }

                // Check for an update
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = null;
                    string contents = null;
                    try
                    {
                        responseStream = response.GetResponseStream();
                        using (StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            responseStream = null;
                            contents = readStream.ReadToEnd();
                            if (null != contents)
                            {
                                // Parse the contents and make the comparison
                                var latest = ParseVersion(contents);
                                var local = Assembly.GetExecutingAssembly().GetName().Version;
                                if (latest[0] > local.Major)
                                {
                                    result = UpdateCheckResult.Yes;
                                }
                                else if (latest[0] == local.Major)
                                {
                                    if (latest[1] > local.Minor)
                                    {
                                        result = UpdateCheckResult.Yes;
                                    }
                                    else if (latest[1] == local.Minor)
                                    {
                                        if (latest[2] > local.Build)
                                        {
                                            result = UpdateCheckResult.Yes;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (null != responseStream)
                        {
                            responseStream.Dispose();
                        }
                    }
                }
            }
            catch
            {
                return UpdateCheckResult.Unknown;
            }
            finally
            {
                // Close the response
                if (null != response)
                {
                    response.Close();
                }
            }

            return result;
        }

        int[] ParseVersion(string verString)
        {
            string[] parts = verString.TrimEnd('\r', '\n', ' ').Split('.');
            int[] latestVersion = new int[3];

            try
            {
                for (int i = 0; i < 3; i++)
                {
                    latestVersion[i] = int.Parse(parts[i]);
                }
            }
            catch { /* noop */ }

            return latestVersion;
        }
    }
}
