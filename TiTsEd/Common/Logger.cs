using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;


namespace TiTsEd.Common
{
    public static class Logger
    {
        public static void Log(String logString, bool truncate=false)
        {
            try
            {
                string dataVersion = "Unknown";
                if ((null != TiTsEd.ViewModel.VM.Instance) && !String.IsNullOrEmpty(TiTsEd.ViewModel.VM.Instance.FileVersion))
                {
                    dataVersion = TiTsEd.ViewModel.VM.Instance.FileVersion;
                }
                // if possible, make TiTsEd's and TiTs' versions an integral part of the exception message,
                // so we don't have to rely on users' claims of being up to date anymore
                var msg = String.Format("[{0}:{1}:{2}]: {3}",
                    (truncate) ? System.Reflection.Assembly.GetEntryAssembly().Location : Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    dataVersion,
                    logString);
                string[] messages = { msg };
                var logFile = GetLogFilePath();
                if (truncate)
                {
                    File.WriteAllLines(logFile, messages);
                }
                else
                {
                    File.AppendAllLines(logFile, messages);
                }
                Console.WriteLine(msg);
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }
        }

        public static void Error(Exception e)
        {
            Error(e.ToString());
        }

        public static void Error(string msg)
        {
            Log(msg);
        }

        public static string GetLogFilePath()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"TiTsEd");
            try
            {
                if (!Directory.Exists(appData))
                {
                    Directory.CreateDirectory(appData);
                }
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (NotSupportedException)
            {
            }

            var logFile = System.IO.Path.Combine(appData, "TiTsEd.log");
            return logFile;
        }
    }
}
