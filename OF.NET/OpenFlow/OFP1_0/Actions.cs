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
    /// 动作
    /// </summary>
    public interface IOfpAction
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
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Type);
            br.Parse(out Len);
            //br.ReadBytes(4); //PAD 4
        }

        public OfpActionHeader(Stream stream, int padding)
        {
            _pad = padding;
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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
    }

    /// <summary>
    /// 输出
    /// </summary>
    public class OfpActionOutput : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_OUTPUT,
            Len = 8
        };

        /// <summary>
        /// 出端口
        /// </summary>
        public ushort Port;

        /// <summary>
        /// 发送到控制器的最大长度
        /// </summary>
        public ushort MaxLen;

        public OfpActionOutput()
        { }

        public OfpActionOutput(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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
    public class OfpActionEnqueue : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_ENQUEUE,
            Len = 16
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
        { }

        public OfpActionEnqueue(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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
    public class OfpActionVlanVid : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_VLAN_VID,
            Len = 8
        };

        /// <summary>
        /// VLAN ID
        /// <remarks>实际的VLAN ID只有12位。0xFFFF用来表明未设置VLAN ID</remarks>
        /// </summary>
        public ushort VlanVid;
        //PAD 2

        public OfpActionVlanVid()
        { }

        public OfpActionVlanVid(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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
    public class OfpActionVlanPcp : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_VLAN_PCP,
            Len = 8
        };

        /// <summary>
        /// VLAN优先级
        /// <remarks>仅有第3位有意义</remarks>
        /// </summary>
        public byte VlanPcp;

        //PAD 3

        public OfpActionVlanPcp()
        { }

        public OfpActionVlanPcp(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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
    public class OfpActionStripVlan : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader() { Type = Data.OfpActionType.OFPAT_STRIP_VLAN, Len = 8 };

        //PAD 4

        public OfpActionStripVlan()
        { }

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
    public class OfpActionDlAddr : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_DL_SRC,
            Len = 16
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
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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
    /// 设置IP ToS
    /// </summary>
    public class OfpActionNwTos : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_NW_TOS,
            Len = 8
        };

        /// <summary>
        /// IP ToS（DSCP，6位）
        /// </summary>
        public byte NwTos;

        //PAD 3

        public OfpActionNwTos()
        { }

        public OfpActionNwTos(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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

    public class OfpActionTpPort : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader()
        {
            Type = Data.OfpActionType.OFPAT_SET_TP_SRC,
            Len = 8
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
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
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
    public class OfpActionVendorHeader : IOfpAction, IToByteArray
    {
        public OfpActionHeader Header { get; private set; } = new OfpActionHeader() { Type = Data.OfpActionType.OFPAT_VENDOR, Len = 0 };

        /// <summary>
        /// 厂商ID
        /// </summary>
        public uint Vendor;

        public OfpActionVendorHeader()
        { }

        public OfpActionVendorHeader(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpActionHeader(stream);
            br.Parse(out Vendor);
        }

        public byte[] ToByteArray()
        {
            if (Header.Len <= 0 || Header.Len % 8 != 0)
            {
                throw new ArgumentException("Invalid Length in Header");
            }
            Writer w = new Writer(Header.Len);
            w.Write(Header.ToByteArray());
            w.Write(Vendor);
            w.ToByteArray();
            return w.ToByteArray();
        }
    }
}
