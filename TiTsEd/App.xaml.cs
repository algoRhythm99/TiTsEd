using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;
using TiTsEd.Common;
using TiTsEd.Model;
using TiTsEd.View;
using TiTsEd.ViewModel;

namespace TiTsEd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.ThrowException);

#if !DEBUG
            DispatcherUnhandledException += OnDispatcherUnhandledException;
#endif

            try
            {
                Logger.Log("OnStartup", true);

                foreach (string s in e.Args)
                {
                    switch (s)
                    {
                        case "-trace":
                        case "-Trace":
                            Logger.Level = Logger.LogLevel.Trace;
                            break;
                        default:
                            break;
                    }
                }

                if (!Directory.Exists(FileManager.BackupPath))
                {
                    Directory.CreateDirectory(FileManager.BackupPath);
                }
                if (Directory.Exists(FileManager.AppDataPath))
                {
                    var appDataDir = new DirectoryInfo(FileManager.AppDataPath);
                    var existingFiles = appDataDir.GetFiles("*.bak");
                    // try to move existing backups to new backup folder
                    if ((null != existingFiles) && (existingFiles.Length > 0))
                    {
                        foreach (var bakFile in existingFiles)
                        {
                            File.Move(bakFile.FullName, Path.Combine(FileManager.BackupPath, bakFile.Name));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            Initialize();

            Settings.Default.Upgrade();
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                Logger.Error(e.Exception);
                ExceptionBox box = new ExceptionBox();
                SetError(box, e.Exception);
                var result = box.ShowDialog(ExceptionBoxButtons.Quit, ExceptionBoxButtons.Continue);
                switch (result)
                {
                    case ExceptionBoxResult.Continue:
                        break;
                    default:
                        Shutdown();
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show(ex.ToString(), "Error in error box ?!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        void SetError(ExceptionBox box, Exception exception)
        {
            var msg = exception.ToString();
            box.ExceptionMessage = msg;

            // Special case for image codec problem
            if (msg.Contains("0x88982F04"))
            {
                box.Title = "Bad image codec";
                box.Message = "You use a non-standard image codec that does not properly handle some PNG files. It's not only TiTsEd, other programs may also be affected.\n\nCheck for FastPictureViewer's or Canon's codec packs and try to update or uninstall them.";
            }
            else
            {
                box.Title = "Unexpected error";
                box.Message = "An unexpected error occured and the application is going to exit.";
                box.ShowReportInstructions = true;
            }
        }

        void Initialize()
        {
            foreach (string xmlFile in XmlData.Files.All)
            {
                var xmlResult = XmlData.LoadXml(xmlFile);
                string message = "";
                switch (xmlResult)
                {
                    case XmlLoadingResult.InvalidFile:
                        message = String.Format("The {0} file is out of date. Did you replace the bundled XML?", xmlFile);
                        break;

                    case XmlLoadingResult.MissingFile:
                        message = String.Format("The {0} file could not be found. Did you try to run the program from the archive without extracting all the files first?", xmlFile);
                        break;

                    case XmlLoadingResult.NoPermission:
                        message = String.Format("The {0} file was already in use or this application does not have permission to read from the folder where it is located.", xmlFile);
                        break;

                    case XmlLoadingResult.Success:
                        break;
                    default:
                        message = String.Format("Unknown error!");
                        break;
                }
                if (!String.IsNullOrEmpty(message))
                {
                    Logger.Error(message);
                    ExceptionBox box = new ExceptionBox();
                    box.Title = "Fatal error";
                    box.Message = message;
                    box.Path = Environment.CurrentDirectory;
                    //box.ShowReportInstructions = true;
                    box.ShowDialog(ExceptionBoxButtons.Quit);
                    Shutdown();
                    return;
                }
            }

            VM.Create();
            FileManager.BuildPaths();
            Logger.Trace("App.xaml.cs: Before FileManager.GetDirectories().ToArray()");
            var directories = FileManager.GetDirectories().ToArray(); // Load all on startup to check for errors
            Logger.Trace("App.xaml.cs: After FileManager.GetDirectories().ToArray()");
            var result = ExceptionBoxResult.Continue;
            switch (FileManager.Result)
            {
                case FileEnumerationResult.NoPermission:
                case FileEnumerationResult.Unreadable:
                case FileEnumerationResult.Unknown:
                    string path = FileManager.ResultPath ?? "ResultPath: null";
                    string message = String.Format("TiTsEd did not get permission to read a folder or file.\nSome files will not be displayed in the Open/Save menus.\nResult: {0} with path {1}", FileManager.Result.ToString(), path);
                    Logger.Error(message);
                    ExceptionBox box = new ExceptionBox();
                    box.Title = "Could not read some folders.";
                    box.Message = message;
                    box.Path = path;
                    box.IsWarning = true;
                    result = box.ShowDialog(ExceptionBoxButtons.Quit, ExceptionBoxButtons.Continue);
                    break;
                default:
                    break;
            }
            if (result == ExceptionBoxResult.Quit)
            {
                Shutdown();
                return;
            }

#if DEBUG
            var file = AutoLoad(directories);
#endif
        }


#if DEBUG
        static AmfFile AutoLoad(FlashDirectory[] directories) {
            //find first file
            AmfFile file = null;
            foreach (var dir in directories) {
                foreach (var dfile in dir.Files) {
                    file = dfile;
                    break;
                }
            }

            if (null != file)
            {
                VM.Instance.Load(file.FilePath, SerializationFormat.Slot);
            }
            return file;
        }

        static void PrintStatuses(AmfFile file) {
            foreach (AmfPair pair in file.GetObj("statusAffects"))
            {
                int key = Int32.Parse(pair.Key as string);
                var name = pair.ValueAsObject.GetString("statusAffectName");
                Debug.WriteLine(key.ToString("000") + " - " + name);
            }
        }

        static void RunSerializationTest(FlashDirectory[] directories) {
            Stopwatch s = new Stopwatch();
            s.Start();
            foreach (var first in directories[0].Files) {
                var outPath = "c:\\" + Path.GetFileName(first.FilePath);
                first.TestSerialization();
                first.Save(outPath, first.Format);

                var input = File.ReadAllBytes(first.FilePath);
                var output = File.ReadAllBytes(outPath);
                if (input.Length != output.Length) throw new InvalidOperationException();
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] != output[i]) throw new InvalidOperationException();
                }
            }
            var elapsed = s.ElapsedMilliseconds;
            MessageBox.Show("Success!");
        }
#endif
    }
}
