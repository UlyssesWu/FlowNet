using System;

namespace FlowNet.OpenFlow.OFP1_0
{
    public class Data
    {
        /// <summary>
        /// OF协议版本
        /// </summary>
        public const byte OFP_VERSION = 0x01;

        /// <summary>
        /// 描述字符串长度
        /// </summary>
        public const int DESC_STR_LEN = 256;

        /// <summary>
        /// 序列号长度
        /// </summary>
        public const int SERIAL_NUM_LEN = 32;

        /// <summary>
        /// 表名长度
        /// </summary>
        public const int OFP_MAX_TABLE_NAME_LEN = 32;

        /// <summary>
        /// MAC地址长度
        /// </summary>
        public const int OFP_MAX_ETH_ALEN = 6;

        /// <summary>
        /// 端口名长度
        /// </summary>
        public const int OFP_MAX_PORT_NAME_LEN = 16;

        //Ch.2.5.1
        /// <summary>
        /// Message Type
        /// </summary>
        public enum OfpType : byte
        {
            //Immutable messages

            /// <summary>
            /// Symmetric message
            /// </summary>
            OFPT_HELLO,
            /// <summary>
            /// Symmetric message
            /// </summary>
            OFPT_ERROR,
            /// <summary>
            /// Symmetric message
            /// </summary>
            OFPT_ECHO_REQUEST,
            /// <summary>
            /// Symmetric message
            /// </summary>
            OFPT_ECHO_REPLY,
            /// <summary>
            /// Symmetric message
            /// </summary>
            OFPT_VENDOR,

            //Switch configuration messages
            /// <summary>
            /// Controller/Switch message
            /// </summary>
            OFPT_FEATURES_REQUEST,
            /// <summary>
            /// Controller/Switch message
            /// </summary>
            OFPT_FEATURES_REPLY,
            /// <summary>
            /// Controller/Switch message
            /// </summary>
            OFPT_GET_CONFIG_REQUEST,
            /// <summary>
            /// Controller/Switch message
            /// </summary>
            OFPT_GET_CONFIG_REPLY,
            /// <summary>
            /// Controller/Switch message
            /// </summary>
            OFPT_SET_CONFIG,

            //Asynchronous messages
            OFPT_PACKET_IN,
            OFPT_FLOW_REMOVED,
            OFPT_PORT_STATUS,

            //Controller command messages
            OFPT_PACKET_OUT,
            OFPT_FLOW_MOD,
            OFPT_PORT_MOD,

            //Statistics messages
            OFPT_STATS_REQUEST,
            OFPT_STATS_REPLY,

            //Barrier messages
            OFPT_BARRIER_REQUEST,
            OFPT_BARRIER_REPLY,

            //Queue Configuration messages
            OFPT_QUEUE_GET_CONFIG_REQUEST,
            OFPT_QUEUE_GET_CONFIG_REPLY

        }

        //Ch.2.5.2
        //Ch.2.5.2.1 - Port



        [Flags]
        public enum OfpPortConfig : uint
        {
            /// <summary>
            /// Port is administratively down.
            /// </summary>
            OFPPC_PORT_DOWN = 1 << 0,
            /// <summary>
            /// Disable 802.1D spanning tree on port.
            /// </summary>
            OFPPC_NO_STP = 1 << 1,
            /// <summary>
            /// Drop all packets except 802.1D spanning tree packets.
            /// </summary>
            OFPPC_NO_RECV = 1 << 2,
            /// <summary>
            /// Drop received 802.1D STP packets.
            /// </summary>
            OFPPC_NO_RECV_STP = 1 << 3,
            /// <summary>
            /// Do not include this port when flooding.
            /// </summary>
            OFPPC_NO_FLOOD = 1 << 4,
            /// <summary>
            /// Drop packets forwarded to port.
            /// </summary>
            OFPPC_NO_FWD = 1 << 5,
            /// <summary>
            /// Do not send packet-in msgs for port.
            /// </summary>
            OFPPC_NO_PACKET_IN = 1 << 6
        }

        [Flags]
        public enum OfpPortState : uint
        {
            /// <summary>
            /// No physical link present.
            /// </summary>
            OFPPS_LINK_DOWN = 1 << 0,

            /* The OFPPS_STP_* bits have no effect on switch operation.  The
            * controller must adjust OFPPC_NO_RECV, OFPPC_NO_FWD, and
            * OFPPC_NO_PACKET_IN appropriately to fully implement an 802.1D spanning
            * tree. */

            /// <summary>
            /// Not learning or relaying frames.
            /// </summary>
            OFPPS_STP_LISTEN = 0 << 8,
            /// <summary>
            /// Learning but not relaying frames.
            /// </summary>
            OFPPS_STP_LEARN = 1 << 8,
            /// <summary>
            /// Learning and relaying frames.
            /// </summary>
            OFPPS_STP_FORWARD = 2 << 8,
            /// <summary>
            /// Not part of spanning tree.
            /// </summary>
            OFPPS_STP_BLOCK = 3 << 8,
            /// <summary>
            /// Bit mask for OFPPS_STP_* values.
            /// </summary>
            OFPPS_STP_MASK = 3 << 8
        }

        /// <summary>
        /// Port numbering.  Physical ports are numbered starting from 1.
        /// <remarks>不应将端口直接定义成此枚举，否则无法支持其他物理端口</remarks>
        /// </summary>
        public enum OfpPort : ushort //UInt16
        {
            /// <summary>
            /// Maximum number of physical switch ports.
            /// </summary>
            OFPP_MAX = 0xff00,
            /// <summary>
            /// Send the packet out the input port.  This virtual port must be explicitly used in order to send back out of the input port.
            /// </summary>
            OFPP_IN_PORT = 0xfff8,
            /// <summary>
            /// Perform actions in flow table. NB: This can only be the destination port for packet-out messages.
            /// </summary>
            OFPP_TABLE = 0xfff9,
            /// <summary>
            /// Process with normal L2/L3 switching.
            /// </summary>
            OFPP_NORMAL = 0xfffa,
            /// <summary>
            /// All physical ports except input port and those disabled by STP.
            /// </summary>
            OFPP_FLOOD = 0xfffb,
            /// <summary>
            /// All physical ports except input port.
            /// </summary>
            OFPP_ALL = 0xfffc,
            /// <summary>
            /// Send to controller.
            /// </summary>
            OFPP_CONTROLLER = 0xfffd,
            /// <summary>
            /// Local openflow "port".
            /// </summary>
            OFPP_LOCAL = 0xfffe,
            /// <summary>
            /// Not associated with a physical port.
            /// </summary>
            OFPP_NONE = 0xffff,

        }

        [Flags]
        public enum OfpPortFeatures : uint
        {
            /// <summary>
            /// 10 Mb half-duplex rate support.
            /// </summary>
            OFPPF_10MB_HD = 1 << 0,
            /// <summary>
            /// 10 Mb full-duplex rate support.
            /// </summary>
            OFPPF_10MB_FD = 1 << 1,
            /// <summary>
            /// 100 Mb half-duplex rate support.
            /// </summary>
            OFPPF_100MB_HD = 1 << 2,
            /// <summary>
            /// 100 Mb full-duplex rate support.
            /// </summary>
            OFPPF_100MB_FD = 1 << 3,
            /// <summary>
            /// 1 Gb half-duplex rate support.
            /// </summary>
            OFPPF_1GB_HD = 1 << 4,
            /// <summary>
            /// 1 Gb full-duplex rate support.
            /// </summary>
            OFPPF_1GB_FD = 1 << 5,
            /// <summary>
            /// 10 Gb full-duplex rate support.
            /// </summary>
            OFPPF_10GB_FD = 1 << 6,
            /// <summary>
            /// Copper medium.
            /// </summary>
            OFPPF_COPPER = 1 << 7,
            /// <summary>
            /// Fiber medium.
            /// </summary>
            OFPPF_FIBER = 1 << 8,
            /// <summary>
            /// Auto-negotiation.
            /// </summary>
            OFPPF_AUTONEG = 1 << 9,
            /// <summary>
            /// Pause.
            /// </summary>
            OFPPF_PAUSE = 1 << 10,
            /// <summary>
            /// Asymmetric pause.
            /// </summary>
            OFPPF_PAUSE_ASYM = 1 << 11
        }

        //Ch.2.5.2.2 - Queue
        public enum OfpQueueProperties : ushort
        {
            /// <summary>
            /// No property defined for queue (default).
            /// </summary>
            QFPQT_NONE = 0,
            /// <summary>
            /// Minimum datarate guaranteed.
            /// </summary>
            QFPQT_MIN_RATE,
            //Other types should be added here.
            //(i.e. max rate, precedence, etc.)
        }

        //Ch.2.5.2.3 - Flow Match
        /// <summary>
        /// 掩码位
        /// </summary>
        [Flags]
        public enum OfpFlowWildcards : uint
        {
            /// <summary>
            /// Switch input port.
            /// </summary>
            OFPFW_IN_PORT = 1 << 0,
            /// <summary>
            /// VLAN id.
            /// </summary>
            OFPFW_DL_VLAN = 1 << 1,
            /// <summary>
            /// Ethernet source address.
            /// </summary>
            OFPFW_DL_SRC = 1 << 2,
            /// <summary>
            /// Ethernet destination address.
            /// </summary>
            OFPFW_DL_DST = 1 << 3,
            /// <summary>
            /// Ethernet frame type.
            /// </summary>
            OFPFW_DL_TYPE = 1 << 4,
            /// <summary>
            /// IP protocol.
            /// </summary>
            OFPFW_NW_PROTO = 1 << 5,
            /// <summary>
            /// TCP/UDP source port.
            /// </summary>
            OFPFW_TP_SRC = 1 << 6,
            /// <summary>
            /// TCP/UDP destination port.
            /// </summary>
            OFPFW_TP_DST = 1 << 7,

            /// <summary>
            /// VLAN priority.
            /// </summary>
            OFPFW_DL_VLAN_PCP = 1 << 20,
            /// <summary>
            /// IP ToS (DSCP field, 6 bits).
            /// </summary>
            OFPFW_NW_TOS = 1 << 21,
            /// <summary>
            /// All
            /// </summary>
            OFPFW_ALL = ((1 << 22) - 1)
        }

        /* IP source address wildcard bit count.  0 is exact match, 1 ignores the
           * LSB, 2 ignores the 2 least-significant bits, ..., 32 and higher wildcard
           * the entire field.  This is the *opposite* of the usual convention where
           * e.g. /24 indicates that 8 bits (not 24 bits) are wildcarded. */

        public const int OFPFW_NW_SRC_SHIFT = 8;
        public const int OFPFW_NW_SRC_BITS = 6;

        /// <summary>
        /// ((1 &lt;&lt; 6) - 1) &lt;&lt; 8
        /// </summary>
        public const uint OFPFW_NW_SRC_MASK = ((1 << (int)OFPFW_NW_SRC_BITS) - 1) << (int)OFPFW_NW_SRC_SHIFT;

        /* IP destination address wildcard bit count.  Same format as source. */
        public const int OFPFW_NW_DST_SHIFT = 14;
        public const int OFPFW_NW_DST_BITS = 6;

        /// <summary>
        /// ((1 &lt;&lt; 6) - 1) &lt;&lt; 14
        /// </summary>
        public const uint OFPFW_NW_DST_MASK = ((1 << (int)OFPFW_NW_DST_BITS) - 1) << (int)OFPFW_NW_DST_SHIFT;

        public const uint OFPFW_NW_DST_ALL = 32 << (int)OFPFW_NW_DST_SHIFT;

        //Ch.2.5.2.4 Action
        public enum OfpActionType : ushort //UInt16
        {
            /// <summary>
            /// Output to switch port.
            /// </summary>
            OFPAT_OUTPUT = 0,
            /// <summary>
            /// Set the 802.1q VLAN id.
            /// <remarks>same as OFPAT_SET_DL_VLAN ?</remarks>
            /// </summary>
            OFPAT_SET_VLAN_VID = 1,
            /// <summary>
            /// Set the 802.1q priority.
            /// </summary>
            OFPAT_SET_VLAN_PCP = 2,
            /// <summary>
            /// Strip the 802.1q header.
            /// </summary>
            OFPAT_STRIP_VLAN = 3,
            /// <summary>
            /// Ethernet source address.
            /// </summary>
            OFPAT_SET_DL_SRC = 4,
            /// <summary>
            /// Ethernet destination address.
            /// </summary>
            OFPAT_SET_DL_DST = 5,
            /// <summary>
            /// IP source address.
            /// </summary>
            OFPAT_SET_NW_SRC = 6,
            /// <summary>
            /// IP destination address.
            /// </summary>
            OFPAT_SET_NW_DST = 7,
            /// <summary>
            /// IP ToS (DSCP field, 6 bits).
            /// </summary>
            OFPAT_SET_NW_TOS = 8,
            /// <summary>
            /// TCP/UDP source port.
            /// </summary>
            OFPAT_SET_TP_SRC = 9,
            /// <summary>
            /// TCP/UDP destination port.
            /// </summary>
            OFPAT_SET_TP_DST = 10,
            /// <summary>
            /// Output to queue.
            /// </summary>
            OFPAT_ENQUEUE = 11,
            /// <summary>
            /// Vendor
            /// </summary>
            OFPAT_VENDOR = 0xffff
        }

        /*
        The actions field is a bitmap of actions supported by the switch. The list of
    actions is found in Section~3.3; all actions marked Required must be supported.
    Vendor actions should not be reported via this bitmask. The bitmask uses the
    values from ofp_action_type as the number of bits to shift left for an associated
    action. For example, OFPAT_SET_DL_VLAN? would use the flag 0x00000002.
        */
        /// <summary>
        /// Bitmap of supported "ofp_action_type"s.
        /// <remarks>此类型并非OF协议中规定的，仅为编程方便而加入。</remarks>
        /// </summary>
        [Flags]
        public enum OfpActionCapabilities : uint //UInt16
        {
            /// <summary>
            /// Output to switch port.
            /// </summary>
            OFPAT_OUTPUT = 1 << 0,
            /// <summary>
            /// Set the 802.1q VLAN id.
            /// <remarks>same as OFPAT_SET_DL_VLAN ?</remarks>
            /// </summary>
            OFPAT_SET_VLAN_VID = 1 << 1,
            /// <summary>
            /// Set the 802.1q priority.
            /// </summary>
            OFPAT_SET_VLAN_PCP = 1 << 2,
            /// <summary>
            /// Strip the 802.1q header.
            /// </summary>
            OFPAT_STRIP_VLAN = 1 << 3,
            /// <summary>
            /// Ethernet source address.
            /// </summary>
            OFPAT_SET_DL_SRC = 1 << 4,
            /// <summary>
            /// Ethernet destination address.
            /// </summary>
            OFPAT_SET_DL_DST = 1 << 5,
            /// <summary>
            /// IP source address.
            /// </summary>
            OFPAT_SET_NW_SRC = 1 << 6,
            /// <summary>
            /// IP destination address.
            /// </summary>
            OFPAT_SET_NW_DST = 1 << 7,
            /// <summary>
            /// IP ToS (DSCP field, 6 bits).
            /// </summary>
            OFPAT_SET_NW_TOS = 1 << 8,
            /// <summary>
            /// TCP/UDP source port.
            /// </summary>
            OFPAT_SET_TP_SRC = 1 << 9,
            /// <summary>
            /// TCP/UDP destination port.
            /// </summary>
            OFPAT_SET_TP_DST = 1 << 10,
            /// <summary>
            /// Output to queue.
            /// </summary>
            OFPAT_ENQUEUE = 1 << 11,
            /// <summary>
            /// Vendor
            /// </summary>
            //OFPAT_VENDOR = 0xffff
        }

        //Ch.2.5.3 Controller-to-Switch message
        //Ch.2.5.3.1 Handshake
        /// <summary>
        /// Capabilities supported by the datapath.
        /// </summary>
        [Flags]
        public enum OfpCapabilities : uint
        {
            /// <summary>
            /// Flow statistics.
            /// </summary>
            OFPC_FLOW_STATS = 1 << 0,
            /// <summary>
            /// Table statistics.
            /// </summary>
            OFPC_TABLE_STATS = 1 << 1,
            /// <summary>
            /// Port statistics.
            /// </summary>
            OFPC_PORT_STATS = 1 << 2,
            /// <summary>
            /// 802.1d spanning tree.
            /// </summary>
            OFPC_STP = 1 << 3,
            /// <summary>
            /// Reserved, must be zero.
            /// </summary>
            OFPC_RESERVED = 1 << 4,
            /// <summary>
            /// Can reassemble IP fragments.
            /// </summary>
            OFPC_IP_REASM = 1 << 5,
            /// <summary>
            /// Queue statistics.
            /// </summary>
            OFPC_QUEUE_STATS = 1 << 6,
            /// <summary>
            /// Match IP addresses in ARP pkts.
            /// </summary>
            OFPC_ARP_MATCH_IP = 1 << 7
        }

        //Ch.2.5.3.2 Switch Config
        [Flags]
        public enum OfpConfigFlags : ushort
        {
            /* Handling of IP fragments. */
            /// <summary>
            /// No special handling for fragments.
            /// </summary>
            OFPC_FRAG_NORMAL = 0,
            /// <summary>
            /// Drop fragments.
            /// </summary>
            OFPC_FRAG_DROP = 1,
            /// <summary>
            /// Reassemble (only if OFPC_IP_REASM set). 
            /// </summary>
            OFPC_FRAG_REASM = 2,
            OFPC_FRAG_MASK = 3
        }

        public enum OfpFlowModCommand : ushort //UInt16
        {
            /// <summary>
            /// New flow
            /// </summary>
            OFPFC_ADD,
            /// <summary>
            /// Modify all matching flows
            /// </summary>
            OFPFC_MODIFY,
            /// <summary>
            /// Modify entry strictly matching wildcards
            /// </summary>
            OFPFC_MODIFY_STRICT,
            /// <summary>
            /// Delete all matching flows
            /// </summary>
            OFPFC_DELETE,
            /// <summary>
            /// Strictly match wildcards and priority
            /// </summary>
            OFPFC_DELETE_STRICT
        }

        public enum OfpFlowModFlags : ushort //UInt16
        {
            /// <summary>
            /// Send flow removed message when flow expires or is deleted.
            /// </summary>
            OFPFF_SEND_FLOW_REM = 1 << 0,
            /// <summary>
            /// Check for overlapping entries first.
            /// </summary>
            OFPFF_CHECK_OVERLAP = 1 << 1,
            /// <summary>
            /// Remark this is for emergency.
            /// </summary>
            OFPFF_EMERG = 1 << 2
        }

        //Ch.2.5.3.5 Stats Request
        //Ch.2.5.3.5.1
        public enum OfpStatsTypes : ushort //UInt16
        {
            /// <summary>
            /// Description of this OpenFlow switch.
            /// The request body is empty.
            /// The reply body is struct ofp_desc_stats.
            /// </summary>
            OFPST_DESC = 0,
            /// <summary>
            /// Individual flow statistics.
            /// The request body is struct ofp_flow_stats_request.
            /// The reply body is an array of struct ofp_flow_stats.
            /// </summary>
            OFPST_FLOW = 1,

            /// <summary>
            /// Aggregate flow statistics.
            /// The request body is struct ofp_aggregate_stats_request.
            /// The reply body is struct ofp_aggregate_stats_reply.
            /// </summary>
            OFPST_AGGREGATE = 2,


            /// <summary>
            /// Flow table statistics.
            /// The request body is empty.
            /// The reply body is an array of struct ofp_table_stats.
            /// </summary>
            OFPST_TABLE = 3,

            /// <summary>
            /// Physical port statistics.
            /// The request body is struct ofp_port_stats_request.
            /// The reply body is an array of struct ofp_port_stats.
            /// </summary>
            OFPST_PORT = 4,


            /// <summary>
            /// Queue statistics for a port.
            /// The request body defines the port.
            /// The reply body is an array of struct ofp_queue_stats.
            /// </summary>
            OFPST_QUEUE = 5,


            /// <summary>
            /// Vendor extension.
            /// The request and reply bodies begin with a 32-bit vendor ID, which takes
            /// the same form as in "struct ofp_vendor_header".  The request and reply
            /// bodies are otherwise vendor-defined.
            /// </summary>
            OFPST_VENDOR = 0xffff
        }

        /// <summary>
        /// <remarks>这不是OFP1.0的官方定义，仅为编程方便</remarks>
        /// </summary>
        [Flags]
        public enum OfpStatsFlagsReply : ushort
        {
            OFPSF_REPLY_MORE = 0x0001
        }

        //Ch.2.5.4 Asynchronous Message
        //Ch.2.5.4.1
        /// <summary>
        /// 1字节
        /// </summary>
        public enum OfpPacketInReason : byte //uint8_t
        {
            /// <summary>
            /// No matching flow.
            /// </summary>
            OFPR_NO_MATCH,
            /// <summary>
            /// Action explicitly output to controller.
            /// </summary>
            OFPR_ACTION
        }

        public enum OfpFlowRemovedReason : byte //uint8_t
        {
            /// <summary>
            /// Flow idle time exceeded idle_timeout.
            /// </summary>
            OFPRR_IDLE_TIMEOUT,
            /// <summary>
            /// Time exceeded hard_timeout.
            /// </summary>
            OFPRR_HARD_TIMEOUT,
            /// <summary>
            /// Evicted by a DELETE flow mod.
            /// </summary>
            OFPRR_DELETE
        }

        public enum OfpPortReason : byte //uint8_t
        {
            /// <summary>
            /// The port was added.
            /// </summary>
            OFPPR_ADD,
            /// <summary>
            /// The port was removed.
            /// </summary>
            OFPPR_DELETE,
            /// <summary>
            /// Some attribute of the port has changed.
            /// </summary>
            OFPPR_MODIFY
        }

        /// <summary>
        /// Values for 'type' in ofp_error_message. 
        /// <para>These values are immutable: they will not change in future versions of the protocol(although new values may be added).</para>
        /// </summary>
        public enum OfpErrorType : ushort //uint16_t
        {
            /// <summary>
            /// Hello protocol failed.
            /// </summary>
            OFPET_HELLO_FAILED,
            /// <summary>
            /// Request was not understood.
            /// </summary>
            OFPET_BAD_REQUEST,
            /// <summary>
            /// Error in action description.
            /// </summary>
            OFPET_BAD_ACTION,
            /// <summary>
            /// Problem modifying flow entry.
            /// </summary>
            OFPET_FLOW_MOD_FAILED,
            /// <summary>
            /// Port mod request failed.
            /// </summary>
            OFPET_PORT_MOD_FAILED,
            /// <summary>
            /// Queue operation failed.
            /// </summary>
            OFPET_QUEUE_OP_FAILED
        }

        /// <summary>
        /// ofp_error_msg 'code' values for OFPET_HELLO_FAILED. 'data' contains an ASCII text string that may give failure details.
        /// </summary>
        public enum OfpHelloFailedCode : ushort //uint16_t
        {
            /// <summary>
            /// No compatible version.
            /// </summary>
            OFPHFC_INCOMPATIBLE,
            /// <summary>
            /// Permissions error.
            /// </summary>
            OFPHFC_EPERM
        }

        /// <summary>
        /// ofp_error_msg 'code' values for OFPET_BAD_REQUEST. 'data' contains at least the first 64 bytes of the failed request.
        /// </summary>
        public enum OfpBadRequestCode : ushort //uint16_t
        {
            /// <summary>
            /// ofp_header.version not supported.
            /// </summary>
            OFPBRC_BAD_VERSION,
            /// <summary>
            /// ofp_header.type not supported.
            /// </summary>
            OFPBRC_BAD_TYPE,
            /// <summary>
            /// ofp_stats_request.type not supported.
            /// </summary>
            OFPBRC_BAD_STAT,
            /// <summary>
            /// Vendor not supported (in ofp_vendor_header or ofp_stats_request or ofp_stats_reply).
            /// </summary>
            OFPBRC_BAD_VENDOR,
            /// <summary>
            /// Vendor subtype not supported.
            /// </summary>
            OFPBRC_BAD_SUBTYPE,
            /// <summary>
            /// Permissions error.
            /// </summary>
            OFPBRC_EPERM,
            /// <summary>
            /// Wrong request length for type.
            /// </summary>
            OFPBRC_BAD_LEN,
            /// <summary>
            /// Specified buffer has already been used.
            /// </summary>
            OFPBRC_BUFFER_EMPTY,
            /// <summary>
            /// Specified buffer does not exist.
            /// </summary>
            OFPBRC_BUFFER_UNKNOWN
        }

        /// <summary>
        /// ofp_error_msg ’code’ values for OFPET_BAD_ACTION. ’data’ contains at least the first 64 bytes of the failed request.
        /// </summary>
        public enum OfpBadActionCode : ushort //uint16_t
        {
            /// <summary>
            /// Unknown action type.
            /// </summary>
            OFPBAC_BAD_TYPE,
            /// <summary>
            /// Length problem in actions.
            /// </summary>
            OFPBAC_BAD_LEN,
            /// <summary>
            /// Unknown vendor id specified.
            /// </summary>
            OFPBAC_BAD_VENDOR,
            /// <summary>
            /// Unknown action type for vendor id.
            /// </summary>
            OFPBAC_BAD_VENDOR_TYPE,
            /// <summary>
            /// Problem validating output action.
            /// </summary>
            OFPBAC_BAD_OUT_PORT,
            /// <summary>
            /// Bad action argument.
            /// </summary>
            OFPBAC_BAD_ARGUMENT,
            /// <summary>
            /// Permissions error. 
            /// </summary>
            OFPBAC_EPERM,
            /// <summary>
            /// Can't handle this many actions.
            /// </summary>
            OFPBAC_TOO_MANY,
            /// <summary>
            /// Problem validating output queue.
            /// </summary>
            OFPBAC_BAD_QUEUE
        }

        /// <summary>
        /// ofp_error_msg ’code’ values for OFPET_FLOW_MOD_FAILED. ’data’ contains at least the first 64 bytes of the failed request.
        /// </summary>
        public enum OfpFlowModFailedCode : ushort //uint16_t
        {
            /// <summary>
            /// Flow not added because of full tables.
            /// </summary>
            OFPFMFC_ALL_TABLES_FULL,
            /// <summary>
            /// Attempted to add overlapping flow with CHECK_OVERLAP flag set.
            /// </summary>
            OFPFMFC_OVERLAP,
            /// <summary>
            /// Permissions error.
            /// </summary>
            OFPFMFC_EPERM,
            /// <summary>
            /// Flow not added because of non‐zero idle/hard timeout.
            /// </summary>
            OFPFMFC_BAD_EMERG_TIMEOUT,
            /// <summary>
            /// Unknown command.
            /// </summary>
            OFPFMFC_BAD_COMMAND,
            /// <summary>
            /// Unsupported action list ‐ cannot process in the order specified.
            /// </summary>
            OFPFMFC_UNSUPPORTED
        }

        /// <summary>
        /// ofp_error_msg ’code’ values for OFPET_PORT_MOD_FAILED. ’data’ contains at least the first 64 bytes of the failed request.
        /// </summary>
        public enum OfpPortModFailedCode : ushort //uint16_t
        {
            /// <summary>
            /// Specified port does not exist.
            /// </summary>
            OFPPMFC_BAD_PORT,
            /// <summary>
            /// Specified hardware address is wrong.
            /// </summary>
            OFPPMFC_BAD_HW_ADDR,
        }

        /// <summary>
        /// ofp_error msg ’code’ values for OFPET_QUEUE_OP_FAILED. ’data’ contains at least the first 64 bytes of the failed request
        /// </summary>
        public enum OfpQueueOpFailedCode : ushort //uint16_t
        {
            /// <summary>
            /// Invalid port (or port does not exist).
            /// </summary>
            OFPQOFC_BAD_PORT,
            /// <summary>
            /// Queue does not exist.
            /// </summary>
            OFPQOFC_BAD_QUEUE,
            /// <summary>
            /// Permissions error.
            /// </summary>
            OFPQOFC_EPERM
        }

        //2.5.5 Symmetric Message

    }
}
