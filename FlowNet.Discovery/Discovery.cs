using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;
using FlowNet.Plugins;
using PacketDotNet;
using PacketDotNet.LLDP;

namespace FlowNet.Discovery
{
    public class Discovery : IPlugin
    {
        private IController _controller;
        public bool Active { get; set; } = true;
        public void Init(IController controller)
        {
            _controller = controller;
        }

        public MessageHandler MessageHandler { get; }
    }

    public class DiscoveryHandler : MessageHandler
    {
        public ushort Ttl = 120;
        public byte[] GenerateLldpPacket(PhysicalAddress switchMac,uint portNum,ulong dpid)
        {
            LLDPPacket packet = new LLDPPacket();
            packet.TlvCollection.Add(new ChassisID(switchMac));
            packet.TlvCollection.Add(new PortID(PortSubTypes.InterfaceName,portNum.ToString("X")));
            packet.TlvCollection.Add(new TimeToLive(Ttl));
            packet.TlvCollection.Add(new SystemDescription(dpid.ToString("X")));
            packet.TlvCollection.Add(new EndOfLLDPDU());
            
            EthernetPacket ethernet = new EthernetPacket(switchMac,EthernetAddress.NDP_MULTICAST,EthernetPacketType.LLDP);
            ethernet.PayloadPacket = packet;
            
            return ethernet.Bytes;
        }
        public override bool PacketIn(OfpPacketIn packetIn, IConnection handler)
        {

            return base.PacketIn(packetIn, handler);
        }
    }
}
