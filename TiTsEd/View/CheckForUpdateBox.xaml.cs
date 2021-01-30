using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using TiTsEd.Common;

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
            Logger.Trace("CheckForUpdateBox: Constructor");
            InitializeComponent();
            Owner = App.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            checkingGrid.Visibility = Visibility.Visible;
            statusGrid.Visibility = Visibility.Collapsed;
            versionLabel.Text = VersionInfo.Version;
            Logger.Trace("CheckForUpdateBox: End");
        }

        void CheckForUpdateBox_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Trace("CheckForUpdateBox_Loaded");

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
            Logger.Trace("close_Click");
            Close();
        }

        void requestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Logger.Trace("requestNavigate");
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            Close();
        }

        void UpdateStatus(UpdateCheckResult status)
        {
            Logger.Trace(String.Format("UpdateStatus: {0}", status));

            switch (status)
            {
                // nothing to do for UpdateCheckResult. Yes, the correct hyperlinked text is in the XAML as the default
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
            Logger.Trace("CheckForUpdate: Begin");

            HttpWebRequest request = null;
            HttpWebResponse response = null;

            // Create the request
            string fileUrl = UIHelpers.GetStringResource("LatestFileUrl");
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
            catch (Exception ex)
            {
                Logger.Debug(ex.Message);
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

            Logger.Trace(String.Format("CheckForUpdate: End: {0}", result));
            return result;
        }

        int[] ParseVersion(string verString)
        {
            Logger.Trace(String.Format("ParseVersion: {0}", verString));

            var sVar = String.IsNullOrWhiteSpace(verString) ? "0.0.0" : verString;
            var parts = sVar.TrimEnd('\r', '\n', ' ').Split('.');
            if ( parts.Length < 3 )
            {
                var parts2 = new string[3] { "0", "0", "0" };
                for (int i = 0; i < parts.Length; i++)
                {
                    parts2[i] = parts[i];
                }
                parts = parts2;
            }
            try
            {
                int value;
                return Array.ConvertAll(parts, s => int.TryParse(s, out value) ? value : 0);
            }
            catch { }
            return new int[3] { 0, 0, 0 };
        }
    }
}
