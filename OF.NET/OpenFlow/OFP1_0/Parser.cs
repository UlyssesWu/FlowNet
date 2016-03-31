using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.OpenFlow.OFP1_0
{
    internal class Writer : IDisposable
    {
        private BinaryWriter bw;
        private MemoryStream ms;
        public Writer()
        {
            ms = new MemoryStream();
            bw = new BinaryWriter(ms);
        }

        public Writer(uint size)
        {
            ms = new MemoryStream();
            ms.SetLength(size);
            bw = new BinaryWriter(ms);
        }

        public void SetLength(uint size)
        {
            ms.SetLength(size);
        }

        /// <summary>
        /// 按字节填充
        /// </summary>
        /// <param name="length">要填充的字节数</param>
        public void Pad(int length)
        {
            if (length <= 0)
            {
                return;
            }
            bw.Write(new byte[length]);
        }

        /// <summary>
        /// bool
        /// </summary>
        /// <param name="value"></param>
        public void Write(bool value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// uint
        /// </summary>
        /// <param name="value"></param>
        public void Write(uint value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// ulong
        /// </summary>
        /// <param name="value"></param>
        public void Write(ulong value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// byte
        /// </summary>
        /// <param name="value"></param>
        public void Write(byte value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// byte[]
        /// </summary>
        /// <param name="value"></param>
        public void Write(byte[] value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// byte[] with length
        /// </summary>
        /// <param name="value"></param>
        public void Write(byte[] value, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (i + 1 > value.Length)
                {
                    bw.Write((byte)0x00);
                }
                else
                {
                    bw.Write(value[i]);
                }
            }
        }

        /// <summary>
        /// ushort
        /// </summary>
        /// <param name="value"></param>
        public void Write(ushort value)
        {
            bw.Write(value);
        }

        /// <summary>
        /// string with length
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        public void Write(string value, int length)
        {
            //bw.Write(value.ToCharArray(0,length));
            for (int i = 0; i < length; i++)
            {
                if (i + 1 > value.Length)
                {
                    bw.Write((char)0x00);
                }
                else
                {
                    bw.Write(value[i]);
                }
            }
        }

        /// <summary>
        /// OfpPortConfig as uint
        /// </summary>
        /// <param name="config"></param>
        public void Write(OfpPortConfig config)
        {
            bw.Write((uint)config);
        }

        /// <summary>
        /// OfpPortState as uint
        /// </summary>
        /// <param name="state"></param>
        public void Write(OfpPortState state)
        {
            bw.Write((uint)state);
        }

        /// <summary>
        /// OfpPortFeatures as uint
        /// </summary>
        /// <param name="features"></param>
        public void Write(OfpPortFeatures features)
        {
            bw.Write((uint)features);
        }

        /// <summary>
        /// OfpType as byte
        /// </summary>
        /// <param name="type"></param>
        public void Write(OfpType type)
        {
            bw.Write((byte)type);
        }

        /// <summary>
        /// OfpPort as ushort
        /// </summary>
        /// <param name="port"></param>
        public void Write(OfpPort port)
        {
            bw.Write((ushort)port);
        }

        /// <summary>
        /// OfpQueueProperties as ushort
        /// </summary>
        /// <param name="prop"></param>
        public void Write(OfpQueueProperties prop)
        {
            bw.Write((ushort)prop);
        }

        /// <summary>
        /// OfpFlowWildcards as uint
        /// </summary>
        /// <param name="wildcards"></param>
        public void Write(OfpFlowWildcards wildcards)
        {
            bw.Write((uint)wildcards);
        }

        /// <summary>
        /// OfpWildcards as uint
        /// </summary>
        /// <param name="wildcards"></param>
        public void Write(OfpWildcards wildcards)
        {
            bw.Write(wildcards.Value);
        }

        /// <summary>
        /// OfpActionType as ushort
        /// </summary>
        /// <param name="type"></param>
        public void Write(OfpActionType type)
        {
            bw.Write((ushort)type);
        }

        /// <summary>
        /// OfpCapabilities as uint
        /// </summary>
        /// <param name="capabilities"></param>
        public void Write(OfpCapabilities capabilities)
        {
            bw.Write((uint)capabilities);
        }

        /// <summary>
        /// OfpActionCapabilities as uint
        /// </summary>
        /// <param name="capabilities"></param>
        public void Write(OfpActionCapabilities capabilities)
        {
            bw.Write((uint)capabilities);
        }
        
        /// <summary>
        /// OfpConfigFlags as ushort
        /// </summary>
        /// <param name="configFlags"></param>
        public void Write(OfpConfigFlags configFlags)
        {
            bw.Write((ushort)configFlags);
        }

        public byte[] ToByteArray()
        {
            bw.Flush();
            return ms.ToArray();
        }

        public void Dispose()
        {
            bw.Close();
            bw.Dispose();
        }
    }

    internal static class ParserExtension
    {

        /// <summary>
        /// Read a string in unicode, no matter what encode <paramref name="reader">BinaryReader</paramref> is.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadUnicodeString(this BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            long startPos = reader.BaseStream.Position;
            var s = Encoding.Unicode.GetString(reader.ReadBytes(2));
            while (s != "\0")
            {
                sb.Append(s);
                s = Encoding.Unicode.GetString(reader.ReadBytes(2));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Read a string in ASCII and end with 0x00(NULL), no matter what encode <paramref name="reader">BinaryReader</paramref> is.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadSigleByteString(this BinaryReader reader)
        {
            StringBuilder sb = new StringBuilder();
            long startPos = reader.BaseStream.Position;
            var s = reader.ReadChar();
            while (s != 0)
            {
                sb.Append(s);
                s = reader.ReadChar();
            }
            return sb.ToString();
        }

        //Reader

        public static byte Parse(this BinaryReader br, out byte b)
        {
            b = br.ReadByte();
            return b;
        }

        public static OfpType Parse(this BinaryReader br, out OfpType b)
        {
            b = (OfpType)br.ReadByte();
            return b;
        }

        public static string Parse(this BinaryReader br, out string b, int maxLength)
        {
            b = new string(br.ReadChars(maxLength)).Replace("\0", "");
            return b;
        }

        public static byte[] Parse(this BinaryReader br, out byte[] b, int maxLength)
        {
            b = br.ReadBytes(maxLength);
            return b;
        }

        public static OfpPort Parse(this BinaryReader br, out OfpPort b)
        {
            b = (OfpPort)br.ReadUInt16();
            return b;
        }

        public static ushort Parse(this BinaryReader br, out ushort b)
        {
            b = br.ReadUInt16();
            return b;
        }

        public static uint Parse(this BinaryReader br, out uint b)
        {
            b = br.ReadUInt32();
            return b;
        }

        public static ulong Parse(this BinaryReader br, out ulong b)
        {
            b = br.ReadUInt64();
            return b;
        }

        public static OfpPortConfig Parse(this BinaryReader br, out OfpPortConfig b)
        {
            b = (OfpPortConfig)br.ReadUInt32();
            return b;
        }
        public static OfpPortState Parse(this BinaryReader br, out OfpPortState b)
        {
            b = (OfpPortState)br.ReadUInt32();
            return b;
        }
        public static OfpPortFeatures Parse(this BinaryReader br, out OfpPortFeatures b)
        {
            b = (OfpPortFeatures)br.ReadUInt32();
            return b;
        }

        public static OfpQueueProperties Parse(this BinaryReader br, out OfpQueueProperties b)
        {
            b = (OfpQueueProperties)br.ReadUInt16();
            return b;
        }

        public static OfpFlowWildcards Parse(this BinaryReader br, out OfpFlowWildcards b)
        {
            b = (OfpFlowWildcards)br.ReadUInt32();
            return b;
        }

        public static OfpWildcards Parse(this BinaryReader br, out OfpWildcards b)
        {
            b = new OfpWildcards(br.ReadUInt32());
            return b;
        }

        public static OfpActionType Parse(this BinaryReader br, out OfpActionType b)
        {
            b = (OfpActionType)br.ReadUInt16();
            return b;
        }

        public static OfpCapabilities Parse(this BinaryReader br, out OfpCapabilities b)
        {
            b = (OfpCapabilities)br.ReadUInt32();
            return b;
        }

        public static OfpActionCapabilities Parse(this BinaryReader br, out OfpActionCapabilities b)
        {
            b = (OfpActionCapabilities)br.ReadUInt32();
            return b;
        }

        public static OfpConfigFlags Parse(this BinaryReader br, out OfpConfigFlags b)
        {
            b = (OfpConfigFlags)br.ReadUInt16();
            return b;
        }

        //Writer

    }
}
