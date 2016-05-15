using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Be.IO;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.OpenFlow.OFP1_0
{
    public interface IToByteArray
    {
        /// <summary>
        /// 转为字节数组
        /// </summary>
        /// <returns></returns>
        byte[] ToByteArray();
    }

    /// <summary>
    /// 所有OF包的头
    /// <remarks>8字节</remarks>
    /// </summary>
    public class OfpHeader : IToByteArray
    {
        public const uint Size = 8;
        /// <summary>
        /// OFP_VERSION
        /// </summary>
        public byte Version = 0x1;
        /// <summary>
        /// OFPT_枚举之一
        /// </summary>
        public OfpType Type = OfpType.OFPT_HELLO;
        /// <summary>
        /// 包含此ofp_header在内的长度
        /// </summary>
        public ushort Length = 8;
        /// <summary>
        /// 包相关的事务ID
        /// </summary>
        public uint Xid = 1;

        public OfpHeader()
        { }
        public OfpHeader(byte[] stream)
        {
            Version = stream[0];
            //ArraySegment<byte> segment = new ArraySegment<byte>(stream,1,2);
            //BitConverter.ToUInt16(stream.Skip(1).Take(2).Reverse().ToArray(), 0);
            Type = (OfpType)stream[1];
            Length = BitConverter.ToUInt16(stream.Skip(2).Take(2).Reverse().ToArray(), 0);
            Xid = BitConverter.ToUInt32(stream.Skip(4).Take(4).Reverse().ToArray(), 0);
        }

        public OfpHeader(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Version);
            br.Parse(out Type);
            br.Parse(out Length);
            br.Parse(out Xid);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Version);
            w.Write(Type);
            w.Write(Length);
            w.Write(Xid);
            return w.ToByteArray();
        }
    }


    /// <summary>
    /// 物理端口
    /// </summary>
    public class OfpPhyPort : IToByteArray
    {
        public const uint Size = 48;

        /// <summary>
        /// 端口号
        /// </summary>
        public ushort PortNo;
        /// <summary>
        /// 硬件地址（MAC） 6位
        /// </summary>
        public byte[] HwAddr = new byte[OFP_MAX_ETH_ALEN];

        ///// <summary>
        ///// MAC
        ///// </summary>
        //public PhysicalAddress HardwareAddress => new PhysicalAddress(HwAddr);

        /// <summary>
        /// 名称
        /// <para>char name[OFP_MAX_PORT_NAME_LEN]</para>
        /// </summary>
        public string Name;

        /// <summary>
        /// 配置
        /// </summary>
        public Data.OfpPortConfig Config;

        /// <summary>
        /// 状态
        /// </summary>
        public Data.OfpPortState State;

        /// <summary>
        /// 当前功能
        /// </summary>
        public Data.OfpPortFeatures Curr;

        /// <summary>
        /// 公开功能
        /// </summary>
        public Data.OfpPortFeatures Advertised;

        /// <summary>
        /// 支持功能
        /// </summary>
        public Data.OfpPortFeatures Supported;

        /// <summary>
        /// Peer公开的功能
        /// </summary>
        public OfpPortFeatures Peer;

        public OfpPhyPort()
        {
        }

        public OfpPhyPort(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out PortNo);
            br.Parse(out HwAddr, OFP_MAX_ETH_ALEN);
            br.Parse(out Name, OFP_MAX_PORT_NAME_LEN);
            br.Parse(out Config);
            br.Parse(out State);
            br.Parse(out Curr);
            br.Parse(out Advertised);
            br.Parse(out Supported);
            br.Parse(out Peer);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(PortNo);
            w.Write(HwAddr, OFP_MAX_ETH_ALEN);
            w.Write(Name, OFP_MAX_PORT_NAME_LEN);
            w.Write(Config);
            w.Write(State);
            w.Write(Curr);
            w.Write(Advertised);
            w.Write(Supported);
            w.Write(Peer);
            return w.ToByteArray();
        }

        /// <summary>
        /// 判断是否为保留端口
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsReservedPort(ushort port)
        {
            return Enum.IsDefined(typeof (OfpPort), port);
        }

        /// <summary>
        /// 解析一个保留端口
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static OfpPort ParseReservedPort(ushort port)
        {
            switch (port)
            {
                case (ushort)OfpPort.OFPP_MAX:
                    return OfpPort.OFPP_MAX;
                case (ushort)OfpPort.OFPP_IN_PORT:
                    return OfpPort.OFPP_IN_PORT;
                case (ushort)OfpPort.OFPP_TABLE:
                    return OfpPort.OFPP_TABLE;
                case (ushort)OfpPort.OFPP_NORMAL:
                    return OfpPort.OFPP_NORMAL;
                case (ushort)OfpPort.OFPP_FLOOD:
                    return OfpPort.OFPP_FLOOD;
                case (ushort)OfpPort.OFPP_ALL:
                    return OfpPort.OFPP_ALL;
                case (ushort)OfpPort.OFPP_CONTROLLER:
                    return OfpPort.OFPP_CONTROLLER;
                case (ushort)OfpPort.OFPP_LOCAL:
                    return OfpPort.OFPP_LOCAL;
                case (ushort)OfpPort.OFPP_NONE:
                    return OfpPort.OFPP_NONE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(port), port, "This port is not a reserved port.");
            }
        }
    }

    /// <summary>
    /// 队列
    /// </summary>
    public class OfpPacketQueue : IToByteArray
    {
        public const uint Size = 8;

        /// <summary>
        /// 特定队列的ID
        /// </summary>
        public uint QueueId;

        /// <summary>
        /// Length in bytes of this queue desc.
        /// </summary>
        public ushort Len;

        //PAD 2

        public List<OfpQueuePropHeader> Properties;

        public OfpPacketQueue()
        {
        }

        public OfpPacketQueue(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out QueueId);
            br.Parse(out Len);
            br.ReadBytes(2); //PAD
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(QueueId);
            w.Write(Len);
            w.Pad(2);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 队列属性头
    /// </summary>
    public class OfpQueuePropHeader : IToByteArray
    {
        public const uint Size = 8;

        /// <summary>
        /// OFPQT_之一
        /// </summary>
        public Data.OfpQueueProperties Property;

        /// <summary>
        /// 包含此头在内的属性长度
        /// </summary>
        public ushort Len;

        //PAD 4
        public OfpQueuePropHeader()
        {
        }

        public OfpQueuePropHeader(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Property);
            br.Parse(out Len);
            br.ReadBytes(4);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Property);
            w.Write(Len);
            w.Pad(4);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 最小速率队列
    /// </summary>
    public class OfpQueuePropMinRate : IToByteArray
    {
        public const uint Size = 16;

        /// <summary>
        /// prop: OFPQT_MIN_RATE，len: 16
        /// </summary>
        public OfpQueuePropHeader PropHeader = new OfpQueuePropHeader() {Property = Data.OfpQueueProperties.QFPQT_MIN_RATE};

        /// <summary>
        /// 1/10比例，>1000为禁用
        /// </summary>
        public ushort Rate;

        //PAD 6
        public OfpQueuePropMinRate()
        {
        }

        public OfpQueuePropMinRate(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            PropHeader = new OfpQueuePropHeader(stream);
            br.Parse(out Rate);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(PropHeader.ToByteArray());
            w.Write(Rate);
            w.Pad(6);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 通配符
    /// </summary>
    public class OfpWildcards : IToByteArray
    {
        public const int Size = 4;

        /// <summary>
        /// 通配位
        /// <remarks>注意：OFP1.0中，1为屏蔽，0为匹配</remarks>
        /// </summary>
        public Data.OfpFlowWildcards Wildcards
        {
            get { return (Data.OfpFlowWildcards) _value; }
            set
            {
                _value = _value & (OFPFW_NW_SRC_MASK | OFPFW_NW_DST_MASK); //清零非Mask位
                _value = _value | (uint) value; //设置非Mask位
            }
        }

        /// <summary>
        /// IP源地址通配位
        /// <remarks>0为精确匹配，1忽略最低位，2忽略后两位，32及以上全部通配（即不匹配IP）</remarks>
        /// </summary>
        public ushort NwSrcMask
        {
            get { return (ushort) ((_value & OFPFW_NW_SRC_MASK) >> OFPFW_NW_SRC_SHIFT); }
            set
            {
                _value = _value & (~OFPFW_NW_SRC_MASK); //清零Mask位
                _value = _value | (((uint) value & ((1 << OFPFW_NW_SRC_BITS) - 1)) << OFPFW_NW_SRC_SHIFT);
            }
        }

        /// <summary>
        /// IP目的地址通配位
        /// <remarks>0为精确匹配，1忽略最低位，2忽略后两位，32及以上全部通配（即不匹配IP）</remarks>
        /// </summary>
        public ushort NwDstMask
        {
            get { return (ushort) ((_value & OFPFW_NW_DST_MASK) >> OFPFW_NW_DST_SHIFT); }
            set
            {
                _value = _value & (~OFPFW_NW_DST_MASK); //清零Mask位
                _value = _value | (((uint) value & ((1 << OFPFW_NW_DST_BITS) - 1)) << OFPFW_NW_DST_SHIFT);
            }
        }

        /// <summary>
        /// Wildcard值
        /// </summary>
        public uint Value => _value;

        private uint _value;

        public OfpWildcards()
        {
        }

        /// <summary>
        /// 从流中读一个UInt32生成通配符
        /// </summary>
        /// <param name="stream"></param>
        public OfpWildcards(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            _value = br.ReadUInt32();
        }

        /// <summary>
        /// 用一个UInt32生成通配符
        /// </summary>
        /// <param name="value"></param>
        public OfpWildcards(uint value)
        {
            _value = value;
        }

        public byte[] ToByteArray()
        {
            return BitConverter.GetBytes(_value);
        }

        public override bool Equals(object obj)
        {
            if (obj is OfpWildcards)
            {
                return this._value == ((OfpWildcards) obj).Value;
            }
            return base.Equals(obj);
        }
    }

    public class OfpMatch : IToByteArray
    {
        public const uint Size = 40;

        /// <summary>
        /// 通配
        /// </summary>
        public OfpWildcards Wildcards = new OfpWildcards() {Wildcards = OfpFlowWildcards.OFPFW_ALL};

        /// <summary>
        /// 入端口
        /// </summary>
        public ushort InPort;

        /// <summary>
        /// 以太网源地址
       
        /// <summary>
        /// 以太网目的地址 /// </summary>
        public byte[] DlSrc = new byte[OFP_MAX_ETH_ALEN];

        /// </summary>
        public byte[] DlDst = new byte[OFP_MAX_ETH_ALEN];

        /// <summary>
        /// 入VLAN ID
        /// </summary>
        public ushort DlVlan = 65535;

        /// <summary>
        /// 入VLAN优先级
        /// </summary>
        public byte DlVlanPcp;

        //PAD 1

        /// <summary>
        /// 以太网帧类型
        /// </summary>
        public ushort DlType;

        /// <summary>
        /// IP ToS（实际为DSCP，6位）
        /// </summary>
        public byte NwTos;

        /// <summary>
        /// IP协议或ARP操作码低8位
        /// </summary>
        public byte NwProto;

        //PAD 2
        //TODO： how to handle IP address?
        /// <summary>
        /// IP源地址
        /// </summary>
        public uint NwSrc;

        /// <summary>
        /// IP目的地址
        /// </summary>
        public uint NwDst;

        /// <summary>
        /// TCP/UDP源端口
        /// </summary>
        public ushort TpSrc;

        /// <summary>
        /// TCP/UDP目的端口
        /// </summary>
        public ushort TpDst;

        public OfpMatch()
        {
        }

        public OfpMatch(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Wildcards);
            br.Parse(out InPort);
            br.Parse(out DlSrc, OFP_MAX_ETH_ALEN);
            br.Parse(out DlDst, OFP_MAX_ETH_ALEN);
            br.Parse(out DlVlan);
            br.Parse(out DlVlanPcp);
            br.ReadBytes(1); //PAD 1
            br.Parse(out DlType);
            br.Parse(out NwTos);
            br.Parse(out NwProto);
            br.ReadBytes(2); //PAD 2
            br.Parse(out NwSrc);
            br.Parse(out NwDst);
            br.Parse(out TpSrc);
            br.Parse(out TpDst);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Wildcards);
            w.Write(InPort);
            w.Write(DlSrc, OFP_MAX_ETH_ALEN);
            w.Write(DlDst, OFP_MAX_ETH_ALEN);
            w.Write(DlVlan);
            w.Write(DlVlanPcp);
            w.Pad(1);
            w.Write(DlType);
            w.Write(NwTos);
            w.Write(NwProto);
            w.Pad(2);
            w.Write(NwSrc);
            w.Write(NwDst);
            w.Write(TpSrc);
            w.Write(TpDst);

            return w.ToByteArray();
        }
    }
}
