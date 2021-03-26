using System;
using System.IO;
using System.Reflection;
using System.Security;


namespace TiTsEd.Common
{
    public static class Logger
    {
        public enum LogLevel
        {
            Fatal = 0,
            Error,
            Warn,
            Info,
            Debug,
            Trace
        };

        public static LogLevel Level { get; set; } = LogLevel.Debug;

        public static void Log(String logString, bool truncate=false, LogLevel level=LogLevel.Error)
        {
            if (Level >= level)
            {
                try
                {
                    string dataVersion = "Unknown";
                    var os = Environment.OSVersion;
                    if ((null != TiTsEd.ViewModel.VM.Instance) && !String.IsNullOrEmpty(TiTsEd.ViewModel.VM.Instance.FileVersion))
                    {
                        dataVersion = TiTsEd.ViewModel.VM.Instance.FileVersion;
                    }
                    // if possible, make TiTsEd's and TiTs' versions an integral part of the exception message,
                    // so we don't have to rely on users' claims of being up to date anymore
                    var titsedVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    var assemblyName = System.Reflection.Assembly.GetEntryAssembly().Location;
                    //                              : Assembly.GetExecutingAssembly().GetName().Name;
                    var msg = String.Format("[{0}:{1}:{2}:{3}]: {4}",
                            assemblyName,
                            titsedVersion,
                            os.VersionString,
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
        }

        public static void Log(string msg, LogLevel level=LogLevel.Error)
        {
            Log(msg, false, level);
        }

        public static void Log(string msg)
        {
            Log(msg, false, LogLevel.Fatal);
        }

        public static void Log(Exception e)
        {
            Log(e.ToString(), LogLevel.Error);
        }

        public static void Error(Exception e)
        {
            Error(e.ToString());
        }

        public static void Error(string msg)
        {
            Log(msg, LogLevel.Error);
        }

        public static void Debug(string msg)
        {
            Log(msg, LogLevel.Debug);
        }

        public static void Trace(string msg)
        {
            Log(msg, LogLevel.Trace);
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
