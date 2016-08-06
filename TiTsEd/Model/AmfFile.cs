using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TiTsEd.Model
{
    public enum SerializationFormat
    {
        Slot,
        Exported,
    }

    public sealed class AmfFile : AmfObject
    {
        static readonly HashSet<String> _backedUpFiles = new HashSet<string>();

        public AmfFile(string path)
            : base(AmfTypes.Array)
        {
            FilePath = path;
            Name = Path.GetFileNameWithoutExtension(FilePath);
            Date = File.GetLastWriteTime(path);
            FileStream stream = null;
            try
            {
                stream = File.OpenRead(path);
                using (var reader = new AmfReader(stream))
                {
                    stream = null;
                    string name;
                    SerializationFormat format;
                    reader.Run(this, out name, out format);
                    Format = format;
                    Name = name;
                }
            }
            // All exceptions need to be handled as the general case, since corrupt
            // saves can also cause exceptions (e.g. InvalidCastException), however,
            // we will flag permission and IO issues for consumers like FileManager
            catch (Exception e)
            {
                AmfFileError.Error type = AmfFileError.Error.Unknown;
                if (e is SecurityException || e is UnauthorizedAccessException) type = AmfFileError.Error.NoPermission;
                else if (e is IOException) type = AmfFileError.Error.Unreadable;
                Error = new AmfFileError(type, e.ToString());
            }
            finally
            {
                if (null != stream)
                {
                    stream.Dispose();
                }
            }
        }

        public SerializationFormat Format
        {
            get;
            private set;
        }

        public string FilePath
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public AmfFileError Error
        {
            get;
            private set;
        }

        public DateTime Date
        {
            get;
            private set;
        }

        public bool CanBeSaved(SerializationFormat format)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                using (var writer = new AmfWriter(stream))
                {
                    stream = null;
                    writer.Run(this, "Test", format);
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (null != stream)
                {
                    stream.Dispose();
                }
            }
            return true;
        }

        public void Save(string path, SerializationFormat format)
        {
            // Delete existing file
            var name = Path.GetFileNameWithoutExtension(path);

            // Write it to a temporary file, then move it.
            try
            {
                var tempPath = Path.GetTempFileName();
                Write(tempPath, format, name);
                EnsureDeleted(path);
                File.Move(tempPath, path);
            }
            // If this fails (no temporary folder access?), save directly
            catch (UnauthorizedAccessException)
            {
                Write(path, format, name);
            }
            catch (SecurityException)
            {
                Write(path, format, name);
            }
            catch (IOException)
            {
                Write(path, format, name);
            }
        }

        private void Write(string path, SerializationFormat format, string name)
        {
            EnsureDeleted(path);
            Stream stream = null;
            try
            {
                stream = File.Create(path);
                using (var writer = new AmfWriter(stream))
                {
                    stream = null;
                    writer.Run(this, name, format);
                }
            }
            finally
            {
                if (null != stream)
                {
                    stream.Dispose();
                }
            }
        }

        static void EnsureDeleted(string path)
        {
            if (File.Exists(path))
            {
                var attribs = File.GetAttributes(path) & ~FileAttributes.ReadOnly;
                File.SetAttributes(path, attribs);
                File.Delete(path);
            }
        }

#if DEBUG
        public void TestSerialization()
        {
            ComparisonStream stream = null;
            try
            {
                stream = new ComparisonStream(FilePath);
                using (var writer = new AmfWriter(stream))
                {
                    stream = null;
                    writer.Run(this, Name, Format);
                }
                stream.AssertSameLength();
            }
            finally
            {
                if (null != stream)
                {
                    stream.Dispose();
                }
            }
        }
#endif
    }

    public sealed class AmfFileError
    {
        public enum Error
        {
            Unknown,
            NoPermission,
            Unreadable,
        }

        public Error Type { get; private set; }
        public string Mesg { get; private set; }

        public AmfFileError(Error type, string mesg)
        {
            Type = type;
            Mesg = mesg;
        }
        public AmfFileError(string mesg)
            : this(Error.Unknown, mesg)
        {
        }

        public override string ToString()
        {
            return Mesg;
        }
    }
}
