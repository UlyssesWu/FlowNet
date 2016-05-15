using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Net.NetworkInformation;
using FlowNet.OpenFlow;
using FlowNet.OpenFlow.OFP1_0;
using FlowNet.Plugins;
using PacketDotNet;
using PacketDotNet.LLDP;
using PacketDotNet.Utils;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.Discovery
{
    [Export(typeof (IPlugin))]
    [ExportMetadata("Name","FlowNet.Discovery")]
    [ExportMetadata("Description","LLDP Discovery for FlowNet")]
    public class Discovery : IPlugin
    {
        private DiscoveryHandler _handler;
        private ushort _flowPriority = 65000;
        private IController _controller;
        public bool Active { get; set; } = true;
        public int Priority { get; set; } = 100;

        public void Init(IController controller)
        {
            _controller = controller;
            _handler = new DiscoveryHandler(_controller);
            _controller.OnClientConnected += PrepareFlow;
            _controller.OnClientDisconnecting += RemoveFlow;
        }

        private void PrepareFlow(IConnection connection)
        {
            connection.OnConnectionReady += InstallFlow;
        }

        private void RemoveFlow(IConnection connection)
        {
            if (connection.SwitchFeatures!=null)
            {
                _controller.Topo.RemoveSwitch(connection.SwitchFeatures.DatapathId);
            }
        }

        private void InstallFlow(IConnection connection)
        {
            _controller.Topo.AddSwitch(connection.SwitchFeatures, connection);

            Console.WriteLine(($"[{connection.Mac}] Installing Discovery Flow"));
            OfpMatch match = new OfpMatch
            {
                Wildcards = new OfpWildcards() { Wildcards = OfpFlowWildcards.OFPFW_DL_TYPE | OfpFlowWildcards.OFPFW_DL_DST},
                DlType = (ushort) EthernetPacketType.LLDP,
                DlDst = EthernetAddress.NDP_MULTICAST.GetAddressBytes()
            };

            OfpFlowMod flow = new OfpFlowMod
            {
                Priority = _flowPriority,
                Match = match
            };
            flow.Actions[OfpActionType.OFPAT_OUTPUT] = new OfpActionOutput() {Port = (ushort) OfpPort.OFPP_CONTROLLER};
            connection.Write(flow.ToByteArray());

            //Console.WriteLine($"[{connection.Mac}]Sending LLDP");
            foreach (var port in connection.SwitchFeatures.Ports)
            {
                _handler.SendLldpPacket(connection, port);
            }
        }

        public MessageHandler MessageHandler => _handler;
        public void Dispose()
        {
        }
    }

    internal class DiscoveryHandler : MessageHandler
    {
        public ushort Ttl = 120;
        private IController _controller;

        public DiscoveryHandler(IController controller)
        {
            _controller = controller;
        }

        public LLDPPacket ParseLldpPacket(byte[] bytes)
        {
            var packet = Packet.ParsePacket(LinkLayers.Ethernet, bytes);
            var lldp = packet.PayloadPacket as LLDPPacket;
            return lldp;
        }

        //private void Test()
        //{
        //    var b = GenerateLldpPacket(EthernetAddress.ETHER_ANY, 3389, 12345);
        //}

        public bool SendLldpPacket(IConnection connection, OfpPhyPort port)
        {
            if (OfpPhyPort.IsReservedPort(port.PortNo))
            {
                return false;
            }
            Console.WriteLine($"[{connection.Mac}] Sending LLDP to Port:{port.PortNo} (PortMAC:{port.HwAddr.ToPhysicalAddress()})");
            OfpPacketOut packetOut = new OfpPacketOut
            {
                Data =
                    GenerateLldpPacket(port.HwAddr.ToPhysicalAddress(), port.PortNo,
                        connection.SwitchFeatures.DatapathId)
            };
            packetOut.Actions.Add(OfpActionType.OFPAT_OUTPUT, new OfpActionOutput() { Port = port.PortNo });
            connection.Write(packetOut.ToByteArray());
            return true;
        }

        public byte[] GenerateLldpPacket(PhysicalAddress portMac,uint portNum,ulong dpid)
        {
            LLDPPacket packet = new LLDPPacket();
            packet.TlvCollection.Add(new ChassisID(dpid.GetPhysicalAddress()));
            packet.TlvCollection.Add(new PortID(PortSubTypes.PortComponent,BitConverter.GetBytes(portNum)));
            packet.TlvCollection.Add(new TimeToLive(Ttl));
            packet.TlvCollection.Add(new SystemDescription(dpid.ToString("X")));
            packet.TlvCollection.Add(new EndOfLLDPDU());
            packet = new LLDPPacket(new ByteArraySegment(packet.Bytes));

            EthernetPacket ethernet = new EthernetPacket(portMac, EthernetAddress.NDP_MULTICAST,EthernetPacketType.LLDP);
            ethernet.PayloadPacket = packet;
            
            return ethernet.Bytes;
        }

        public LLDPPacket TryLldpPacket(Packet packet)
        {
            LLDPPacket lp = null;
            if (packet != null)
            {
                lp = packet.Extract(typeof(LLDPPacket)) as LLDPPacket;
            }
            return lp;
        }

        public void HandleLldpPacket(LLDPPacket packet, OfpPacketIn packetIn, IConnection connection)
        {
            if (packet.TlvCollection.Count < 3)
            {
                _controller.LogError($"[{connection.Mac}] LLDP TLV not enough");
                return;
            }
            ulong dpid = 0;
            var ch = packet.TlvCollection[0] as ChassisID;
            var po = packet.TlvCollection[1] as PortID;
            if (packet.TlvCollection.Count >= 4)
            {
                var sd = packet.TlvCollection[3] as SystemDescription;
                dpid = ulong.Parse(sd.Description, NumberStyles.AllowHexSpecifier);
            }
            if (dpid == 0)
            {
                dpid = ch.MACAddress.GetDatapathId();
            }
            ushort port = 0;
            if (po != null && po.SubTypeValue != null)
            {
                port = BitConverter.ToUInt16((byte[]) po.SubTypeValue, 0);
            }
            Console.WriteLine($"[{connection.Mac}:{packetIn.InPort}] is connected with [{ch?.MACAddress}:{port}] (Get LLDP)");

            //Update Topo
            _controller.Topo.AddTwoWayLink(dpid, port, connection.SwitchFeatures.DatapathId, packetIn.InPort);
        }

        public override bool PacketIn(OfpPacketIn packetIn,object packet, IConnection handler)
        {
            try
            {
                var p = packet as Packet;
                var lp = TryLldpPacket(p);
                if (lp != null)
                {
                    HandleLldpPacket(lp, packetIn, handler);
                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        public override bool PortStatus(OfpPortStatus status, IConnection handler)
        {
            if (status.Reason == OfpPortReason.OFPPR_ADD || status.Reason == OfpPortReason.OFPPR_MODIFY)
            {
                SendLldpPacket(handler, status.Desc);
            }
            if (status.Reason == OfpPortReason.OFPPR_DELETE)
            {
                _controller.Topo.RemoveLinkByPortNo(handler.SwitchFeatures.DatapathId, status.Desc.PortNo);
            }
            return false;
        }
    }
}
