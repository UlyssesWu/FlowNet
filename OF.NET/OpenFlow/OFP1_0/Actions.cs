﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Be.IO;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.OpenFlow.OFP1_0
{

    /// <summary>
    /// 动作
    /// </summary>
    public interface IOfpAction : IToByteArray
    {
        /// <summary>
        /// 动作头
        /// </summary>
        OfpActionHeader Header { get; }
    }

    

    /// <summary>
    /// 动作头
    /// <remarks>动作消息的长度应该总为8的倍数</remarks>
    /// </summary>
    public class OfpActionHeader : IToByteArray
    {
        public const uint Size = 4;
        /// <summary>
        /// OFPAT_之一
        /// </summary>
        public Data.OfpActionType Type;
        /// <summary>
        /// 包括此头在内的总长度，应为8的倍数
        /// </summary>
        public ushort Len;
        //若无其它内容，应增加PAD 4

        private int _pad = 0;

        public OfpActionHeader()
        { }

        public OfpActionHeader(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Type);
            br.Parse(out Len);
            //br.ReadBytes(4); //PAD 4
        }

        public OfpActionHeader(Stream stream, int padding)
        {
            _pad = padding;
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Type);
            br.Parse(out Len);
            br.ReadBytes(_pad); //PAD 4
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Type);
            w.Write(Len);
            w.Pad(_pad);
            return w.ToByteArray();
        }

        /// <summary>
        /// 通过头解析一个动作
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IOfpAction ParseAction(Stream stream)
        {
            if (stream.Length - stream.Position >= 8)
            {
                var pos = stream.Position;
                OfpActionHeader header = new OfpActionHeader(stream);
                stream.Seek(pos, SeekOrigin.Begin);
                switch (header.Type)
                {
                    case OfpActionType.OFPAT_OUTPUT:
                        return new OfpActionOutput(stream);
                    case OfpActionType.OFPAT_SET_VLAN_VID:
                        return new OfpActionVlanVid(stream);
                    case OfpActionType.OFPAT_SET_VLAN_PCP:
                        return new OfpActionVlanPcp(stream);
                    case OfpActionType.OFPAT_STRIP_VLAN:
                        return new OfpActionStripVlan(stream);
                    case OfpActionType.OFPAT_SET_DL_SRC:
                        return new OfpActionDlAddr(stream);
                    case OfpActionType.OFPAT_SET_DL_DST:
                        return new OfpActionDlAddr(stream);
                    case OfpActionType.OFPAT_SET_NW_SRC:
                        return new OfpActionNwAddr(stream);
                    case OfpActionType.OFPAT_SET_NW_DST:
                        return new OfpActionNwAddr(stream);
                    case OfpActionType.OFPAT_SET_NW_TOS:
                        return new OfpActionNwTos(stream);
                    case OfpActionType.OFPAT_SET_TP_SRC:
                        return new OfpActionTpPort(stream);
                    case OfpActionType.OFPAT_SET_TP_DST:
                        return new OfpActionTpPort(stream);
                    case OfpActionType.OFPAT_ENQUEUE:
                        return new OfpActionEnqueue(stream);
                    case OfpActionType.OFPAT_VENDOR:
                        return new OfpActionVendorHeader(stream);
                    default:
                        return null;
                        //throw new FormatException("Can not parse header");
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 输出
    /// </summary>
    public class OfpActionOutput : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_OUTPUT, Len = 8
        };

        /// <summary>
        /// 出端口
        /// <remarks>可为OFPP_CONTROLLER或一个物理端口</remarks>
        /// </summary>
        public ushort Port;

        /// <summary>
        /// 发送到控制器的最大长度
        /// </summary>
        public ushort MaxLen = ushort.MaxValue - 1;

        public OfpActionOutput()
        {
        }

        public OfpActionOutput(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out Port);
            br.Parse(out MaxLen);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(Port);
            w.Write(MaxLen);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 入队列
    /// </summary>
    public class OfpActionEnqueue : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_ENQUEUE, Len = 16
        };

        /// <summary>
        /// 队列从属的端口，应为有效的物理端口或OFPP_IN_PORT
        /// </summary>
        public ushort Port;

        //PAD 6

        /// <summary>
        /// Where to qnqueue the packets
        /// </summary>
        public uint QueueId;

        public OfpActionEnqueue()
        {
        }

        public OfpActionEnqueue(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out Port);
            br.ReadBytes(6); //PAD 6
            br.Parse(out QueueId);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(Port);
            w.Pad(6);
            w.Write(QueueId);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 设置VLAN VID
    /// </summary>
    public class OfpActionVlanVid : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_VLAN_VID, Len = 8
        };

        /// <summary>
        /// VLAN ID
        /// <remarks>实际的VLAN ID只有12位。0xFFFF用来表明未设置VLAN ID</remarks>
        /// </summary>
        public ushort VlanVid;

        //PAD 2

        public OfpActionVlanVid()
        {
        }

        public OfpActionVlanVid(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out VlanVid);
            br.ReadBytes(2); //PAD 2
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(VlanVid);
            w.Pad(2);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 设置VLAN优先级
    /// </summary>
    public class OfpActionVlanPcp : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_VLAN_PCP, Len = 8
        };

        /// <summary>
        /// VLAN优先级
        /// <remarks>仅有第3位有意义</remarks>
        /// </summary>
        public byte VlanPcp;

        //PAD 3

        public OfpActionVlanPcp()
        {
        }

        public OfpActionVlanPcp(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out VlanPcp);
            br.ReadBytes(3);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(VlanPcp);
            w.Pad(3);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 剥离VLAN Tag
    /// </summary>
    public class OfpActionStripVlan : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader() {Type = Data.OfpActionType.OFPAT_STRIP_VLAN, Len = 8};

        //PAD 4

        public OfpActionStripVlan()
        {
        }

        public OfpActionStripVlan(Stream stream)
        {
            Header = new OfpActionHeader(stream, 4);
        }

        public byte[] ToByteArray()
        {
            return Header.ToByteArray();
        }
    }

    /// <summary>
    /// 设置源/目的MAC地址
    /// </summary>
    public class OfpActionDlAddr : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_DL_SRC, Len = 16
        };

        /// <summary>
        /// 要设置的MAC地址
        /// </summary>
        public byte[] DlAddr = new byte[OFP_MAX_ETH_ALEN];

        //PAD 6

        public OfpActionDlAddr(bool isDst = false)
        {
            Header.Type = isDst ? Data.OfpActionType.OFPAT_SET_DL_DST : Data.OfpActionType.OFPAT_SET_DL_SRC;
        }

        public OfpActionDlAddr(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out DlAddr, OFP_MAX_ETH_ALEN);
            br.ReadBytes(6);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(DlAddr, OFP_MAX_ETH_ALEN);
            w.Pad(6);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 设置IP地址
    /// </summary>
    public class OfpActionNwAddr : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_NW_SRC,
            Len = 8
        };

        /// <summary>
        /// IP地址
        /// </summary>
        public uint NwAddr;

        public OfpActionNwAddr(bool isDst = false)
        {
            Header.Type = isDst ? Data.OfpActionType.OFPAT_SET_NW_DST : Data.OfpActionType.OFPAT_SET_NW_SRC;
        }

        public OfpActionNwAddr(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out NwAddr);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(NwAddr);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 设置IP ToS
    /// </summary>
    public class OfpActionNwTos : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_NW_TOS, Len = 8
        };

        /// <summary>
        /// IP ToS（DSCP，6位）
        /// </summary>
        public byte NwTos;

        //PAD 3

        public OfpActionNwTos()
        {
        }

        public OfpActionNwTos(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out NwTos);
            br.ReadBytes(3);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(NwTos);
            w.Pad(3);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 设置目标端口
    /// </summary>
    public class OfpActionTpPort : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_TP_SRC, Len = 8
        };

        /// <summary>
        /// TCP/UDP端口
        /// </summary>
        public ushort TpPort;

        //PAD 2

        public OfpActionTpPort(bool isDst = false)
        {
            Header.Type = isDst ? Data.OfpActionType.OFPAT_SET_TP_DST : Data.OfpActionType.OFPAT_SET_TP_SRC;
        }

        public OfpActionTpPort(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out TpPort);
            br.ReadBytes(2); //PAD 2
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(TpPort);
            w.Pad(2);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 厂商自定义动作头
    /// </summary>
    public class OfpActionVendorHeader : IOfpAction
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader() {Type = Data.OfpActionType.OFPAT_VENDOR, Len = 0};

        /// <summary>
        /// 厂商ID
        /// </summary>
        public uint Vendor;

        /// <summary>
        /// 包内容
        /// </summary>
        public byte[] Content;

        public OfpActionVendorHeader()
        {
        }

        public OfpActionVendorHeader(Stream stream)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out Vendor);
            br.Parse(out Content, Header.Len);
        }

        public byte[] ToByteArray()
        {
            if (Header.Len <= 0 || Header.Len%8 != 0)
            {
                throw new ArgumentException("Invalid Length in Header");
            }
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(Vendor);
            w.Write(Content);
            w.ToByteArray();
            return w.ToByteArray();
        }
    }
}
