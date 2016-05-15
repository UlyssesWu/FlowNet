using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;
using FlowNet.Plugins;
using FlowNet.Topology;
using PacketDotNet;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.Switch
{
    [Export(typeof(IPlugin))]
    [ExportMetadata("Name", "FlowNet.Switch")]
    [ExportMetadata("Description", "Stupid L3 Switch for FlowNet")]
    public class L3Switch : IPlugin
    {
        private SwitchHandler _handler;

        public void Dispose()
        {
        }


        public bool Active { get; set; } = true;
        public int Priority { get; set; } = 200;
        public void Init(IController controller)
        {
            _handler = new SwitchHandler(controller);
        }

        public MessageHandler MessageHandler => _handler;
    }

    internal class SwitchHandler : MessageHandler
    {
        private const ushort _flowPriority = 60000;

        private IController _controller;
        public SwitchHandler(IController controller)
        {
            _controller = controller;
        }

        public override bool PacketIn(OfpPacketIn packetIn, object packet, IConnection handler)
        {
            Packet p = packet as Packet;
            if (p == null)
            {
                return false;
            }
            bool first = false;
            ARPPacket arp = p.Extract(typeof(ARPPacket)) as ARPPacket;
            if (arp != null)
            {
                if (!Equals(arp.SenderHardwareAddress, EthernetAddress.ETHER_ANY))
                {
                    var host = arp.SenderHardwareAddress.ToHostString();
                    if (!Equals(arp.SenderProtocolAddress, IPAddress.Any) && !Equals(arp.SenderProtocolAddress, IPAddress.Broadcast))
                    {
                        if (_controller.Topo.Hosts.ContainsKey(host)) //更新host
                        {
                            _controller.Topo.Hosts[host].SetIpAddress(arp.SenderProtocolAddress);
                        }
                        else //添加host及link
                        {
                            var h = new Host(arp.SenderHardwareAddress);
                            h.SetIpAddress(arp.SenderProtocolAddress);
                            _controller.Topo.AddHost(h);
                            _controller.Topo.AddHostLink(handler.SwitchFeatures.DatapathId, packetIn.InPort,
                                h.MacAddress);

                            first = true;
                        }
                    }
                }
                if (arp.Operation == ARPOperation.Request)
                {
                    if (!arp.TargetProtocolAddress.IsAvailable())
                    {
                        Flood(p, handler);
                        return false;
                    }
                    foreach (var host in _controller.Topo.Hosts.Values)
                    {
                        if (Equals(host.IpAddress, arp.TargetProtocolAddress) && host.HasIpAddress)
                        {
                            ARPPacket reply = new ARPPacket(ARPOperation.Response, arp.SenderHardwareAddress,
                                arp.SenderProtocolAddress, host.MacAddress, host.IpAddress);
                            EthernetPacket ep = new EthernetPacket(host.MacAddress, arp.SenderHardwareAddress,
                                EthernetPacketType.Arp);
                            ep.PayloadPacket = reply;
                            OfpPacketOut packetOut = new OfpPacketOut() { Data = ep.Bytes};
                            packetOut.Actions.Add(OfpActionType.OFPAT_OUTPUT, new OfpActionOutput() { Port = packetIn.InPort });
                            handler.Write(packetOut.ToByteArray());
                            return false;
                        }
                    }
                    Flood(p, handler);
                    return false;
                }
                else if (arp.Operation == ARPOperation.Response)
                {
                    HandleArpResponse(arp);
                    //Send ARP back

                    var dst = arp.TargetHardwareAddress.ToHostString();
                    if (!_controller.Topo.Hosts.ContainsKey(dst))
                    {
                        return false;
                    }
                    foreach (var key in _controller.Topo.Adjacency[dst].Keys)
                    {
                        if (_controller.Topo.Switches.ContainsKey(key))
                        {
                            Console.WriteLine("send ARP back");
                            var link = _controller.Topo.Adjacency[key][dst];
                            OfpPacketOut packetOut = new OfpPacketOut() { Data = p.Bytes };
                            packetOut.Actions.Add(OfpActionType.OFPAT_OUTPUT, new OfpActionOutput() { Port = link.SrcPort });
                            _controller.Topo.Switches[key].Connection.Write(packetOut.ToByteArray());
                            return false;
                        }
                    }
                }
            }
            else
            {
                IPv4Packet ipPacket = p.Extract(typeof(IPv4Packet)) as IPv4Packet;
                if (ipPacket == null)
                {
                    return false;
                }
                PhysicalAddress srcMac = PhysicalAddress.None;
                foreach (var host in _controller.Topo.Hosts.Values)
                {
                    if (Equals(host.IpAddress, ipPacket.SourceAddress))
                    {
                        srcMac = host.MacAddress;
                        break;
                    }
                }

                if (Equals(srcMac, PhysicalAddress.None))
                {
                    return false;
                }

                foreach (var host in _controller.Topo.Hosts.Values)
                {
                    if (Equals(host.IpAddress, ipPacket.DestinationAddress))
                    {
                        var dstMac = host.MacAddress;
                        var dstIp = ipPacket.DestinationAddress;
                        var srcIp = ipPacket.SourceAddress;
                        var src = srcMac.ToHostString();
                        var dst = dstMac.ToHostString();
                        if (_controller.Topo.Hosts.ContainsKey(src) && _controller.Topo.Hosts.ContainsKey(dst))
                        {
                            var path = _controller.Topo.FindShortestPath(src, dst);

                            if (path.Count > 0)
                            {
                                path.Reverse();
                                for (int i = 0; i < path.Count - 1; i++)
                                {
                                    string s = path[i];
                                    if (s.StartsWith("s"))
                                    {
                                        //Apply FlowMod
                                        var sw = _controller.Topo.Switches[s];
                                        if (sw != null)
                                        {
                                            var link = _controller.Topo.Adjacency[s][path[i + 1]];
                                            if (link != null)
                                            {
                                                //Console.WriteLine("send IPv4 flowmod");
                                                SendFlowMod(sw, link, srcMac, dstMac);
                                                //SendFlowMod(sw, link, srcIp, dstIp);
                                            }
                                        }
                                    }
                                }

                            }

                            path = _controller.Topo.FindShortestPath(dst, src);

                            if (path.Count > 0)
                            {
                                path.Reverse();
                                for (int i = 0; i < path.Count - 1; i++)
                                {
                                    string s = path[i];
                                    if (s.StartsWith("s"))
                                    {
                                        //Apply FlowMod
                                        var sw = _controller.Topo.Switches[s];
                                        if (sw != null)
                                        {
                                            var link = _controller.Topo.Adjacency[s][path[i + 1]];
                                            if (link != null)
                                            {
                                                //Console.WriteLine("send IPv4 flowmod");
                                                SendFlowMod(sw, link, dstMac, srcMac);
                                                //SendFlowMod(sw, link, dstIp, srcIp);
                                            }
                                        }
                                    }
                                }
                            }

                        }

                    }
                }
            }
            return false;
        }

        private void HandleArpResponse(ARPPacket arp)
        {
            var src = arp.SenderHardwareAddress.ToHostString();
            var dst = arp.TargetHardwareAddress.ToHostString();
            if (_controller.Topo.Hosts.ContainsKey(src) && _controller.Topo.Hosts.ContainsKey(dst))
            {
                var path = _controller.Topo.FindShortestPath(src, dst);
                if (path.Count > 0)
                {
                    path.Reverse();
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        string s = path[i];
                        if (s.StartsWith("s"))
                        {
                            //Apply FlowMod
                            var sw = _controller.Topo.Switches[s];
                            if (sw != null)
                            {
                                var link = _controller.Topo.Adjacency[s][path[i + 1]];
                                if (link != null)
                                {
                                    //Debug.WriteLine($"{link.Src}:{link.SrcPort}->{link.Dst}:{link.DstPort}");
                                    //SendFlowMod(sw, link, arp.TargetHardwareAddress, arp.SenderHardwareAddress);
                                    SendFlowMod(sw, link, arp.SenderHardwareAddress, arp.TargetHardwareAddress); //MARK:RIGHT
                                    //SendFlowMod(sw, link, arp.TargetProtocolAddress, arp.SenderProtocolAddress);
                                }
                            }
                        }
                    }

                    //for (int i = 0; i < path.Count - 1; i++)
                    //{
                    //    string s = path[i];
                    //    if (s.StartsWith("s"))
                    //    {
                    //        //Apply FlowMod
                    //        var sw = _controller.Topo.Switches[s];
                    //        if (sw != null)
                    //        {
                    //            var link = _controller.Topo.Adjacency[s][path[i + 1]];
                    //            if (link != null)
                    //            {
                    //                //Debug.WriteLine($"{link.Src}:{link.SrcPort}->{link.Dst}:{link.DstPort}");
                    //                SendFlowMod(sw, link, arp.SenderHardwareAddress, arp.TargetHardwareAddress);
                    //                SendFlowMod(sw, link, arp.SenderProtocolAddress, arp.TargetProtocolAddress);
                    //            }
                    //        }
                    //    }
                    //}
                }

                path = _controller.Topo.FindShortestPath(dst, src);
                if (path.Count > 0)
                {
                    path.Reverse();
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        string s = path[i];
                        if (s.StartsWith("s"))
                        {
                            //Apply FlowMod
                            var sw = _controller.Topo.Switches[s];
                            if (sw != null)
                            {
                                var link = _controller.Topo.Adjacency[s][path[i + 1]];
                                if (link != null)
                                {
                                    //Debug.WriteLine($"{link.Src}:{link.SrcPort}->{link.Dst}:{link.DstPort}");
                                    SendFlowMod(sw, link, arp.TargetHardwareAddress, arp.SenderHardwareAddress);
                                    //SendFlowMod(sw, link, arp.SenderProtocolAddress, arp.TargetProtocolAddress);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SendFlowMod(Topology.Switch sw, Link link, PhysicalAddress src, PhysicalAddress dst, ushort priority = _flowPriority)
        {
            OfpMatch match = new OfpMatch
            {
                //Wildcards = new OfpWildcards() { Wildcards = 0 },
                Wildcards = new OfpWildcards() { Wildcards = ~(OfpFlowWildcards.OFPFW_DL_DST | OfpFlowWildcards.OFPFW_DL_SRC),NwDstMask = 63,NwSrcMask = 63},
                DlSrc = src.GetAddressBytes(),
                DlDst = dst.GetAddressBytes(),
                DlType = (ushort)EthernetPacketType.IpV4,
            };
            OfpFlowMod flow = new OfpFlowMod
            {
                Priority = priority,
                Match = match,
            };
            flow.Actions[OfpActionType.OFPAT_OUTPUT] = new OfpActionOutput() { Port = link.SrcPort };
            Console.WriteLine($"[{sw.MacAddress}]FlowMod from [{src}] to [{dst}]");
            sw.Connection.Write(flow.ToByteArray());
        }

        private void SendFlowMod(IConnection connection, Link link, PhysicalAddress src, PhysicalAddress dst, ushort priority = _flowPriority)
        {
            OfpMatch match = new OfpMatch
            {
                //Wildcards = new OfpWildcards() { Wildcards = 0 },
                Wildcards = new OfpWildcards() { Wildcards = ~(OfpFlowWildcards.OFPFW_DL_DST | OfpFlowWildcards.OFPFW_DL_SRC), NwDstMask = 63, NwSrcMask = 63 },
                DlSrc = src.GetAddressBytes(),
                DlDst = dst.GetAddressBytes()
            };
            OfpFlowMod flow = new OfpFlowMod
            {
                Priority = priority,
                Match = match
            };
            flow.Actions[OfpActionType.OFPAT_OUTPUT] = new OfpActionOutput() { Port = link.SrcPort };
            Console.WriteLine($"[{connection.Mac}]FlowMod from [{src}] to [{dst}]");
            connection.Write(flow.ToByteArray());
        }

        private void SendFlowMod(IConnection connection, Link link, IPAddress src, IPAddress dst, ushort priority = _flowPriority)
        {
            OfpMatch match = new OfpMatch
            {
                Wildcards = new OfpWildcards() { NwDstMask = 0, NwSrcMask = 0},
                NwSrc = (uint)src.Address,
                NwDst = (uint)dst.Address,
            };
            OfpFlowMod flow = new OfpFlowMod
            {
                Priority = priority,
                Match = match
            };
            flow.Actions[OfpActionType.OFPAT_OUTPUT] = new OfpActionOutput() { Port = link.SrcPort };
            Console.WriteLine($"[{connection.Mac}]FlowMod from [{src}] to [{dst}]");
            connection.Write(flow.ToByteArray());
        }

        private void SendFlowMod(Topology.Switch sw, Link link, IPAddress src, IPAddress dst, ushort priority = _flowPriority)
        {
            OfpMatch match = new OfpMatch
            {
                Wildcards = new OfpWildcards() { NwDstMask = 0, NwSrcMask = 0 },
                NwSrc = (uint)src.Address,
                NwDst = (uint)dst.Address,
            };
            OfpFlowMod flow = new OfpFlowMod
            {
                Priority = priority,
                Match = match
            };
            flow.Actions[OfpActionType.OFPAT_OUTPUT] = new OfpActionOutput() { Port = link.SrcPort };
            Console.WriteLine($"[{sw.MacAddress}]FlowMod from [{src}] to [{dst}]");
            sw.Connection.Write(flow.ToByteArray());
        }

        private void Flood(Packet packet, IConnection connection, uint bufferId = uint.MaxValue)
        {
            Console.WriteLine($"[{connection.Mac}]Flood packet");
            OfpPacketOut packetOut = new OfpPacketOut() { Data = packet.Bytes};
            packetOut.Actions.Add(OfpActionType.OFPAT_OUTPUT, new OfpActionOutput() { Port = (ushort)OfpPort.OFPP_FLOOD });
            connection.Write(packetOut.ToByteArray());
        }
    }
}
