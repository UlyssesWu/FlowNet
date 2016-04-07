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
    /// 包进入消息
    /// </summary>
    public class OfpPacketIn : IOfpMessage
    {
        public const uint Size = 20;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_PACKET_IN };

        /// <summary>
        /// Datapath ID
        /// </summary>
        public uint BufferId;

        /// <summary>
        /// 包的真实长度
        /// </summary>
        public ushort TotalLen;

        /// <summary>
        /// 接收包的端口
        /// </summary>
        public ushort InPort;

        /// <summary>
        /// 包被发送过来的原因
        /// <remarks><para>若流表项要求发送给控制器，则Output行为中的<see cref="OfpActionOutput.MaxLen"/>个字节被发送；</para><para>若因查不到匹配表项而被发送，则至少miss_send_len长度（OFPT_SET_CONFIG消息）被发送，若包未放入缓存，则整个包内容都发送给控制器，此时<see cref="BufferId"/>为-1</para></remarks>
        /// </summary>
        public OfpPacketInReason Reason;

        //PAD 1

        /// <summary>
        /// 包的全部或部分内容
        /// </summary>
        public byte[] Data;

        public OfpPacketIn()
        { }

        public OfpPacketIn(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
            br.Parse(out BufferId);
            br.Parse(out TotalLen);
            br.Parse(out InPort);
            br.Parse(out Reason);
            br.ReadBytes(1);
            int length = (int)(Header.Length - Size);
            Data = br.ReadBytes(length);
        }

        public uint UpdateLength()
        {
            uint len = (uint)(Size + Data.Length);
            Header.Length = (ushort)len;
            return len;
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer();
            w.Write(Header.ToByteArray());
            w.Write(BufferId);
            w.Write(TotalLen);
            w.Write(InPort);
            w.Write(Reason);
            w.Pad(1);
            w.Write(Data);
            return w.ToByteArray();
        }

    }

    /// <summary>
    /// 流删除消息
    /// </summary>
    public class OfpFlowRemoved : IOfpMessage
    {
        public const uint Size = 88;

        public OfpHeader Header { get; private set; } = new OfpHeader()
        {
            Type = OfpType.OFPT_FLOW_REMOVED,
            Length = 88
        };

        /// <summary>
        /// 匹配域描述
        /// </summary>
        public OfpMatch Match;

        /// <summary>
        /// Opaque controller-issued identifier
        /// </summary>
        public ulong Cookie;

        /// <summary>
        /// 流表项优先级
        /// </summary>
        public ushort Priority;

        /// <summary>
        /// 被删除的理由
        /// </summary>
        public OfpFlowRemovedReason Reason;

        //PAD 1

        /// <summary>
        /// 生存时间（秒部分）
        /// </summary>
        public uint DurationSec;

        /// <summary>
        /// 生存时间（纳秒部分）
        /// </summary>
        public uint DurationNsec;

        /// <summary>
        /// 设定的空闲超时时间
        /// </summary>
        public ushort IdleTimeout;

        //PAD 2

        /// <summary>
        /// 包计数
        /// </summary>
        public ulong PacketCount;

        /// <summary>
        /// 流量计数
        /// </summary>
        public ulong ByteCount;

        public OfpFlowRemoved()
        { }

        public OfpFlowRemoved(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
            Match = new OfpMatch(stream);
            br.Parse(out Cookie);
            br.Parse(out Priority);
            br.Parse(out Reason);
            br.ReadBytes(1);
            br.Parse(out DurationSec);
            br.Parse(out DurationNsec);
            br.Parse(out IdleTimeout);
            br.ReadBytes(2);
            br.Parse(out PacketCount);
            br.Parse(out ByteCount);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Header.ToByteArray());
            w.Write(Match.ToByteArray());
            w.Write(Cookie);
            w.Write(Priority);
            w.Write(Reason);
            w.Pad(1);
            w.Write(DurationSec);
            w.Write(DurationNsec);
            w.Write(IdleTimeout);
            w.Pad(2);
            w.Write(PacketCount);
            w.Write(ByteCount);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 端口状态消息
    /// </summary>
    public class OfpPortStatus : IOfpMessage
    {
        public const uint Size = 64;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_PORT_STATUS, Length = 64 };

        /// <summary>
        /// 端口的更改情况（ADD/DELETE/MODIFY）
        /// </summary>
        public OfpPortReason Reason;

        //PAD 7

        /// <summary>
        /// 端口描述
        /// </summary>
        public OfpPhyPort Desc;

        public OfpPortStatus()
        { }

        public OfpPortStatus(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
            br.Parse(out Reason);
            br.ReadBytes(7);
            Desc = new OfpPhyPort(stream);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Header.ToByteArray());
            w.Write(Reason);
            w.Pad(7);
            w.Write(Desc.ToByteArray());
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 错误消息（交换机通知控制器）
    /// </summary>
    public class OfpErrorMsg : IOfpMessage
    {
        public const uint Size = 12;

        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_ERROR };

        public OfpErrorType Type;

        public ushort Code;

        public byte[] Data;

        public OfpErrorMsg()
        { }

        public OfpErrorMsg(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
            br.Parse(out Type);
            br.Parse(out Code);
            int length = (int)(Header.Length - Size);
            br.Parse(out Data, length);
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
            w.Write(Type);
            w.Write(Code);
            w.Write(Data);
            return w.ToByteArray();
        }

    }

}
