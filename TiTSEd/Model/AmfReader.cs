using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiTsEd.Model
{
    public sealed class AmfReader : IDisposable
    {
        readonly BinaryReader _reader;
        readonly List<String> _stringLookup = new List<String>();
        readonly List<Object> _objectLookup = new List<Object>();
        readonly List<AmfTrait> _traitLookup = new List<AmfTrait>();

        public AmfReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public void Run(AmfFile file, out string name, out SerializationFormat format)
        {
            // Case for "save to file". Fenoxo only serializes ones object and there is no header.
            if (_reader.PeekChar() == 0x0A)
            {
                format = SerializationFormat.Exported;
                ReadPlainDataFile(file, out name);
            }
            // Case for "save to slot". Real AMF3 file with a proper header.
            else
            {
                format = SerializationFormat.Slot;
                ReadStandardFile(file, out name);
            }
        }

        void ReadPlainDataFile(AmfFile file, out string name)
        {
            name = Path.GetFileNameWithoutExtension(file.FilePath);

            var dataContainer = (AmfObject)ReadValue();
            var data = dataContainer.GetObj("data");
            foreach (var pair in data) file[pair.Key] = pair.Value;
        }

        void ReadStandardFile(AmfFile file, out string name)
        {
            // Endianness
            if (_reader.ReadByte() != 0) throw new NotImplementedException("Unknown endianness");
            if (_reader.ReadByte() != 0xBF) throw new NotImplementedException("Unknown endianness");

            // Size
            int size = (int)ReadU32();
            if (size + 6 != _reader.BaseStream.Length) throw new InvalidOperationException("Wrong file size");

            // Magic signature
            if (ReadPlainString(4) != "TCSO") throw new InvalidOperationException("Wrong file tag");
            _reader.BaseStream.Seek(6, SeekOrigin.Current);

            // Read name
            size = ReadU16();
            name = ReadPlainString(size);

            // Version
            int version = (int)ReadU32();
            if (version < 3) throw new NotImplementedException("Wrong AMF version");

            // Read content
            while (true)
            {
                var key = ReadString();
                var value = ReadValue();
                file[key] = value;

                _reader.ReadByte(); // Trailer. No official documentation. Usually zero.
                if (_reader.BaseStream.Position == _reader.BaseStream.Length) break;
            }
        }

        Object ReadValue()
        {
            var type = (AmfTypes)_reader.ReadByte();
            switch (type)
            {
                case AmfTypes.Undefined:
                    return null;

                case AmfTypes.Null:
                    return AmfNull.Instance;

                case AmfTypes.True:
                    return true;

                case AmfTypes.False:
                    return false;

                case AmfTypes.Integer:
                    return ReadI29();

                case AmfTypes.Double:
                    return ReadDouble();

                case AmfTypes.String:
                    return ReadString();

                case AmfTypes.Date:
                    return ReadDate();

                case AmfTypes.ByteArray:
                    return ReadByteArray();

                case AmfTypes.Array:
                    return ReadArray();

                case AmfTypes.Object:
                    return ReadObject();

                case AmfTypes.Dictionary:
                    return ReadDictionary();

                case AmfTypes.VectorInt:
                case AmfTypes.VectorUInt:
                case AmfTypes.VectorDouble:
                case AmfTypes.VectorGeneric:
                    return ReadVector(type);

                case AmfTypes.XmlDoc:
                    return ReadXML(true);

                case AmfTypes.Xml:
                    return ReadXML(false);

                default:
                    throw new NotImplementedException();
            }
        }

        readonly byte[] _buffer = new byte[8];
        void FillBufferReversed(int count)
        {
            for (int i = 0; i < count; i++) _buffer[count - (i + 1)] = _reader.ReadByte();
        }

        double ReadDouble()
        {
            FillBufferReversed(8);
            return BitConverter.ToDouble(_buffer, 0);
        }

        string ReadString()
        {
            // Stored by reference?
            bool isValue;
            var lengthOrIndex = ReadU29(out isValue);
            if (!isValue) return _stringLookup[lengthOrIndex];

            // Empty string (never stored by ref) ?
            if (lengthOrIndex == 0) return "";

            // Read the string
            var str = ReadPlainString(lengthOrIndex);
            _stringLookup.Add(str);
            return str;
        }

        string ReadPlainString(int lengthInBytes)
        {
            // Paint in the butt: strings are stored as length-in-bytes followed by the characters.
            // This is different from dotnet where strings are stored as number-of-characters followed by the characters.
            var start = _reader.BaseStream.Position;
            var chars = new char[lengthInBytes];

            var numChars = 0;
            while(_reader.BaseStream.Position < start + lengthInBytes)
            {
                chars[numChars] = _reader.ReadChar();
                ++numChars;
            }

            return new string(chars, 0, numChars);
        }

        ushort ReadU16()
        {
            FillBufferReversed(2);  // Different endianness
            return BitConverter.ToUInt16(_buffer, 0);
        }

        uint ReadU32()
        {
            FillBufferReversed(4);  // Different endianness
            return BitConverter.ToUInt32(_buffer, 0);
        }

        int ReadI32()
        {
            FillBufferReversed(4);  // Different endianness
            return BitConverter.ToInt32(_buffer, 0);
        }

        int ReadI29()
        {
            int result = ReadU29();
            const int maxPositiveInclusive = (1 << 28) - 1;
            if (result <= maxPositiveInclusive) return result;  // Postive number

            // Negative number. -x is stored as 2^29 - x
            const int upperExclusiveBound = 1 << 29;
            return result - upperExclusiveBound;
        }

        int ReadU29(out bool lowBitFlag)
        {
            int result = ReadU29();
            lowBitFlag = (result & 1) == 1;
            return result >> 1;
        }

        int ReadU29()
        {
            // Unsigned integer encoded on 8 to 32 bits, with 7 to 29 significant bits.
            // The most signficant bits are stored on the left (at the beginning).
            // The fourth byte always have 8 significant bits. 
            // 7-7-7-8  or  7-7-7   or 7-7  or 7

            int numBytes = 0;
            int result = 0;
            while (true)
            {
                byte b = _reader.ReadByte();
                if (numBytes == 3) return (result << 8) | b;

                result = (result << 7) | (b & 0x7F);
                if ((b & 0x7F) == b) return result;

                ++numBytes;
            }
        }

        DateTime ReadDate()
        {
            // Stored by ref?
            bool isInstance;
            int refIndex = ReadU29(out isInstance);
            if (!isInstance) return (DateTime)_objectLookup[refIndex];

            // Stored by value
            var elapsed = ReadDouble();
            var result = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(elapsed);
            _objectLookup.Add(result);
            return result;
        }

        AmfObject ReadArray()
        {
            // Stored by ref?
            bool isInstance;
            var indexOrCount = ReadU29(out isInstance);
            if (!isInstance) return (AmfObject)_objectLookup[indexOrCount];

            // Stored by value
            var result = new AmfObject(AmfTypes.Array);
            _objectLookup.Add(result);

            // Associative part (key-value pairs)
            while (true)
            {
                var key = ReadString();
                if (key == "") break;

                var value = ReadValue();
                result[key] = value;
            }

            // Dense part (consecutive indices >=0 and <count)
            for (int i = 0; i < indexOrCount; i++)
            {
                var value = ReadValue();
                result.Push(value);
            }

            return result;
        }

        AmfObject ReadObject()
        {
            // Stored by ref?
            bool isInstance;
            int refIndex = ReadU29(out isInstance);
            if (!isInstance) return (AmfObject)_objectLookup[refIndex];

            // Stored by value
            var result = new AmfObject(AmfTypes.Object);
            _objectLookup.Add(result);

            result.Trait = ReadTrait(refIndex);

            foreach (var name in result.Trait.Properties)
            {
                var value = ReadValue();
                result[name] = value;
            }

            if (result.Trait.IsDynamic)
            {
                while(true)
                {
                    var name = ReadString();
                    if (name == "") break;

                    var value = ReadValue();
                    result[name] = value;
                }
            }

            if (result.Trait.IsExternalizable)
            {
                ReadCustomData(result);
            }

            return result;
        }

        void ReadCustomData(AmfObject obj)
        {
            switch (obj.Trait.Name)
            {
                case "CockTypesEnum":
                    ReadCustomDataForEnum(obj);
                    break;

                default:
                    throw new NotImplementedException("Unsupported externalized trait: " + (obj.Trait.Name ?? "<noname>"));
            }
        }

        void ReadCustomDataForEnum(AmfObject obj)
        {
            obj.Trait.IsEnum = true;
            obj.EnumValue = ReadI32();
        }

        AmfTrait ReadTrait(int refIndex)
        {
            // Stored by ref?
            bool isInstance = PopFlag(ref refIndex);
            if (!isInstance) return _traitLookup[refIndex];

            // Stored by value
            var result = new AmfTrait();
            _traitLookup.Add(result);

            result.IsExternalizable = PopFlag(ref refIndex);
            result.IsDynamic = PopFlag(ref refIndex);
            result.Name = ReadString();

            result.Properties = new string[refIndex];
            for (var i = 0; i < result.Properties.Length; i++)
            {
                result.Properties[i] = ReadString();
            }

            // Special hack for serializable traits
            return result;
        }

        byte[] ReadByteArray()
        {
            // Stored by ref?
            bool isInstance;
            int lengthOrIndex = ReadU29(out isInstance);
            if (!isInstance) return (byte[])_objectLookup[lengthOrIndex];

            // Stored by value
            var result = _reader.ReadBytes(lengthOrIndex);
            _objectLookup.Add(result);
            return result;
        }

        AmfObject ReadVector(AmfTypes type)
        {
            // Stored by ref?
            bool isInstance;
            int lengthOrIndex = ReadU29(out isInstance);
            if (!isInstance) return (AmfObject)_objectLookup[lengthOrIndex];

            // Stored by value
            var result = new AmfObject(type);
            _objectLookup.Add(result);

            result.IsFixedVector = _reader.ReadBoolean();
            if (type == AmfTypes.VectorGeneric) result.GenericElementType = ReadString();

            for (int j = 0; j < lengthOrIndex; ++j)
            {
                switch (type)
                {
                    case AmfTypes.VectorInt:
                        result.Push(ReadI32());
                        break;

                    case AmfTypes.VectorUInt:
                        result.Push(ReadU32());
                        break;

                    case AmfTypes.VectorDouble:
                        result.Push(ReadDouble());
                        break;

                    case AmfTypes.VectorGeneric:
                        result.Push(ReadValue());
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            return result;
        }

        AmfObject ReadDictionary()
        {
            // Stored by ref?
            bool isInstance;
            int lengthOrIndex = ReadU29(out isInstance);
            if (!isInstance) return (AmfObject)_objectLookup[lengthOrIndex];

            // Stored by value
            var result = new AmfObject(AmfTypes.Dictionary);
            _objectLookup.Add(result);

            result.HasWeakKeys = _reader.ReadBoolean();
            for (int j = 0; j < lengthOrIndex; ++j)
            {
                var key = ReadValue();
                var value = ReadValue();
                result[key] = value;
            }
            return result;
        }

        AmfXmlType ReadXML(bool isDocument)
        {
            // Stored by ref?
            bool isInstance;
            int lengthOrIndex = ReadU29(out isInstance);
            if (!isInstance) return (AmfXmlType)_objectLookup[lengthOrIndex];

            // Stored by value
            var result = new AmfXmlType { IsDocument = isDocument };
            result.Content = ReadPlainString(lengthOrIndex);
            _objectLookup.Add(result);
            return result;
        }

        static bool PopFlag(ref int value)
        {
            bool result = (value & 1) == 1;
            value >>= 1;
            return result;
        }

        void IDisposable.Dispose()
        {
            _reader.Dispose();
        }
    }
}
