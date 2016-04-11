using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using FlowNet.OpenFlow;
using FlowNet.OpenFlow.OFP1_0;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNetTests
{
    [TestClass]
    public class OpenFlow0x1Test
    {
        [TestMethod]
        public void TestOfpHeader()
        {
            OfpHeader dst = new OfpHeader();
            OfpHeader src = new OfpHeader() { Length = 12, Version = 1, Type = OfpType.OFPT_ERROR, Xid = 2 };
            using (MemoryStream ms = new MemoryStream(src.ToByteArray()))
            {
                dst = new OfpHeader(ms);
            }
            Assert.AreEqual(src.Version, dst.Version);
            Assert.AreEqual(src.Type, dst.Type);
            Assert.AreEqual(src.Length, dst.Length);
            Assert.AreEqual(src.Xid, dst.Xid);
        }

        [TestMethod]
        public void TestOfpPhyPort()
        {
            OfpPhyPort dst = new OfpPhyPort();
            OfpPhyPort src = new OfpPhyPort() { Advertised = OfpPortFeatures.OFPPF_100MB_HD, State = OfpPortState.OFPPS_STP_LISTEN, Curr = OfpPortFeatures.OFPPF_100MB_FD, Peer = OfpPortFeatures.OFPPF_FIBER, Name = "TestPort", HwAddr = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 }, Supported = OfpPortFeatures.OFPPF_1GB_HD, PortNo = (ushort)OfpPort.OFPP_FLOOD, Config = OfpPortConfig.OFPPC_NO_FLOOD };
            using (MemoryStream ms = new MemoryStream(src.ToByteArray()))
            {
                dst = new OfpPhyPort(ms);
            }
            Assert.AreEqual(src.PortNo, dst.PortNo);
            Assert.AreEqual(src.Advertised, dst.Advertised);
            Assert.AreEqual(src.State, dst.State);
            Assert.AreEqual(src.Config, dst.Config);
            Assert.AreEqual(src.Curr, dst.Curr);
            Assert.AreEqual(src.HwAddr.ToPhysicalAddress(), dst.HwAddr.ToPhysicalAddress());
            Assert.AreEqual(src.Name, dst.Name);
            Assert.AreEqual(src.Peer, dst.Peer);
            Assert.AreEqual(src.Supported, dst.Supported);

        }

        [TestMethod]
        public void TestOfpQueuePropMinRate()
        {
            OfpQueuePropMinRate dst;
            OfpQueuePropMinRate src = new OfpQueuePropMinRate()
            {
                PropHeader = new OfpQueuePropHeader() { Len = 12, Property = OfpQueueProperties.QFPQT_MIN_RATE },
                Rate = 999
            };
            using (MemoryStream ms = new MemoryStream(src.ToByteArray()))
            {
                dst = new OfpQueuePropMinRate(ms);
            }

            Assert.AreEqual(src.PropHeader.Len, dst.PropHeader.Len);
            Assert.AreEqual(src.PropHeader.Property, dst.PropHeader.Property);
            Assert.AreEqual(src.Rate, dst.Rate);
        }

        [TestMethod]
        public void TestWildcards()
        {
            //IN_PORT,NW_PROTO
            uint w = Convert.ToUInt32("0000000000000001100000100100001", 2);
            OfpWildcards src = new OfpWildcards(w);
            OfpWildcards dst = new OfpWildcards(src.Value);
            dst.Wildcards |= OfpFlowWildcards.OFPFW_DL_SRC;
            dst.NwDstMask = 7;
            Assert.IsTrue(dst.Wildcards.HasFlag(OfpFlowWildcards.OFPFW_IN_PORT));
            Assert.IsTrue(dst.Wildcards.HasFlag(OfpFlowWildcards.OFPFW_NW_PROTO));
            Assert.IsTrue(dst.Wildcards.HasFlag(OfpFlowWildcards.OFPFW_DL_SRC));
            Assert.AreEqual(1u, dst.NwSrcMask);
            Assert.AreEqual(7u, dst.NwDstMask);
            Assert.AreEqual(Convert.ToUInt32("0000000000000011100000100100101", 2), dst.Value);
        }

        [TestMethod]
        public void TestOfpMatch()
        {
            OfpMatch dst;
            OfpMatch src = new OfpMatch()
            {
                DlSrc = new byte[] { 1, 2, 3, 4, 5, 6 },
                DlDst = new byte[] { 6, 5, 4, 3, 2, 1 },
                DlType = 0x8100,
                Wildcards = new OfpWildcards(Convert.ToUInt32("0000000000000001100000100100001", 2)),
                NwProto = 1,
                TpSrc = 3389,
                TpDst = 1433,
                NwTos = 10,
                InPort = 443,
                DlVlanPcp = 3,
                NwSrc = 123456u,
                NwDst = 654321u,
                DlVlan = 1
            };

            using (MemoryStream ms = new MemoryStream(src.ToByteArray()))
            {
                dst = new OfpMatch(ms);
            }

            Assert.AreEqual(src.DlDst.ToPhysicalAddress(), dst.DlDst.ToPhysicalAddress());
            Assert.AreEqual(src.DlSrc.ToPhysicalAddress(), dst.DlSrc.ToPhysicalAddress());
            Assert.AreEqual(src.DlType, dst.DlType);
            Assert.AreEqual(src.DlVlan, dst.DlVlan);
            Assert.AreEqual(src.DlVlanPcp, dst.DlVlanPcp);
            Assert.AreEqual(src.InPort, dst.InPort);
            Assert.AreEqual(src.NwDst, dst.NwDst);
            Assert.AreEqual(src.NwProto, dst.NwProto);
            Assert.AreEqual(src.NwSrc, dst.NwSrc);
            Assert.AreEqual(src.NwTos, dst.NwTos);
            Assert.AreEqual(src.Wildcards, dst.Wildcards);
            Assert.AreEqual(src.TpDst, dst.TpDst);
            Assert.AreEqual(src.TpSrc, dst.TpSrc);
        }

        [TestMethod]
        public void TestOfpSwitchFeatures()
        {
            OfpSwitchFeatures dst;
            OfpSwitchFeatures src = new OfpSwitchFeatures()
            {
                Actions = (OfpActionCapabilities)(1 << 12 - 1),
                Capabilities = OfpCapabilities.OFPC_TABLE_STATS | OfpCapabilities.OFPC_PORT_STATS,
                DatapathId = 1234567u,
                NBuffers = 10,
                NTables = 1,
                Ports = new List<OfpPhyPort>()
                {
  new OfpPhyPort() {Advertised = OfpPortFeatures.OFPPF_100MB_HD, State = OfpPortState.OFPPS_STP_LISTEN, Curr = OfpPortFeatures.OFPPF_100MB_FD, Peer = OfpPortFeatures.OFPPF_FIBER, Name = "TestPort", HwAddr = new byte[] {0x01,0x02,0x03,0x04,0x05,0x06}, Supported = OfpPortFeatures.OFPPF_1GB_HD, PortNo = (ushort)OfpPort.OFPP_FLOOD, Config = OfpPortConfig.OFPPC_NO_FLOOD},
  new OfpPhyPort() {Advertised = OfpPortFeatures.OFPPF_100MB_HD, State = OfpPortState.OFPPS_STP_LISTEN, Curr = OfpPortFeatures.OFPPF_100MB_FD, Peer = OfpPortFeatures.OFPPF_FIBER, Name = "TestPort2", HwAddr = new byte[] {0x01,0x02,0x03,0x04,0x05,0x06}, Supported = OfpPortFeatures.OFPPF_1GB_HD, PortNo = (ushort)OfpPort.OFPP_FLOOD, Config = OfpPortConfig.OFPPC_NO_FLOOD}
                }
            };


            using (MemoryStream ms = new MemoryStream(src.ToByteArray()))
            {
                dst = new OfpSwitchFeatures(ms);
            }

            Assert.AreEqual(src.Actions, dst.Actions);
            Assert.AreEqual(src.Capabilities, dst.Capabilities);
            Assert.AreEqual(src.DatapathId, dst.DatapathId);
            Assert.AreEqual(src.NBuffers, dst.NBuffers);
            Assert.AreEqual(src.NTables, dst.NTables);
            Assert.AreEqual(src.Ports[0].Name, dst.Ports[0].Name);
            Assert.AreEqual(src.Ports[1].Name, dst.Ports[1].Name);

            Assert.IsTrue(dst.Capabilities.HasFlag(OfpCapabilities.OFPC_PORT_STATS));
            Assert.IsFalse(dst.Capabilities.HasFlag(OfpCapabilities.OFPC_RESERVED));
            Assert.IsTrue(dst.Actions.HasFlag(OfpActionCapabilities.OFPAT_ENQUEUE));

        }
        
        [TestMethod]
        public void TestOfpFlowMod()
        {
            OfpFlowMod dst;
            //A flow entry to redirect all packets which dst-port is 3360(mysql) to dst-port 1433(sqlserver)
            OfpFlowMod src = new OfpFlowMod()
            {
                Command = OfpFlowModCommand.OFPFC_ADD, 
                Match = new OfpMatch()
                {
                    Wildcards = new OfpWildcards() { Wildcards = ~OfpFlowWildcards.OFPFW_TP_SRC },
                    TpDst = 3360,
                },
                Actions = new ActionList()
                {
                    { OfpActionType.OFPAT_SET_TP_DST, new OfpActionTpPort(true) {TpPort = 1433} },
                    { OfpActionType.OFPAT_OUTPUT, new OfpActionOutput() {Port = (ushort)OfpPort.OFPP_ALL} } //MARK:如何确定发往哪个端口——建立、查询Datapath和MAC数据库
                }
            };

            using (MemoryStream ms = new MemoryStream(src.ToByteArray()))
            {
                dst = new OfpFlowMod(ms);
            }

            Assert.AreEqual(src.Match.Wildcards, dst.Match.Wildcards);
            Assert.AreEqual(src.Match.TpDst, dst.Match.TpDst);
            Assert.IsTrue(((OfpActionOutput)dst.Actions[OfpActionType.OFPAT_OUTPUT]).Port == (ushort)OfpPort.OFPP_ALL);
            Assert.IsTrue(((OfpActionTpPort)dst.Actions[OfpActionType.OFPAT_SET_TP_DST]).TpPort == 1433);
        }
    }
}
