using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using TiTsEd.Common;
using TiTsEd.ViewModel;

namespace TiTsEd.Model
{
    public enum DirectoryKind
    {
        Regular,
        External,
        Backup,
    }

    public enum FileEnumerationResult
    {
        Success,
        NoPermission,
        Unreadable,
        Unknown,
    }

    public class FlashDirectory
    {
        public string Name;
        public string Path;
        public bool HasSeparatorBefore;
        public readonly DirectoryKind Kind;
        public readonly List<AmfFile> Files = new List<AmfFile>();

        public FlashDirectory(string name, string path, bool hasSeparatorBefore, DirectoryKind kind)
        {
            Name = name;
            Path = path;
            Kind = kind;
            HasSeparatorBefore = hasSeparatorBefore;
        }
    }

    public static class FileManager
    {
        static readonly List<string> _externalPaths = new List<string>();
        static readonly List<FlashDirectory> _directories = new List<FlashDirectory>();

        const int MaxBackupFiles = 10;

        const int MaxSaveSlots = 14;
        public const int SaveSlotsLowerBound = 1;
        public const int SaveSlotsUpperBound = MaxSaveSlots; // must use largest value here

        public static int SaveSlotsUpperBoundByGame
        {
            get { return MaxSaveSlots; }
        }

        public static FileEnumerationResult Result { get; private set; }
        public static string ResultPath { get; private set; }

        public static string AppDataPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"TiTsEd");
            }
        }

        public static string BackupPath
        {
            get
            {
                return Path.Combine(AppDataPath, @"backups");
            }
        }

        public static void BuildPaths()
        {
            Result = FileEnumerationResult.Success;

            string chromeAppPath = @"Google\Chrome\User Data\";
            string chromeProfilePattern = @"\\(?:Default|Profile \d+)$";
            string operaAppPath = @"Opera Software\";
            string operaProfilePattern = @"\\Opera(?: \w+)?$";

            bool insertSeparatorBeforeInMenu = false;

            // AIR handles: the AIR version
            // Standard handles: Firefox, Netscape Suite, Internet Explorer (desktop, not metro/tablet), Opera (v≤23; NPAPI).
            // Chrome handles: Google Chrome (maybe Chromium).
            // Opera handles: Opera (v≥24; PPAPI).
            // Edge/Metro handles: Edge and Internet Explorer (metro/tablet, not desktop).
            BuildAIRPath("Local (AIR {0})", ref insertSeparatorBeforeInMenu);

            insertSeparatorBeforeInMenu = true;

            BuildNpapiPath("Local (Standard{0})", @"localhost", ref insertSeparatorBeforeInMenu);

            BuildPpapiPath("Local (Chrome{0})", Environment.SpecialFolder.LocalApplicationData, chromeAppPath, chromeProfilePattern, @"localhost", ref insertSeparatorBeforeInMenu);

            BuildPpapiPath("Local (Opera{0})", Environment.SpecialFolder.ApplicationData, operaAppPath, operaProfilePattern, @"localhost", ref insertSeparatorBeforeInMenu);

            BuildNpapiPath("Local (Edge/Metro{0})", @"#AppContainer\localhost", ref insertSeparatorBeforeInMenu);

            insertSeparatorBeforeInMenu = true;

            BuildNpapiPath("LocalWithNet (Standard{0})", @"#localWithNet", ref insertSeparatorBeforeInMenu);

            BuildPpapiPath("LocalWithNet (Chrome{0})", Environment.SpecialFolder.LocalApplicationData, chromeAppPath, chromeProfilePattern, @"#localWithNet", ref insertSeparatorBeforeInMenu);

            BuildPpapiPath("LocalWithNet (Opera{0})", Environment.SpecialFolder.ApplicationData, operaAppPath, operaProfilePattern, @"#localWithNet", ref insertSeparatorBeforeInMenu);

            BuildNpapiPath("LocalWithNet (Edge/Metro{0})", @"#AppContainer\#localWithNet", ref insertSeparatorBeforeInMenu);

            insertSeparatorBeforeInMenu = true;

            BuildNpapiPath("Online (Standard{0})", @"www.fenoxo.com", ref insertSeparatorBeforeInMenu);

            BuildPpapiPath("Online (Chrome{0})", Environment.SpecialFolder.LocalApplicationData, chromeAppPath, chromeProfilePattern, @"www.fenoxo.com", ref insertSeparatorBeforeInMenu);

            BuildPpapiPath("Online (Opera{0})", Environment.SpecialFolder.ApplicationData, operaAppPath, operaProfilePattern, @"www.fenoxo.com", ref insertSeparatorBeforeInMenu);

            BuildNpapiPath("Online (Edge/Metro{0})", @"#AppContainer\www.fenoxo.com", ref insertSeparatorBeforeInMenu);

        }

        static void BuildAIRPath(string nameFormat, ref bool separatorBefore)
        {
            string path = "";
            try
            {
                // …\AppData\Roaming
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (null == path)
                {
                    return;
                }

                // ..\AppData\Roaming\com.taintedspace.tits\Local Store\#SharedObjects
                path = Path.Combine(path, @"com.taintedspace.tits\Local Store\#SharedObjects\");
                if (!Directory.Exists(path))
                {
                    return;
                }
                string parentPath = path;

                var dirs = new Dictionary<string, string>();
                dirs.Add("main", path);

                var airSaveSets = Directory.GetDirectories(path);
                foreach (var folder in airSaveSets)
                {
                    if (folder == parentPath)
                    {
                        continue;
                    }
                    path = folder;
                    var folderName = Path.GetFileName(folder);
                    dirs.Add(String.Format("Save Set {0}", folderName), folder);
                }

                // Create items now that we know how many of them there are.
                foreach (KeyValuePair<string, string> pair in dirs)
                {
                    var name = String.Format(nameFormat, pair.Key);
                    path = pair.Value;
                    var flash = new FlashDirectory(name, path, separatorBefore, DirectoryKind.Regular);
                    separatorBefore = false;
                    _directories.Add(flash);
                }
            }
            catch (SecurityException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.NoPermission;
            }
            catch (UnauthorizedAccessException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.NoPermission;
            }
            catch (IOException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.Unreadable;
            }
            catch (Exception)
            {
                ResultPath = path;
                Result = FileEnumerationResult.Unknown;
            }

        }

        static void BuildNpapiPath(string nameFormat, string suffix, ref bool separatorBefore)
        {
            string path = "";
            try
            {
                // …\AppData\Roaming
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (null == path)
                {
                    return;
                }

                // …\AppData\Roaming\Macromedia\Flash Player\#SharedObjects
                path = Path.Combine(path, @"Macromedia\Flash Player\#SharedObjects");
                if (!Directory.Exists(path))
                {
                    return;
                }

                // …\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\{flash_profile}
                var flashProfilePaths = Directory.GetDirectories(path);

                // …\AppData\Roaming\Macromedia\Flash Player\#SharedObjects\{flash_profile}\{suffix}
                var titsDirectories = new List<String>();
                foreach (var flashProfile in flashProfilePaths)
                {
                    path = Path.Combine(flashProfile, suffix);
                    if (Directory.Exists(path))
                    {
                        titsDirectories.Add(path);
                    }
                }

                // Create items now that we know how many of them there are.
                int saveNum = 1;
                foreach (var titsDir in titsDirectories)
                {
                    path = titsDir;
                    var name = String.Format(nameFormat, titsDirectories.Count > 1 ? String.Format(" #{0}", saveNum) : "");
                    var flash = new FlashDirectory(name, path, separatorBefore, DirectoryKind.Regular);
                    separatorBefore = false;
                    _directories.Add(flash);
                    saveNum++;
                }
            }
            catch (SecurityException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.NoPermission;
            }
            catch (UnauthorizedAccessException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.NoPermission;
            }
            catch (IOException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.Unreadable;
            }
            catch (Exception)
            {
                ResultPath = path;
                Result = FileEnumerationResult.Unknown;
            }
        }

        static void BuildPpapiPath(string nameFormat, Environment.SpecialFolder appDataPath, string appPath, string appProfilePattern, string suffix, ref bool separatorBefore)
        {
            // …\AppData\Local\Google\Chrome\User Data\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\{flash_profile}\{suffix}
            // …\AppData\Roaming\Opera Software\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\{flash_profile}\{suffix}

            Regex appProfileRegex = new Regex(appProfilePattern);
            string path = "";
            try
            {
                // …\AppData\Local
                // …\AppData\Roaming
                var basePath = Environment.GetFolderPath(appDataPath);
                if (null == basePath)
                {
                    return;
                }

                // …\AppData\Local\Google\Chrome\User Data
                // …\AppData\Roaming\Opera Software
                basePath = Path.Combine(basePath, appPath);
                if (!Directory.Exists(basePath))
                {
                    return;
                }

                // Get app profile directories.
                var userDataDirectories = Directory.GetDirectories(basePath);
                var appProfilePaths = new List<String>();
                foreach (var userDir in userDataDirectories)
                {
                    if (appProfileRegex.IsMatch(userDir))
                    {
                        path = Path.Combine(basePath, userDir);
                        if (Directory.Exists(path))
                        {
                            appProfilePaths.Add(path);
                        }
                    }
                }

                // Get shared object directories.
                var titsDirectories = new List<String>();
                foreach (var profilePath in appProfilePaths)
                {
                    // …\AppData\Local\Google\Chrome\User Data\{app_profile}
                    // …\AppData\Roaming\Opera Software\{app_profile}
                    path = profilePath;

                    // …\AppData\Local\Google\Chrome\User Data\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects
                    // …\AppData\Roaming\Opera Software\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects
                    path = Path.Combine(path, @"Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\");
                    if (!Directory.Exists(path))
                    {
                        continue;
                    }

                    // …\AppData\Local\Google\Chrome\User Data\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\{flash_profile}
                    // …\AppData\Roaming\Opera Software\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\{flash_profile}
                    var flashProfilePaths = Directory.GetDirectories(path);

                    // …\AppData\Local\Google\Chrome\User Data\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\{flash_profile}\{suffix}
                    // …\AppData\Roaming\Opera Software\{app_profile}\Pepper Data\Shockwave Flash\WritableRoot\#SharedObjects\{flash_profile}\{suffix}
                    foreach (var dir in flashProfilePaths)
                    {
                        path = Path.Combine(dir, suffix);
                        if (Directory.Exists(path))
                        {
                            titsDirectories.Add(path);
                        }
                    }
                }

                // Create items now that we know how many of them there are.
                int saveNum = 1;
                foreach (var titsDir in titsDirectories)
                {
                    var name = String.Format(nameFormat, titsDirectories.Count > 1 ? String.Format(" #{0}", saveNum) : "");
                    var flash = new FlashDirectory(name, titsDir, separatorBefore, DirectoryKind.Regular);
                    separatorBefore = false;
                    _directories.Add(flash);
                    saveNum++;
                }
            }
            catch (SecurityException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.NoPermission;
            }
            catch (UnauthorizedAccessException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.NoPermission;
            }
            catch (IOException)
            {
                ResultPath = path;
                Result = FileEnumerationResult.Unreadable;
            }
            catch (Exception)
            {
                ResultPath = path;
                Result = FileEnumerationResult.Unknown;
            }
        }

        public static IEnumerable<FlashDirectory> GetDirectories()
        {
            foreach (var dir in _directories)
            {
                yield return CreateDirectory(dir);
            }
            yield return CreateExternalDirectory();
        }

        public static FlashDirectory CreateBackupDirectory()
        {
            var dir = new FlashDirectory("Backup", BackupPath, true, DirectoryKind.Backup);

            var dirInfo = new DirectoryInfo(BackupPath);
            foreach (var filePath in dirInfo.GetFiles("*.bak").OrderByDescending(x => x.LastWriteTimeUtc).Select(x => x.FullName))
            {
                AddFileToDirectory(dir, filePath);
            }
            return dir;
        }

        static FlashDirectory CreateExternalDirectory()
        {
            var dir = new FlashDirectory("External", "", true, DirectoryKind.External);
            foreach (var filePath in _externalPaths)
            {
                AddFileToDirectory(dir, filePath);
            }
            return dir;
        }

        static FlashDirectory CreateDirectory(FlashDirectory dir)
        {
            dir = new FlashDirectory(dir.Name, dir.Path, dir.HasSeparatorBefore, DirectoryKind.Regular);
            if (String.IsNullOrEmpty(dir.Path))
            {
                return dir;
            }

            for (int i = SaveSlotsLowerBound; i <= SaveSlotsUpperBound; i++)
            {
                var filePath = Path.Combine(dir.Path, "TiTs_" + i + ".sol");
                AddFileToDirectory(dir, filePath);
            }
            return dir;
        }

        private static bool AddFileToDirectory(FlashDirectory dir, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var amfFile = new AmfFile(filePath);
            if (null != amfFile.Error)
            {
                ResultPath = filePath;
                switch (amfFile.Error.Type)
                {
                    case AmfFileError.Error.NoPermission:
                        Result = FileEnumerationResult.NoPermission;
                        return false;

                    case AmfFileError.Error.Unreadable:
                        Result = FileEnumerationResult.Unreadable;
                        return false;

                    case AmfFileError.Error.Unknown:
                        Result = FileEnumerationResult.Unknown;
                        return false;

                    default:
                        Result = FileEnumerationResult.Unknown;
                        return false;
                }
            }
            dir.Files.Add(amfFile);
            return true;
        }

        public static void TryRegisterExternalFile(string path)
        {
            path = Canonicalize(path);

            // Is it a regular file?
            foreach (var dir in _directories)
            {
                if (AreParentAndChild(dir.Path, path))
                {
                    return;
                }
            }

            // Is it a backup?
            if (AreDirectoriesSame(Path.GetDirectoryName(path), BackupPath))
            {
                return;
            }

            // Is this file already known?
            if (_externalPaths.Contains(path))
            {
                return;
            }

            _externalPaths.Add(path);
        }

        static bool AreParentAndChild(string dirPath, string filePath)
        {
            if (String.IsNullOrEmpty(dirPath))
            {
                return false;
            }
            if (String.IsNullOrEmpty(filePath))
            {
                return false;
            }
            return AreDirectoriesSame(dirPath, Path.GetDirectoryName(filePath));
        }

        static bool AreDirectoriesSame(string dirA, string dirB)
        {
            if (String.IsNullOrEmpty(dirA))
            {
                return String.IsNullOrEmpty(dirB);
            }
            else
            {
                if (String.IsNullOrEmpty(dirB))
                {
                    return false;
                }
            }
            var strA = Path.GetFullPath(Canonicalize(dirA));
            var strB = Path.GetFullPath(Canonicalize(dirB));
            return strA.Equals(strB);
        }

        static string Canonicalize(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return path;
            }
            return path.Replace("//", "/").Replace("/", "\\").Replace("\\\\", "\\");
        }

        public static void CreateBackup(string sourcePath)
        {
            var backupDir = new DirectoryInfo(BackupPath);

            try
            {
                if (!Directory.Exists(BackupPath))
                {
                    Directory.CreateDirectory(BackupPath);
                }
                var existingFiles = backupDir.GetFiles("*.bak").OrderByDescending(x => x.LastWriteTimeUtc).ToArray();
                CopyToBackupPath(sourcePath);

                if (TryDeleteIdenticalFile(sourcePath, existingFiles))
                {
                    return;
                }

                for (int i = MaxBackupFiles; i < existingFiles.Length; ++i)
                {
                    existingFiles[i].Delete();
                }
            }
            catch (SecurityException se)
            {
                Logger.Error(se);
            }
            catch (UnauthorizedAccessException uae)
            {
                Logger.Error(uae);
            }
            catch (IOException ioe)
            {
                Logger.Error(ioe);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

        }

        static void CopyToBackupPath(string sourcePath)
        {
            var targetName = DateTime.UtcNow.Ticks + ".bak";
            var targetPath = Path.Combine(BackupPath, targetName);
            File.Copy(sourcePath, targetPath, true);
        }

        static bool TryDeleteIdenticalFile(string sourcePath, FileInfo[] existingFiles)
        {
            var sourceData = File.ReadAllBytes(sourcePath);

            foreach (var file in existingFiles)
            {
                if (AreIdentical(file, sourceData))
                {
                    file.Delete();
                    return true;
                }
            }
            return false;
        }

        static bool AreIdentical(FileInfo x, byte[] yData)
        {
            if (null == x)
            {
                if (null != yData)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if ((0 == x.Length) && (null == yData))
                {
                    return true;
                }

                if (x.Length != yData.Length)
                {
                    return false;
                }

                var xData = File.ReadAllBytes(x.FullName);
                for (int i = 0; i < xData.Length; ++i)
                {
                    if (xData[i] != yData[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
