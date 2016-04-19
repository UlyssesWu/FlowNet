using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Be.IO;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.OpenFlow.OFP1_0
{
    /// <summary>
    /// 交换机功能请求消息
    /// </summary>
    public class OfpSwitchFeaturesRequest : IOfpMessage
    {
        public OfpHeader Header { get; } = new OfpHeader()
        { Type = OfpType.OFPT_FEATURES_REQUEST, Length = 8 };

        public OfpSwitchFeaturesRequest()
        { }

        public byte[] ToByteArray()
        {
            return Header.ToByteArray();
        }

    }

    /// <summary>
    /// 交换机功能回复消息
    /// <remarks>控制器发送仅有消息头的OFPT_FEATURES_REQUEST消息，交换机返回OFPT_FEATURES_REPLY消息</remarks>
    /// </summary>
    public class OfpSwitchFeatures : IOfpMessage
    {
        /// <summary>
        /// 不含端口定义在内的长度
        /// </summary>
        public const uint Size = 32;

        public OfpHeader Header { get; private set; } = new OfpHeader()
        {
            Type = OfpType.OFPT_FEATURES_REPLY,
            Length = 32
        };
        /// <summary>
        /// Datapath ID
        /// <remarks>低48位为MAC地址，高16位看具体实现</remarks>
        /// </summary>
        public UInt64 DatapathId;

        /// <summary>
        /// 一次最多缓存的包数量
        /// </summary>
        public uint NBuffers;

        /// <summary>
        /// Datapath所支持的表数量
        /// <remarks>交换机支持的流表个数</remarks>
        /// </summary>
        public byte NTables;

        //PAD 3 for 64-bit align

        /// <summary>
        /// 功能兼容性Bitmap 
        /// </summary>
        public OfpCapabilities Capabilities;

        /// <summary>
        /// 动作兼容性Bitmap
        /// </summary>
        public OfpActionCapabilities Actions;

        /// <summary>
        /// 端口定义
        /// <remarks>端口数量从头的Length来推断</remarks>
        /// </summary>
        public List<OfpPhyPort> Ports = new List<OfpPhyPort>();

        public OfpSwitchFeatures()
        { }

        public OfpSwitchFeatures(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            br.Parse(out DatapathId);
            br.Parse(out NBuffers);
            br.Parse(out NTables);
            br.ReadBytes(3); //PAD 3
            br.Parse(out Capabilities);
            br.Parse(out Actions);
            var portCount = (Header.Length - Size) / OfpPhyPort.Size;
            for (int i = 0; i < portCount; i++)
            {
                OfpPhyPort port = new OfpPhyPort(stream);
                Ports.Add(port);
            }

        }

        /// <summary>
        /// 计算长度
        /// </summary>
        /// <returns></returns>
        public uint UpdateLength()
        {
            var length = (uint)(Size + Ports.Count * OfpPhyPort.Size);
            Header.Length = (ushort)length; //TODO: length check
            return length;
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer();
            UpdateLength();
            w.Write(Header.ToByteArray());
            w.Write(DatapathId);
            w.Write(NBuffers);
            w.Write(NTables);
            w.Pad(3);
            w.Write(Capabilities);
            w.Write(Actions);
            foreach (var port in Ports)
            {
                w.Write(port.ToByteArray());
            }
            return w.ToByteArray();
        }

    }

    //There is no body for OFPT_GET_CONFIG_REQUEST beyond the OpenFlow header.
    /// <summary>
    /// 请求交换机配置消息
    /// </summary>
    public class OfpSwitchConfigRequest : IOfpMessage
    {
        public OfpHeader Header { get; } = new OfpHeader()
        { Type = OfpType.OFPT_GET_CONFIG_REQUEST, Length = 8 };

        public OfpSwitchConfigRequest()
        { }

        public byte[] ToByteArray()
        {
            return Header.ToByteArray();
        }

    }

    //The OFPT_SET_CONFIG and OFPT_GET_CONFIG_REPLY use the following:

    /// <summary>
    /// 交换机配置消息
    /// <remarks>控制器发送OFPT_GET_CONFIG_REQUEST（仅有头部）来查询配置，发送OFPT_SET_CONFIG修改配置。对于查询，交换机返回OFPT_GET_CONFIG_REPLY</remarks>
    /// </summary>
    public class OfpSwitchConfig : IOfpMessage
    {
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_GET_CONFIG_REPLY, Length = 12 };

        /// <summary>
        /// OFPC_之一
        /// </summary>
        public OfpConfigFlags Flags;

        /// <summary>
        /// 当包需要发送到控制器时，发送的长度
        /// <remarks>如为0，则只发送packet_in消息</remarks>
        /// </summary>
        public ushort MissSendLen;

        public OfpSwitchConfig(bool isSet = false)
        {
            Header.Type = isSet ? OfpType.OFPT_SET_CONFIG : OfpType.OFPT_GET_CONFIG_REPLY;
        }

        public OfpSwitchConfig(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            br.Parse(out Flags);
            br.Parse(out MissSendLen);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Length);
            w.Write(Header.ToByteArray());
            w.Write(Flags);
            w.Write(MissSendLen);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 流表修改消息
    /// </summary>
    public class OfpFlowMod : IOfpMessage
    {
        /// <summary>
        /// 不包括Actions在内的长度
        /// </summary>
        public const uint Size = 72;

        public OfpHeader Header { get; private set; } = new OfpHeader() { Type = OfpType.OFPT_FLOW_MOD };

        /// <summary>
        /// 匹配域
        /// </summary>
        public OfpMatch Match;

        /// <summary>
        /// Opaque controller-issued identifier
        /// </summary>
        public ulong Cookie;

        /// <summary>
        /// 命令，OFPFC_之一
        /// </summary>
        public OfpFlowModCommand Command;

        /// <summary>
        /// 空闲超时时限（秒）
        /// </summary>
        public ushort IdleTimeout;

        /// <summary>
        /// 最大超时时限（秒）
        /// <remarks>0表示永不过期</remarks>
        /// </summary>
        public ushort HardTimeout;

        /// <summary>
        /// 优先级
        /// <remarks>数字越高，优先级越高。高优先级的流表项会放到低编号的流表中</remarks>
        /// </summary>
        public ushort Priority;

        /// <summary>
        /// 被OFPT_PACKET_IN消息发出的网包在buffer中的ID
        /// </summary>
        public uint BufferId;

        /// <summary>
        /// （可选）删除操作时的匹配
        /// </summary>
        public ushort OutPort;

        /// <summary>
        /// Flags
        /// <remarks>若设置OFPFF_SEND_FLOW_REM，则指令流删除时需要向控制器回复flow_removed消息</remarks>
        /// </summary>
        public OfpFlowModFlags Flags;

        /// <summary>
        /// 动作
        /// <remarks>长度由头部推断</remarks>
        /// </summary>
        //public Dictionary<OfpActionType, IOfpAction> Actions = new Dictionary<OfpActionType, IOfpAction>(); 
        public ActionList Actions = new ActionList();

        public OfpFlowMod()
        { }

        public OfpFlowMod(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            //var pos = stream.Position;
            Match = new OfpMatch(stream);
            //pos = stream.Position;
            br.Parse(out Cookie);
            br.Parse(out Command);
            br.Parse(out IdleTimeout);
            br.Parse(out HardTimeout);
            br.Parse(out Priority);
            br.Parse(out BufferId);
            br.Parse(out OutPort); //FIXED: 
            br.Parse(out Flags);
            //pos = stream.Position;
            IOfpAction action = OfpActionHeader.ParseAction(stream);
            while (action != null)
            {
                Actions[action.Header.Type] = action;
                action = OfpActionHeader.ParseAction(stream);
            }
        }

        /// <summary>
        /// 计算长度
        /// </summary>
        public uint UpdateLength()
        {
            uint len = Size + (uint)Actions.Length;
            Header.Length = (ushort)len;
            return len;
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer();
            UpdateLength();
            //var pos = (long)Header.Length;
            w.Write(Header.ToByteArray());
            //pos = w.Position;
            w.Write(Match.ToByteArray());
            //pos = w.Position;
            w.Write(Cookie);
            w.Write(Command);
            w.Write(IdleTimeout);
            w.Write(HardTimeout);
            w.Write(Priority);
            w.Write(BufferId);
            w.Write(OutPort);
            w.Write(Flags);
            //pos = w.Position;
            w.Write(Actions.ToByteArray());
            //pos = w.Position;
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 控制器查询队列配置
    /// </summary>
    public class OfpQueueGetConfigRequest : IOfpMessage
    {
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Length = 12, Type = OfpType.OFPT_QUEUE_GET_CONFIG_REQUEST };

        /// <summary>
        /// 要查询的端口
        /// <remarks>应当为有效的物理端口，小于OFPP_MAX</remarks>
        /// </summary>
        public ushort Port;

        //PAD 2

        public OfpQueueGetConfigRequest()
        { }

        public OfpQueueGetConfigRequest(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            br.Parse(out Port);
            br.ReadBytes(2);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Header.Length);
            w.Write(Header.ToByteArray());
            w.Write(Port);
            w.Pad(2);
            return w.ToByteArray();
        }

    }

    /// <summary>
    /// 交换机回复队列配置
    /// </summary>
    public class OfpQueueGetConfig : IOfpMessage
    {
        public const uint Size = 16;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_QUEUE_GET_CONFIG_REPLY };

        public ushort Port;

        //PAD 6

        /// <summary>
        /// 已配置的队列的列表
        /// </summary>
        public List<OfpPacketQueue> Queues;

        public OfpQueueGetConfig()
        { }

        public OfpQueueGetConfig(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            br.Parse(out Port);
            br.ReadBytes(6); //PAD 6
            var count = (Header.Length - Size) / OfpPacketQueue.Size;
            for (int i = 0; i < count; i++)
            {
                Queues.Add(new OfpPacketQueue(stream));
            }
        }

        public uint UpdateLength()
        {
            var length = Size;
            length += (uint)(Queues.Count * OfpPacketQueue.Size);
            Header.Length = (ushort)length; //TODO: length check
            return length;
        }

        public byte[] ToByteArray()
        {
            UpdateLength();
            Writer w = new Writer();
            w.Write(Header.ToByteArray());
            w.Write(Port);
            w.Pad(6);
            foreach (var queue in Queues)
            {
                w.Write(queue.ToByteArray());
            }
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 状态请求信息
    /// </summary>
    public class OfpStatsRequest : IOfpMessage
    {
        public const uint Size = 12;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_STATS_REQUEST };

        /// <summary>
        /// 消息类型，用于决定Body中的信息如何解析，OFPST_之一
        /// </summary>
        public OfpStatsTypes Type = OfpStatsTypes.OFPST_DESC;

        /// <summary>
        /// OFPSF_REQ_* flags (none yet defined).
        /// </summary>
        public ushort Flags = 0;

        /// <summary>
        /// 具体请求内容
        /// </summary>
        public byte[] Body;

        public OfpStatsRequest()
        { }

        public OfpStatsRequest(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            br.Parse(out Type);
            br.Parse(out Flags);
            br.Parse(out Body, (int)(Header.Length - Size));
        }

        public uint UpdateLength()
        {
            var length = Size;
            length += (uint)Body.Length;
            Header.Length = (ushort)length;
            return length;
        }

        public byte[] ToByteArray()
        {
            UpdateLength();
            Writer w = new Writer();
            w.Write(Header.ToByteArray());
            w.Write(Type);
            w.Write(Flags);
            w.Write(Body);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 状态回复信息
    /// </summary>
    public class OfpStatsReply : IOfpMessage
    {
        public const uint Size = 12;
        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_STATS_REPLY };

        /// <summary>
        /// 消息类型，用于决定Body中的信息如何解析，OFPST_之一
        /// </summary>
        public OfpStatsTypes Type = OfpStatsTypes.OFPST_DESC;

        /// <summary>
        /// 表明是否有多条回复
        /// </summary>
        public OfpStatsFlagsReply Flags = 0;

        /// <summary>
        /// 回复内容
        /// </summary>
        public byte[] Body;

        public OfpStatsReply()
        { }

        public OfpStatsReply(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            br.Parse(out Type);
            br.Parse(out Flags);
            br.Parse(out Body, (int)(Header.Length - Size));
        }

        public uint UpdateLength()
        {
            var length = Size;
            length += (uint)Body.Length;
            Header.Length = (ushort)length;
            return length;
        }

        public byte[] ToByteArray()
        {
            UpdateLength();
            Writer w = new Writer();
            w.Write(Header.ToByteArray());
            w.Write(Type);
            w.Write(Flags);
            w.Write(Body);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 整体信息
    /// </summary>
    public class OfpDescStats : IToByteArray
    {
        public const uint Size = 1056;
        /// <summary>
        /// 制造商描述
        /// </summary>
        public string MfrDesc;
        /// <summary>
        /// 硬件描述
        /// </summary>
        public string HwDesc;
        /// <summary>
        /// 软件描述
        /// </summary>
        public string SwDesc;
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNum;
        /// <summary>
        /// Datapath的可读描述
        /// </summary>
        public string DpDesc;

        public OfpDescStats()
        { }

        public OfpDescStats(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out MfrDesc, DESC_STR_LEN);
            br.Parse(out HwDesc, DESC_STR_LEN);
            br.Parse(out SwDesc, DESC_STR_LEN);
            br.Parse(out SerialNum, SERIAL_NUM_LEN);
            br.Parse(out DpDesc, DESC_STR_LEN);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(MfrDesc, DESC_STR_LEN);
            w.Write(HwDesc, DESC_STR_LEN);
            w.Write(SwDesc, DESC_STR_LEN);
            w.Write(SerialNum, SERIAL_NUM_LEN);
            w.Write(DpDesc, DESC_STR_LEN);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 对于OFPST_FLOW类型的OfpStatsRequest请求
    /// </summary>
    public class OfpFlowStatsRequest : IToByteArray
    {
        public const uint Size = 44;
        /// <summary>
        /// 要匹配的域
        /// </summary>
        public OfpMatch Match;

        /// <summary>
        /// 要读取的流表ID（from OfpTableStats）
        /// <remarks>OxFF代指所有表，0xFE代指Emergency表</remarks>
        /// </summary>
        public byte TableId;

        //PAD 1

        /// <summary>
        /// 要求匹配项包含此出端口
        /// <remarks>OFPP_NONE值代表无此限制</remarks>
        /// </summary>
        public ushort OutPort;

        public OfpFlowStatsRequest()
        { }

        public OfpFlowStatsRequest(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Match = new OfpMatch(stream);
            br.Parse(out TableId);
            br.ReadBytes(1);
            br.Parse(out OutPort);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Match.ToByteArray());
            w.Write(TableId);
            w.Pad(1);
            w.Write(OutPort);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 对于OFPST_FLOW类型的回复体，流表项状态信息
    /// </summary>
    public class OfpFlowStats : IToByteArray
    {
        public const uint Size = 88;
        /// <summary>
        /// 长度
        /// </summary>
        public ushort Length;

        /// <summary>
        /// 表ID
        /// </summary>
        public byte TableId;

        //PAD 1

        public OfpMatch Match;

        /// <summary>
        /// 生存时间（秒部分）
        /// </summary>
        public uint DurationSec;

        /// <summary>
        /// 生存时间（纳秒部分），要求至少提供ms精度
        /// <remarks>The total duration in nanoseconds can be computed as duration_sec * 10^9 + duration_nsec.</remarks>
        /// </summary>
        public uint DurationNsec;

        /// <summary>
        /// 优先级
        /// </summary>
        public ushort Priority;

        /// <summary>
        /// 空闲超时
        /// </summary>
        public ushort IdleTimeout;

        /// <summary>
        /// 强制超时
        /// <remarks>0表示永不过期</remarks>
        /// </summary>
        public ushort HardTimeout;

        //PAD 6

        /// <summary>
        /// Opaque Controller-issued identifier
        /// </summary>
        public ulong Cookie;

        /// <summary>
        /// 包计数
        /// </summary>
        public ulong PacketCount;

        /// <summary>
        /// 流量计数
        /// </summary>
        public ulong ByteCount;

        /// <summary>
        /// 动作
        /// </summary>
        //public Dictionary<OfpActionType, IOfpAction> Actions = new Dictionary<OfpActionType, IOfpAction>();
        public ActionList Actions = new ActionList();

        public OfpFlowStats()
        { }

        public OfpFlowStats(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Length);
            br.Parse(out TableId);
            br.ReadBytes(1);
            Match = new OfpMatch(stream);
            br.Parse(out DurationSec);
            br.Parse(out DurationNsec);
            br.Parse(out Priority);
            br.Parse(out IdleTimeout);
            br.Parse(out HardTimeout);
            br.ReadBytes(6);
            br.Parse(out Cookie);
            br.Parse(out PacketCount);
            br.Parse(out ByteCount);

            IOfpAction action = OfpActionHeader.ParseAction(stream);
            while (action != null)
            {
                Actions[action.Header.Type] = action;
                action = OfpActionHeader.ParseAction(stream);
            }
        }

        /// <summary>
        /// 计算长度
        /// </summary>
        public uint UpdateLength()
        {
            uint len = Size + (uint)Actions.Length;
            Length = (ushort)len;
            return len;
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer();
            UpdateLength();
            w.Write(Length);
            w.Write(TableId);
            w.Pad(1);
            w.Write(Match.ToByteArray());
            w.Write(DurationSec);
            w.Write(DurationNsec);
            w.Write(Priority);
            w.Write(IdleTimeout);
            w.Write(HardTimeout);
            w.Pad(6);
            w.Write(Cookie);
            w.Write(PacketCount);
            w.Write(ByteCount);

            w.Write(Actions.ToByteArray());
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 多流请求信息
    /// </summary>
    public class OfpAggregateStatsRequest : IToByteArray
    {
        public const uint Size = 44;

        /// <summary>
        /// 匹配域
        /// </summary>
        public OfpMatch Match;
        
        /// <summary>
        /// 表ID
        /// </summary>
        public byte TableId;

        //PAD 1

        /// <summary>
        /// 出端口
        /// </summary>
        public ushort OutPort;

        public OfpAggregateStatsRequest()
        { }

        public OfpAggregateStatsRequest(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Match = new OfpMatch(stream);
            br.Parse(out TableId);
            br.ReadBytes(1);
            br.Parse(out OutPort);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(Match.ToByteArray());
            w.Write(TableId);
            w.Pad(1);
            w.Write(OutPort);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 多流回复消息 for OFPST_AGGREGATE
    /// </summary>
    public class OfpAggregateStats : IToByteArray
    {
        public const uint Size = 24;
        /// <summary>
        /// 流中的包数
        /// </summary>
        public ulong PacketCount;

        /// <summary>
        /// 流中的字节数
        /// </summary>
        public ulong ByteCount;

        /// <summary>
        /// 流数
        /// </summary>
        public uint FlowCount;

        //PAD 4

        public OfpAggregateStats()
        { }

        public OfpAggregateStats(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out PacketCount);
            br.Parse(out ByteCount);
            br.Parse(out FlowCount);
            br.ReadBytes(4);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(PacketCount);
            w.Write(ByteCount);
            w.Write(FlowCount);
            w.Pad(4);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 表状态请求信息
    /// </summary>
    public class OfpTableStatsRequest : OfpStatsRequest
    {
        public OfpTableStatsRequest()
        {
            Type = OfpStatsTypes.OFPST_TABLE;
        }
    }

    /// <summary>
    /// 表状态回复信息 for OFPST_TABLE
    /// <remarks>表统计请求只有消息头，无消息体</remarks>
    /// </summary>
    public class OfpTableStats : IToByteArray
    {
        public const uint Size = 64;

        /// <summary>
        /// 表ID
        /// </summary>
        public byte TableId;

        //PAD 3

        /// <summary>
        /// 表名
        /// </summary>
        public string Name;

        /// <summary>
        /// 表支持的通配符
        /// </summary>
        public OfpWildcards Wildcards;

        /// <summary>
        /// 支持的最大流表项数
        /// </summary>
        public uint MaxEntries;

        /// <summary>
        /// 有效流表项数
        /// </summary>
        public uint ActiveCount;

        /// <summary>
        /// 在表中查询过的包数
        /// </summary>
        public ulong LookupCount;

        /// <summary>
        /// 在表中匹配的包数
        /// </summary>
        public ulong MatchedCount;

        public OfpTableStats()
        { }

        public OfpTableStats(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out TableId);
            br.ReadBytes(3); //PAD 3
            br.Parse(out Name, OFP_MAX_TABLE_NAME_LEN);
            Wildcards = new OfpWildcards(stream);
            br.Parse(out MaxEntries);
            br.Parse(out ActiveCount);
            br.Parse(out LookupCount);
            br.Parse(out MatchedCount);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(TableId);
            w.Pad(3);
            w.Write(Name, OFP_MAX_TABLE_NAME_LEN);
            w.Write(Wildcards);
            w.Write(MaxEntries);
            w.Write(ActiveCount);
            w.Write(LookupCount);
            w.Write(MatchedCount);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 端口统计请求信息 for OFPST_PORT
    /// </summary>
    public class OfpPortStatsRequest : IToByteArray
    {
        public const uint Size = 8;

        /// <summary>
        /// 一个单一端口或者所有端口（OFPP_NONE）
        /// </summary>
        public ushort PortNo;

        //PAD 6

        public OfpPortStatsRequest()
        { }

        public OfpPortStatsRequest(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out PortNo);
            br.ReadBytes(6);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(PortNo);
            w.Pad(6);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 端口统计回复信息 for OFPST_PORT
    /// <remarks>对于不可用的计数器，返回-1</remarks>
    /// </summary>
    public class OfpPortStats : IToByteArray
    {
        public const uint Size = 104;

        /// <summary>
        /// 端口
        /// </summary>
        public ushort PortNo;

        //PAD 6

        /// <summary>
        /// 接收到的包数
        /// </summary>
        public ulong RxPackets;

        /// <summary>
        /// 传输的包数
        /// </summary>
        public ulong TxPackets;

        /// <summary>
        /// 接收到的字节数
        /// </summary>
        public ulong RxBytes;

        /// <summary>
        /// 传输的包数
        /// </summary>
        public ulong TxBytes;

        /// <summary>
        /// RX中丢弃的包数
        /// </summary>
        public ulong RxDropped;

        /// <summary>
        /// TX中丢弃的包数
        /// </summary>
        public ulong TxDropped;

        /// <summary>
        /// 接收的错误总数
        /// </summary>
        public ulong RxErrors;

        /// <summary>
        /// 传输的错误总数
        /// </summary>
        public ulong TxErrors;

        /// <summary>
        /// 帧对齐错误数
        /// </summary>
        public ulong RxFrameErr;

        /// <summary>
        /// RX超限包数
        /// </summary>
        public ulong RxOverErr;

        /// <summary>
        /// CRC错误数
        /// </summary>
        public ulong RxCrcErr;

        /// <summary>
        /// 冲突数
        /// </summary>
        public ulong Collisions;

        public OfpPortStats()
        { }

        public OfpPortStats(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out PortNo);
            br.ReadBytes(6);
            br.Parse(out RxPackets);
            br.Parse(out TxPackets);
            br.Parse(out RxBytes);
            br.Parse(out TxBytes);
            br.Parse(out RxDropped);
            br.Parse(out TxDropped);
            br.Parse(out RxErrors);
            br.Parse(out TxErrors);
            br.Parse(out RxFrameErr);
            br.Parse(out RxOverErr);
            br.Parse(out RxCrcErr);
            br.Parse(out Collisions);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(PortNo);
            w.Pad(6);
            w.Write(RxPackets);
            w.Write(TxPackets);
            w.Write(RxBytes);
            w.Write(TxBytes);
            w.Write(RxDropped);
            w.Write(TxDropped);
            w.Write(RxErrors);
            w.Write(TxErrors);
            w.Write(RxFrameErr);
            w.Write(RxOverErr);
            w.Write(RxCrcErr);
            w.Write(Collisions);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 队列请求信息 for OFPST_QUEUE
    /// </summary>
    public class OfpQueueStatsRequest : IToByteArray
    {
        public const uint Size = 8;

        /// <summary>
        /// 端口
        /// <remarks>OFPT_ALL表示所有端口</remarks>
        /// </summary>
        public ushort PortNo;

        //PAD 2

        /// <summary>
        /// 队列
        /// <remarks>OFPQ_ALL表示所有队列</remarks>
        /// </summary>
        public uint QueueId;

        public OfpQueueStatsRequest()
        { }

        public OfpQueueStatsRequest(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out PortNo);
            br.ReadBytes(2);
            br.Parse(out QueueId);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(PortNo);
            w.Pad(2);
            w.Write(QueueId);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 队列回复信息 for OFPST_QUEUE
    /// </summary>
    public class OfpQueueStats : IToByteArray
    {
        public const uint Size = 32;

        /// <summary>
        /// 端口号
        /// </summary>
        public ushort PortNo;

        //PAD 2

        /// <summary>
        /// 队列ID
        /// </summary>
        public uint QueueId;

        /// <summary>
        /// 传输的字节数
        /// </summary>
        public ulong TxBytes;

        /// <summary>
        /// 传输的包数
        /// </summary>
        public ulong TxPackets;

        /// <summary>
        /// 由于超限而丢弃的包数
        /// </summary>
        public ulong TxErrors;

        public OfpQueueStats()
        { }

        public OfpQueueStats(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out PortNo);
            br.ReadBytes(2); //PAD 2
            br.Parse(out QueueId);
            br.Parse(out TxBytes);
            br.Parse(out TxPackets);
            br.Parse(out TxErrors);
        }


        public byte[] ToByteArray()
        {
            Writer w = new Writer(Size);
            w.Write(PortNo);
            w.Pad(2);
            w.Write(QueueId);
            w.Write(TxBytes);
            w.Write(TxPackets);
            w.Write(TxErrors);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 生产商信息
    /// </summary>
    public class OfpVendorStats : IToByteArray
    {
        public byte[] Vendor;

        public byte[] Content;

        public OfpVendorStats()
        { }

        public OfpVendorStats(Stream stream, int length)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            br.Parse(out Vendor, 4);
            br.Parse(out Content, length);
        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer();
            w.Write(Vendor);
            w.Write(Content);
            return w.ToByteArray();
        }
    }

    /// <summary>
    /// 发包
    /// </summary>
    public class OfpPacketOut : IOfpMessage
    {
        public const uint Size = 16;

        public OfpHeader Header { get; private set; } = new OfpHeader()
        { Type = OfpType.OFPT_PACKET_OUT };

        /// <summary>
        /// Datapath ID （若无则为-1）
        /// </summary>
        public uint BufferId;

        /// <summary>
        /// 包的入端口（若无则为OFPP_NONE）
        /// </summary>
        public ushort InPort;

        /// <summary>
        /// 动作数组的字节长度
        /// </summary>
        public ushort ActionsLen;

        /// <summary>
        /// 动作
        /// <remarks>如果Output的出端口为OFPP_TABLE，则<see cref="InPort"/>将被用于流表的查询</remarks>
        /// </summary>
        public ActionList Actions;

        /// <summary>
        /// 数据包内容
        /// <remarks>长度由包头长度推断。仅当<see cref="BufferId"/>为-1时有意义</remarks>
        /// </summary>
        public byte[] Data;

        public OfpPacketOut()
        { }

        public OfpPacketOut(Stream stream, OfpHeader header = null)
        {
            BeBinaryReader br = new BeBinaryReader(stream, Encoding.ASCII, true);
            Header = header ?? new OfpHeader(stream);
            br.Parse(out BufferId);
            br.Parse(out InPort);
            br.Parse(out ActionsLen);

            int length = ActionsLen;
            while (length >= 8)
            {
                var action = OfpActionHeader.ParseAction(stream);
                Actions[action.Header.Type] = action;
                length -= action.Header.Len;
            }

            if (BufferId == uint.MaxValue) //-1
            {
                length = Header.Length - (int)Size - ActionsLen;
                if (length > 0)
                {
                    br.Parse(out Data, length);
                }
            }
        }

        public uint UpdateLength()
        {
            ActionsLen = (ushort)Actions.Length;
            var dataLen = Data?.Length ?? 0;
            Header.Length = (ushort)(Size + ActionsLen + dataLen);
            return Header.Length;
        }

        public byte[] ToByteArray()
        {
            UpdateLength();
            Writer w = new Writer();
            w.Write(Header.ToByteArray());
            w.Write(BufferId);
            w.Write(InPort);
            w.Write(ActionsLen);
            w.Write(Actions.ToByteArray());
            w.Write(Data);
            return w.ToByteArray();
        }

    }

    //Barrier保障消息
    //OFPT_BARRIER_REQUEST消息没有消息体
    //交换机收到此消息后，需先执行完该消息到达时之前的所有指令，然后回复OFPT_BARRIER_REPLY
    //OFPT_BARRIER_REPLY消息携带原请求信息的XID（直接在头中？）
    /// <summary>
    /// Barrier保障消息
    /// </summary>
    public class OfpBarrrier : IOfpMessage
    {
        public OfpHeader Header { get; } = new OfpHeader()
        { Type = OfpType.OFPT_BARRIER_REQUEST, Length = 8 };


        public OfpBarrrier(bool isReply = false)
        {
            Header.Type = isReply ? OfpType.OFPT_BARRIER_REPLY : OfpType.OFPT_BARRIER_REQUEST;
        }

        public byte[] ToByteArray()
        {
            return Header.ToByteArray();
        }

    }
}
