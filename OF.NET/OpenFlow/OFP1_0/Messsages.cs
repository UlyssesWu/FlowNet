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
    /// 消息
    /// </summary>
    public interface IOfpMessage
    {
        /// <summary>
        /// OF头
        /// </summary>
        OfpHeader Header { get; }
    }

    /// <summary>
    /// 交换机功能请求消息
    /// <remarks>控制器发送仅有消息头的OFPT_FEATURES_REQUEST消息，交换机返回OFPT_FEATURES_REPLY消息</remarks>
    /// </summary>
    public class OfpSwitchFeatures : IOfpMessage, IToByteArray
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

        public OfpSwitchFeatures(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream,Encoding.ASCII,true);
            Header = new OfpHeader(stream);
            br.Parse(out DatapathId);
            br.Parse(out NBuffers);
            br.Parse(out NTables);
            br.ReadBytes(3); //PAD 3
            br.Parse(out Capabilities);
            br.Parse(out Actions);
            var portCount = (Header.Length - Size)/OfpPhyPort.Size;
            for (int i = 0; i < portCount; i++)
            {
                OfpPhyPort port = new OfpPhyPort(stream);
                Ports.Add(port);
            }

        }

        public byte[] ToByteArray()
        {
            Writer w = new Writer();
            Header.Length = (ushort) (Size + Ports.Count*OfpPhyPort.Size); //TODO: Size check
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
    //The OFPT_SET_CONFIG and OFPT_GET_CONFIG_REPLY use the following:

    /// <summary>
    /// 交换机配置消息
    /// <remarks>控制器发送OFPT_GET_CONFIG_REQUEST（仅有头部）来查询配置，发送OFPT_SET_CONFIG修改配置。对于查询，交换机返回OFPT_GET_CONFIG_REPLY</remarks>
    /// </summary>
    public class OfpSwitchConfig :IOfpMessage,IToByteArray
    {
        public OfpHeader Header { get; private set; } = new OfpHeader()
        {Type = OfpType.OFPT_GET_CONFIG_REPLY, Length = 12};

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

        public OfpSwitchConfig(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true);
            Header = new OfpHeader(stream);
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
    public class OfpFlowMod : IOfpMessage,IToByteArray
    {
        /// <summary>
        /// 不包括Actions在内的长度
        /// </summary>
        public const uint Size = 72;

        public OfpHeader Header { get; private set; } = new OfpHeader() {Type = OfpType.OFPT_FLOW_MOD};

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
        /// </summary>
        public ushort HardTimeout;

        /// <summary>
        /// 优先级
        /// </summary>
        public ushort Priority;

        public uint BufferId;

        public ushort OutPort;

        public OfpFlowModFlags Flags;

        /// <summary>
        /// 动作
        /// <remarks>长度由头部推断</remarks>
        /// </summary>
        public List<OfpActionHeader> Actions = new List<OfpActionHeader>(); 

        public byte[] ToByteArray()
        {
            throw new NotImplementedException();
        }
    }
}
