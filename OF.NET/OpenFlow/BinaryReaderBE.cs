using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public class BinaryReaderBE : BinaryReader
    {
        private bool _needConvert = true;  

        public BinaryReaderBE(Stream input) : base(input)
        {
            _needConvert = BitConverter.IsLittleEndian;
        }

        public BinaryReaderBE(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            _needConvert = BitConverter.IsLittleEndian;
        }

        public override int ReadInt32()
        {
            var a32 = base.ReadBytes(4);
            if (_needConvert)
            {
                Array.Reverse(a32);
            }
            return BitConverter.ToInt32(a32, 0);
        }

        public override uint ReadUInt32()
        {
            var a32 = base.ReadBytes(4);
            if (_needConvert)
            {
                Array.Reverse(a32);
            }
            return BitConverter.ToUInt32(a32, 0);
        }

        public override ushort ReadUInt16()
        {
            var a16 = base.ReadBytes(2);
            if (_needConvert)
            {
                Array.Reverse(a16);
            }
            return BitConverter.ToUInt16(a16, 0);
        }

        public override short ReadInt16()
        {
            var a16 = base.ReadBytes(2);
            if (_needConvert)
            {
                Array.Reverse(a16);
            }
            return BitConverter.ToInt16(a16, 0);
        }

        public override ulong ReadUInt64()
        {
            var a64 = base.ReadBytes(8);
            if (_needConvert)
            {
                Array.Reverse(a64);
            }
            return BitConverter.ToUInt64(a64, 0);
        }

        public override long ReadInt64()
        {
            var a64 = base.ReadBytes(8);
            if (_needConvert)
            {
                Array.Reverse(a64);
            }
            return BitConverter.ToInt64(a64, 0);
        }
    }
}
