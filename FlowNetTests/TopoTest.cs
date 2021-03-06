﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using FlowNet.Topology;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PacketDotNet;
using PacketDotNet.LLDP;
using PacketDotNet.Utils;
using System.Xml;

namespace FlowNetTests
{
    /// <summary>
    /// 测试拓扑相关功能
    /// </summary>
    [TestClass]
    public class TopoTest
    {

        public TopoTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void DijkstraTest()
        {
            Dijkstra g = new Dijkstra();
            g.AddVertex("A", new Dictionary<string, int>() { { "B", 7 }, { "C", 8 } });
            g.AddVertex("B", new Dictionary<string, int>() { { "A", 7 }, { "F", 2 } });
            //g.AddVertex("B", new Dictionary<string, int>() { { "F", 2 } });
            g.AddVertex("C", new Dictionary<string, int>() { { "A", 8 }, { "F", 6 }, { "G", 4 } });
            g.AddVertex("D", new Dictionary<string, int>() { { "F", 8 } });
            g.AddVertex("E", new Dictionary<string, int>() { { "H", 1 } });
            g.AddVertex("F", new Dictionary<string, int>() { { "B", 2 }, { "C", 6 }, { "D", 8 }, { "G", 9 }, { "H", 3 } });
            g.AddVertex("G", new Dictionary<string, int>() { { "C", 4 }, { "F", 9 } });
            g.AddVertex("H", new Dictionary<string, int>() { { "E", 1 }, { "F", 3 } });

            StringBuilder sb = new StringBuilder();
            g.FindShortestPath("A", "H").ForEach(x => sb.Insert(0,x));
            var dst = sb.ToString();
            var src = "BFH";
            Assert.AreEqual(src,dst);

            Dijkstra<Entity> g2 = new Dijkstra<Entity>();
            //Host h1 = new Host();
            //h1.SetIpAddress(IPAddress.Parse("10.0.0.1"));

            StringBuilder sb2 = new StringBuilder();
            //g2.FindShortestPath("A", "H").ForEach(x => sb2.Insert(0, x));
            var dst2 = sb2.ToString();
            var src2 = "BFH";
            Assert.AreEqual(src2, dst2);
        }

        public static readonly PhysicalAddress NDP_MULTICAST =
           new PhysicalAddress(new byte[] { 0x01, 0x23, 0x20, 0x00, 0x00, 0x01 });

        public byte[] GenerateLldpPacket(PhysicalAddress switchMac, uint portNum, ulong dpid)
        {
            LLDPPacket packet = new LLDPPacket();
            packet.TlvCollection.Add(new ChassisID(switchMac));
            packet.TlvCollection.Add(new PortID(PortSubTypes.PortComponent, BitConverter.GetBytes(portNum)));
            packet.TlvCollection.Add(new TimeToLive(120));
            packet.TlvCollection.Add(new SystemDescription(dpid.ToString("X")));
            packet.TlvCollection.Add(new EndOfLLDPDU());
            packet = new LLDPPacket(new ByteArraySegment(packet.Bytes));
            
            EthernetPacket ethernet = new EthernetPacket(switchMac, NDP_MULTICAST, EthernetPacketType.LLDP);
            ethernet.PayloadPacket = packet;

            return ethernet.Bytes;
        }

        [TestMethod]
        public void LldpTest()
        {
            var b = GenerateLldpPacket(NDP_MULTICAST, 3389, 12345);
        }
    }
}
