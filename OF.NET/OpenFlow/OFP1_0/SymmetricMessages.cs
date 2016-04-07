using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.OpenFlow.OFP1_0
{
    /// <summary>
    /// Hello
    /// </summary>
    public class OfpHello : IOfpMessage
    {
        public const uint Size = 8;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        {Type = OfpType.OFPT_HELLO,Length = 8};

        public OfpHello()
        {}

        public OfpHello(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
            var length = Header.Length - Size;
            if (length > 0)
            {
                br.ReadBytes((int)length);
            }
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Header.ToByteArray());
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// Echo
    /// </summary>
    public class OfpEcho : IOfpMessage
    {
        public const uint Size = 8;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_ECHO_REQUEST, Length = 8 };

        public byte[] Data;

        public OfpEcho(bool isReply = false)
        {
            Header.Type = isReply ? OfpType.OFPT_ECHO_REPLY : OfpType.OFPT_ECHO_REQUEST;
        }

        public OfpEcho(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
            var length = Header.Length - Size;
            if (length > 0)
            {
                Data = br.ReadBytes((int)length);
            }
        }

        public uint UpdateLength()
        {
            uint len = Size;
            len += Data == null ? 0 : (uint)Data.Length;
            Header.Length = (ushort)len;
            return len;
        }

        public byte[] ToByteArray()
        {
            UpdateLength();
            Writer w = new Writer(Size);
            w.Write(Header.ToByteArray());
            w.Write(Data);
            return w.ToByteArray();
        }

    }

    /// <summary>
    /// Vendor
    /// </summary>
    public class OfpVendorHeader : IOfpMessage
    {
        public const uint Size = 12;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        {Type = OfpType.OFPT_VENDOR};

        public byte[] Vendor;

        public byte[] Data;

        public OfpVendorHeader()
        {}

        public OfpVendorHeader(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
            br.Parse(out Vendor, 4);
            var length = Header.Length - Size;
            if (length > 0)
            {
                Data = br.ReadBytes((int)length);
            }
        }

        public uint UpdateLength()
        {
            uint len = Size;
            len += Data == null ? 0 : (uint)Data.Length;
            Header.Length = (ushort)len;
            return len;
        }

        public byte[] ToByteArray()
        {
            UpdateLength();
            Writer w = new Writer();
            w.Write(Header.ToByteArray());
            w.Write(Vendor);
            w.Write(Data);
            return w.ToByteArray();
        }

    }
}
