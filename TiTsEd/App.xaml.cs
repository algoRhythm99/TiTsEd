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
            if (!Directory.Exists(FileManager.BackupPath)) Directory.CreateDirectory(FileManager.BackupPath);
            Settings.Default.Upgrade();
            base.OnStartup(e);

#if !DEBUG
            DispatcherUnhandledException += OnDispatcherUnhandledException;
#endif
            Initialize();
        }

        void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            DispatcherUnhandledException -= OnDispatcherUnhandledException;

            try
            {
                ExceptionBox box = new ExceptionBox();
                SetError(box, e.Exception);
                box.ShowDialog(ExceptionBoxButtons.Quit);
            }
            catch(Exception e2)
            {
                MessageBox.Show(e2.ToString(), "Error in error box ?!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Logger.Error(e.Exception);
            Shutdown();
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
            ExceptionBox box;

            foreach (string xmlFile in XmlData.Files.All)
            {
                var xmlResult = XmlData.LoadXml(xmlFile);
                switch (xmlResult)
                {
                    case XmlLoadingResult.Success:
                        break;

                    case XmlLoadingResult.InvalidFile:
                        box = new ExceptionBox();
                        box.Title = "Fatal error";
                        box.Message = "The " + xmlFile + " file is out of date. Did you replace the bundled XML?";
                        box.Path = Environment.CurrentDirectory;
                        box.ShowDialog(ExceptionBoxButtons.Quit);
                        Shutdown();
                        return;

                    case XmlLoadingResult.MissingFile:
                        box = new ExceptionBox();
                        box.Title = "Fatal error";
                        box.Message = "The " + xmlFile + " file could not be found. Did you try to run the program from the archive without extracting all the files first?";
                        box.Path = Environment.CurrentDirectory;
                        box.ShowDialog(ExceptionBoxButtons.Quit);
                        Shutdown();
                        return;

                    case XmlLoadingResult.NoPermission:
                        box = new ExceptionBox();
                        box.Title = "Fatal error";
                        box.Message = "The " + xmlFile + " file was already in use or this application does not have permission to read from the folder where it is located.";
                        box.Path = Environment.CurrentDirectory;
                        box.ShowDialog(ExceptionBoxButtons.Quit);
                        Shutdown();
                        return;

                    default:
                        throw new NotImplementedException();
                }
            }

            VM.Create();

            FileManager.BuildPaths();
            var directories = FileManager.GetDirectories().ToArray(); // Load all on startup to check for errors
            var result = ExceptionBoxResult.Continue;
            switch (FileManager.Result)
            {
                case FileEnumerationResult.NoPermission:
                    box = new ExceptionBox();
                    box.Title = "Could not scan some folders.";
                    box.Message = "TiTsEd did not get permission to read a folder or file.\nSome files will not be displayed in the Open/Save menus.";
                    box.Path = FileManager.ResultPath;
                    box.IsWarning = true;
                    result = box.ShowDialog(ExceptionBoxButtons.Quit, ExceptionBoxButtons.Continue);
                    break;

                case FileEnumerationResult.Unreadable:
                    box = new ExceptionBox();
                    box.Title = "Could not read some folders.";
                    box.Message = "TiTsEd could not read a folder or file.\nSome files will not be displayed in the Open/Save menus.";
                    box.Path = FileManager.ResultPath;
                    box.IsWarning = true;
                    result = box.ShowDialog(ExceptionBoxButtons.Quit, ExceptionBoxButtons.Continue);
                    break;
            }
            if (result == ExceptionBoxResult.Quit)
            {
                Shutdown();
                return;
            }

#if DEBUG
            var file = AutoLoad(directories);
            //new AmfFile("e:\\plainObject.sol").TestSerialization();
            //new AmfFile("e:\\unicode.sol").TestSerialization();
            //DebugStatuses(file);
            //RunSerializationTest(set);
            //ParsePerks();
            //ImportStatuses();
            //ImportFlags();
#endif
        }


#if DEBUG
        static AmfFile AutoLoad(FlashDirectory[] directories)
        {
            var file = directories[0].Files[0];

            VM.Instance.Load(file.FilePath, SerializationFormat.Slot, createBackup: true);
            return file;
        }

        static void PrintStatuses(AmfFile file)
        {
            foreach (AmfPair pair in file.GetObj("statusAffects"))
            {
                int key = Int32.Parse(pair.Key as string);
                var name = pair.ValueAsObject.GetString("statusAffectName");
                Debug.WriteLine(key.ToString("000") + " - " + name);
            }
        }

        static void RunSerializationTest(FlashDirectory[] directories)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            foreach (var first in directories[0].Files)
            {
                var outPath = "e:\\" + Path.GetFileName(first.FilePath);
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
