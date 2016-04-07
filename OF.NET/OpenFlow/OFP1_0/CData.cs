using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OFNet.OpenFlow.OFP1_0
{
    public class Data
    {
        //Ch.2.5.1
        /// <summary>
        /// OFP Header
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Ofp_Header
        {
            /// <summary>
            /// OFP version
            /// </summary>
            public byte version;
            /// <summary>
            /// One of the OFPT_ constants.
            /// </summary>
            public Ofp_Type type; //byte
            /// <summary>
            /// Length including this ofp_header
            /// </summary>
            public UInt16 length;
            /// <summary>
            /// Transaction id associated with this packet. Replies use the same id as was in the request to facilitate pairing.
            /// </summary>
            public UInt32 xid;
        }

        /// <summary>
        /// Message Type
        /// </summary>
        public enum Ofp_Type : byte
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
            PFPT_PORT_MOD,

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
        public const int OFP_MAX_ETH_ALEN = 6;
        public const int OFP_MAX_PORT_NAME_LEN = 16;

        [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Ansi)]
        public struct Ofp_Phy_Port
        {
            /// <summary>
            /// Port no.
            /// </summary>
            public UInt16 port_no;
            /// <summary>
            /// Port MAC
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = OFP_MAX_ETH_ALEN)]
            public byte[] hw_addr;

            /// <summary>
            /// Port Name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = OFP_MAX_PORT_NAME_LEN,ArraySubType = UnmanagedType.ByValTStr)]
            public string name;

            /// <summary>
            /// Bitmap of OFPPC_* flags
            /// </summary>
            Ofp_Port_Config config; //UInt32

            /// <summary>
            /// Bitmap of OFPPS_* flags
            /// </summary>
            UInt32 state;
        }

        [Flags]
        public enum Ofp_Port_Config : uint
        {
            OFPPC_PORT_DOWN = 1<<0,
            OFPPC_NO_STP = 1<<1,
            OFPPC_NO_RECV = 1<<2,
            OFPPC_NO_RECV_STP = 1<<3,
            OFPPC_NO_FLOOD = 1<<4,
            OFPPC_NO_FWD = 1<<5,
            OFPPC_NO_PACKET_IN = 1<<6
        }

        public struct Ofp_Match
        {
            UInt32 wildcards;
        }
    }
}
