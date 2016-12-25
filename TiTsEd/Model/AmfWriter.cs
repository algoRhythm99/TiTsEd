using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiTsEd.Model
{
    public sealed class AmfWriter : IDisposable
    {
        readonly BinaryWriter _writer;
        readonly Dictionary<String, int> _stringLookup = new Dictionary<String, int>();
        readonly Dictionary<Object, int> _objectLookup = new Dictionary<Object, int>();
        readonly Dictionary<AmfTrait, int> _traitLookup = new Dictionary<AmfTrait, int>();
        readonly MemoryStream _charStream = new MemoryStream(1 << 16);
        readonly BinaryWriter _charWriter;
        readonly BinaryReader _charReader;

        public AmfWriter(Stream stream)
        {
            _charWriter = new BinaryWriter(_charStream);
            _charReader = new BinaryReader(_charStream);
            _writer = new BinaryWriter(stream);
        }

        public void Run(AmfFile file, string newName, SerializationFormat format)
        {
            switch (format)
            {
                case SerializationFormat.Slot:
                    WriteStandardFile(file, newName);
                    break;

                case SerializationFormat.Exported:
                    WritePlainDataFile(file);
                    break;

                default:
                    throw new NotImplementedException();
            }


            // Flush
            _writer.Flush();
        }

        void WritePlainDataFile(AmfFile file)
        {
            var data = new AmfObject(AmfTypes.Object);
            foreach(var pair in file) data[pair.Key] = pair.Value;

            WriteValue(data);
        }

        void WriteStandardFile(AmfFile file, string newName)
        {
            // Endianness
            _writer.Write((byte)0x00);
            _writer.Write((byte)0xBF);

            // Placeholder for size
            _writer.Write((int)0);

            // Magic signature
            _writer.Write('T');
            _writer.Write('C');
            _writer.Write('S');
            _writer.Write('O');
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x04);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);

            // Name
            var countBytes = BitConverter.GetBytes((UInt16)newName.Length);
            WriteReversedBytes(countBytes);
            _writer.Write(newName.ToArray());

            // AMF version number
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x00);
            _writer.Write((byte)0x03);

            // Key-value pairs
            foreach (var pair in file)
            {
                WriteString((string)pair.Key);
                WriteValue(pair.Value);
                _writer.Write((byte)0);
            }

            // Replace size
            _writer.BaseStream.Seek(2, SeekOrigin.Begin);
            uint dataSize = (uint)_writer.BaseStream.Length - 6;
            WriteU32(dataSize);
        }

        void WriteValue(Object obj)
        {
            if (obj == null)
            {
                _writer.Write((byte)AmfTypes.Undefined);
            }
            else if (obj is AmfNull)
            {
                _writer.Write((byte)AmfTypes.Null);
            }
            else if (obj is Boolean)
            {
                if ((bool)obj) _writer.Write((byte)AmfTypes.True);
                else _writer.Write((byte)AmfTypes.False);
            }
            else if (obj is Int32)
            {
                _writer.Write((byte)AmfTypes.Integer);
                WriteI29((int)obj);
            }
            else if (obj is Double)
            {
                _writer.Write((byte)AmfTypes.Double);
                WriteDouble((double)obj);
            }
            else if (obj is String)
            {
                _writer.Write((byte)AmfTypes.String);
                WriteString((string)obj);
            }
            else if (obj is DateTime)
            {
                _writer.Write((byte)AmfTypes.Date);
                WriteDate((DateTime)obj);
            }
            else if (obj is Byte[])
            {
                _writer.Write((byte)AmfTypes.ByteArray);
                WriteByteArray((Byte[])obj);
            }
            else if (obj is AmfXmlType)
            {
                var x = (AmfXmlType)obj;
                if (x.IsDocument) _writer.Write((byte)AmfTypes.XmlDoc);
                else _writer.Write((byte)AmfTypes.Xml);
                WriteXML(x);
            }
            else if (obj is AmfObject)
            {
                var o = (AmfObject)obj;
                _writer.Write((byte)o.AmfType);
                switch (o.AmfType)
                {
                    case AmfTypes.Array:
                        WriteArray(o);
                        break;

                    case AmfTypes.Object:
                        WriteObject(o);
                        break;

                    case AmfTypes.Dictionary:
                        WriteDictionary(o);
                        break;

                    case AmfTypes.VectorInt:
                    case AmfTypes.VectorUInt:
                    case AmfTypes.VectorDouble:
                    case AmfTypes.VectorGeneric:
                        WriteVector(o);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        void WriteReversedBytes(byte[] srcBytes)
        {
            for (int i = srcBytes.Length - 1; i >= 0; --i) _writer.Write(srcBytes[i]);
        }

        void WriteDouble(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteReversedBytes(bytes);
        }

        void WriteU32(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteReversedBytes(bytes);
        }

        void WriteI32(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            WriteReversedBytes(bytes);
        }

        void WriteI29(int value)
        {
            const int upperExclusiveBound = 1 << 29;
            if (value < 0) WriteU29(value + upperExclusiveBound); // -x is stored as 2^29 - x
            else WriteU29(value);
        }

        void WriteU29(int value, bool lowBitFlag)
        {
            value <<= 1;
            if (lowBitFlag) value |= 1;
            WriteU29(value);
        }

        void WriteU29(int value)
        {
            // Unsigned integer encoded on 8 to 32 bits, with 7 to 29 significant bits.
            // The most signficant bits are stored on the left (at the beginning).
            // The fourth byte always have 8 significant bits. 
            // 7-7-7-8  or  7-7-7   or 7-7  or 7

            // Say that value == A << 21 | B << 14 | C << 7 | D
            // We first test whether A is zero, write it or continue. Then B, then C, then D.


            // Initial shift to get the 7 most significant bits. 
            // If we do have four bytes, then we will have 29 bits and need to shift by 22 (29 - 7), 15, 8, 0. 
            // If we do have up to three bytes, then we will have up to 21 bits and will need to shift by 14 (21 - 7), 7, 0.
            bool fourBytes = (value >> 21) != 0;    // Shift is not circular in C#
            int shift = fourBytes ? 22 : 14;        
            int numBytes = 0;

            while (shift >= 0)
            {
                int mask = (numBytes == 3 ? 0xFF : 0x7F);
                byte b = (byte)((value >> shift) & mask);

                if (shift == 8) shift = 0;  // Only happen when there are four bytes to write.
                else shift -= 7;

                // Skip if:
                // * No group of bits have been written yet.
                // * Those 7 most signficant bits so far are zero
                // * They are not the 7 least significant bits (we always need to write those to store the number 0).
                if (fourBytes && b == 0 && numBytes == 0 && shift >= 0) continue;    
                ++numBytes;

                // Write a continuation bit if those are not the least significant bits (shift would be <0 for those ones)
                if (shift >= 0) b |= 0x80;
                _writer.Write(b);
            }
        }

        void WriteString(string str)
        {
            int index;
            // Empty string, special case.
            if (str == "")
            {
                WriteU29(0, true);
            }
            // String stored by reference.
            else if (_stringLookup.TryGetValue(str, out index))
            {
                WriteU29(index, false);
            }
            // Plain string
            else
            {
                WritePlainString(str);
                _stringLookup.Add(str, _stringLookup.Count);    // Stores reference.
            }
        }

        void WritePlainString(string str)
        {
            // Paint in the butt: strings are stored as length-in-bytes followed by the characters.
            // This is different from dotnet where strings are stored as number-of-characters followed by the characters.
            // Hence wny we first write the string in _charSteam, in order to get its length in bytes (and after we only need to copy the buffer).
            _charStream.Position = 0;
            foreach (var c in str) _charWriter.Write(c);

            int numBytes = (int)_charStream.Position;
            byte[] bytes = new byte[numBytes];

            _charStream.Position = 0;
            _charStream.Read(bytes, 0, numBytes);

            WriteU29(numBytes, true);
            _writer.Write(bytes);
        }

        void WriteDate(DateTime date)
        {
            if (TryWriteRef(date)) return;
            WriteU29(0, true);

            var elapsed = date - new DateTime(1970, 1, 1);
            WriteDouble(elapsed.TotalMilliseconds);
        }

        void WriteByteArray(byte[] array)
        {
            if (TryWriteRef(array)) return;
            WriteU29(array.Length, true);
            _writer.Write(array);
        }

        void WriteArray(AmfObject array)
        {
            if (TryWriteRef(array)) return;
            WriteU29(array.DenseCount, true);

            // Associative part (key-value pairs)
            foreach(var pair in array.GetSparseAndAssociativePairs())
            {
                WriteString(pair.Key.ToString());
                WriteValue(pair.Value);
            }
            WriteString("");

            // Dense part (consecutive indices >=0 and <count)
            foreach (var value in array.GetDensePart())
            {
                WriteValue(value);
            }
        }

        void WriteObject(AmfObject obj)
        {
            if (TryWriteRef(obj)) return;
            WriteTrait(obj.Trait);

            // Trait's properties
            foreach (var name in obj.Trait.Properties)
            {
                var value = obj[name];
                WriteValue(value);
            }

            // Dynamic properties
            if (obj.Trait.IsDynamic)
            {
                foreach(var pair in obj)
                {
                    // Is prop from trait or dynamic?
                    if (obj.Trait.Properties.Contains(pair.Key)) continue;

                    WriteString(pair.Key.ToString());
                    WriteValue(pair.Value);
                }
                WriteString("");
            }

            if (obj.Trait.IsExternalizable)
            {
                WriteCustomData(obj);
            }
        }

        void WriteCustomData(AmfObject obj)
        {
            switch (obj.Trait.Name)
            {
                case "CockTypesEnum":
                    WriteCustomDataForEnum(obj);
                    break;

                default:
                    throw new NotImplementedException("Unsupported externalized trait: " + (obj.Trait.Name ?? "<noname>"));
            }
        }

        void WriteCustomDataForEnum(AmfObject obj)
        {
            WriteI32((int)obj.EnumValue);
        }

        void WriteTrait(AmfTrait trait)
        {
            // By reference or by instance?
            int index;
            if (_traitLookup.TryGetValue(trait, out index))
            {
                WriteU29((index << 2) | 1);
                return;
            }
            _traitLookup.Add(trait, _traitLookup.Count);

            // Index and flags
            index = 3;                                      // 0b0011. trait by instance, obj by instance
            if (trait.IsExternalizable) index |= 4;         // 0b0100
            if (trait.IsDynamic) index |= 8;                // 0b1000
            index |= (trait.Properties.Length << 4);        
            WriteU29(index);

            // Name and properties
            WriteString(trait.Name);
            foreach(var name in trait.Properties)
            {
                WriteString(name);
            }
        }

        void WriteDictionary(AmfObject obj)
        {
            if (TryWriteRef(obj)) return;
            WriteU29(obj.Count, true);

            _writer.Write(obj.HasWeakKeys);
            foreach (var pair in obj)
            {
                WriteValue(pair.Key);
                WriteValue(pair.Value);
            }
        }

        void WriteVector(AmfObject vector)
        {
            if (vector.IsSparse) throw new InvalidOperationException("Vectors must be dense.");

            if (TryWriteRef(vector)) return;
            WriteU29(vector.DenseCount, true);

            _writer.Write(vector.IsFixedVector);
            if (vector.AmfType == AmfTypes.VectorGeneric) WriteString(vector.GenericElementType);

            foreach (var value in vector.GetDensePart())
            {
                switch (vector.AmfType)
                {
                    case AmfTypes.VectorInt:
                        WriteI32((int)value);
                        break;

                    case AmfTypes.VectorUInt:
                        WriteU32((uint)value);
                        break;

                    case AmfTypes.VectorDouble:
                        WriteDouble((double)value);
                        break;

                    case AmfTypes.VectorGeneric:
                        WriteValue(value);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        void WriteXML(AmfXmlType xml)
        {
            if (TryWriteRef(xml)) return;
            WritePlainString(xml.Content);
        }

        // Most object types are stored by reference so that they are only serialized once. After that only their reference index is stored.
        bool TryWriteRef(Object obj)
        {
            int index;
            if (_objectLookup.TryGetValue(obj, out index))
            {
                WriteU29(index, false);
                return true;
            }
            else
            {
                _objectLookup.Add(obj, _objectLookup.Count);
                return false;
            }
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer.Dispose();
                _charReader.Dispose();
                _charStream.Dispose();
                _charWriter.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
