using System;
using System.IO;

namespace TiTsEd.Model
{
#if DEBUG
    public sealed class ComparisonStream : Stream
    {
        readonly byte[] _comparisonContent;
        int _position;
        int _length;

        public ComparisonStream(string referencePath)
        {
            _comparisonContent = File.ReadAllBytes(referencePath);
        }

        public void AssertSameLength()
        {
            Assert(_comparisonContent.Length == _length);
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _position; }
            set 
            {
                Assert(value <= _length);
                _position = (int)value;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                WriteByte(buffer[offset + i]);
            }
        }

        public override void WriteByte(byte value)
        {
            Assert(_position < _comparisonContent.Length);

            var comparisonValue = _comparisonContent[_position];
            if (_position >= 8 || _length >= 8)
            {
                // The condition above is set because the size is only correctly specified after the rest has been written
                Assert(comparisonValue == value);
            }

            ++_position;
            _length = Math.Max(_length, _position);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = (int)offset;
                    break;

                case SeekOrigin.Current:
                    Position += (int)offset;
                    break;

                case SeekOrigin.End:
                    Position = _length - (int)offset;
                    break;
            }
            return _position;
        }

        public override void SetLength(long value)
        {
            _length = (int)value;
            Assert(_length <= _comparisonContent.Length);
            Assert(_position <= _length);
        }

        public void Assert(bool condition)
        {
            if (!condition) throw new InvalidOperationException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanTimeout
        {
            get { return false; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override int ReadByte()
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {

        }
    }
#endif
}
