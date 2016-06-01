using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;

namespace FlowNet.Topology
{
    //s-{dpid} 代指交换机
    //h-{mac} 代指主机

    public class Topo
    {
        private ulong _topoVer = 1;
        private ulong _graphVer = 1;

        private object _graphLock = new object();

        private Dijkstra<string> _dijkstra = new Dijkstra<string>();

        public IDictionary<string, Switch> Switches => _switches;

        public IDictionary<string, Host> Hosts => _hosts;

        private ConcurrentDictionary<string, Host> _hosts = new ConcurrentDictionary<string, Host>();
        private ConcurrentDictionary<string, Switch> _switches = new ConcurrentDictionary<string, Switch>();

        public ConcurrentDictionary<string, ConcurrentDictionary<string, Link>> Adjacency = new ConcurrentDictionary<string, ConcurrentDictionary<string, Link>>();

        public void AddSwitch(OfpSwitchFeatures switchFeatures, IConnection connection)
        {
            var name = switchFeatures.DatapathId.ToSwitchString();
            var sw = new Switch(switchFeatures, connection);
            
            if (!_switches.ContainsKey(name))
            {
                _switches.TryAdd(name, sw);
            }
            else
            {
                _switches[name] = sw;
            }

            if (!Adjacency.ContainsKey(name) || Adjacency[name] == null)
            {
                Adjacency[name] = new ConcurrentDictionary<string, Link>();
            }
            _topoVer++;
        }

        public void RemoveSwitch(ulong dpid)
        {
            var target = dpid.ToSwitchString();
            ConcurrentDictionary<string, Link> temp;
            Adjacency.TryRemove(target, out temp);
            foreach (var links in Adjacency.Values)
            {
                Link tempLink;
                links.TryRemove(target,out tempLink);
            }
            Switch sw;
            _switches.TryRemove(target,out sw);
            _topoVer++;
        }

        public void AddHost(Host host)
        {
            var name = host.MacAddress.ToHostString();
            _hosts[name] = host;
            if (!Adjacency.ContainsKey(name) || Adjacency[name] == null)
            {
                Adjacency[name] = new ConcurrentDictionary<string, Link>();
            }
            _topoVer++;
        }

        public void RemoveHost(PhysicalAddress address)
        {
            var h = address.ToHostString();
            ConcurrentDictionary<string, Link> temp;
            Adjacency.TryRemove(h, out temp);
            foreach (var links in Adjacency.Values)
            {
                Link tempLink;
                links.TryRemove(h,out tempLink);
            }
            Host tempHost;
            _hosts.TryRemove(h,out tempHost);
            _topoVer++;
        }

        public bool AddLink(ulong srcDpid, ushort srcPort, ulong dstDpid, ushort dstPort)
        {
            var src = srcDpid.ToSwitchString();
            var dst = dstDpid.ToSwitchString();
            if (_switches.ContainsKey(src) && _switches.ContainsKey(dst))
            {
                Adjacency[src][dst] = new Link(src, srcPort, dst, dstPort);
                _topoVer++;
                return true;
            }
            return false;
        }

        public bool AddTwoWayLink(ulong srcDpid, ushort srcPort, ulong dstDpid, ushort dstPort)
        {
            if (!AddLink(srcDpid, srcPort, dstDpid, dstPort))
            {
                return false;
            }
            if (!AddLink(dstDpid, dstPort, srcDpid, srcPort))
            {
                RemoveLink(srcDpid, dstDpid);
                return false;
            }
            _topoVer++;
            return true;
        }

        public void RemoveTwoWayLink(ulong srcDpid, ulong dstDpid)
        {
            bool mod = false;
            var src = $"s-{srcDpid.ToString()}";
            var dst = $"s-{dstDpid.ToString()}";
            Link tempLink;
            if (Adjacency.ContainsKey(src))
            {
                Adjacency[src].TryRemove(dst,out tempLink);
                mod = true;
            }
            if (Adjacency.ContainsKey(dst))
            {
                Adjacency[dst].TryRemove(src,out tempLink);
                mod = true;
            }
            if (mod)
            {
                _topoVer++;
            }
        }

        public bool AddHostLink(ulong switchDpid, ushort switchPort, PhysicalAddress hostMac)
        {
            var s = switchDpid.ToSwitchString();
            var h = hostMac.ToHostString();
            if (_switches.ContainsKey(s) && _hosts.ContainsKey(h))
            {
                Adjacency[s][h] = new Link(s, switchPort, h, 0);
                Adjacency[h][s] = new Link(h, 0, s, switchPort);
                _topoVer++;
                return true;
            }
            return false;
        }


        public void RemoveLink(ulong srcDpid, ulong dstDpid)
        {
            var src = $"s-{srcDpid.ToString()}";
            var dst = $"s-{dstDpid.ToString()}";
            if (Adjacency.ContainsKey(src))
            {
                Link tempLink;
                Adjacency[src].TryRemove(dst,out tempLink);
                _topoVer++;
            }
        }

        public void RemoveLinkByPortNo(ulong switchDpid, ushort portNo)
        {
            var s = $"s-{switchDpid.ToString()}";
            if (_switches.ContainsKey(s))
            {
                foreach (var adjs in Adjacency.Values)
                {
                    List<string> temp = (from adj in adjs let link = adj.Value where (link.Src == s && link.SrcPort == portNo) || (link.Dst == s && link.DstPort == portNo) select adj.Key).ToList();
                    foreach (var t in temp)
                    {
                        Link tempLink;
                        adjs.TryRemove(t,out tempLink);
                    }
                }
                _topoVer++;
            }
        }


        public void RemoveHostLink(ulong switchDpid, PhysicalAddress hostMac)
        {
            bool mod = false;
            var s = switchDpid.ToSwitchString();
            var h = hostMac.ToHostString();
            Link tempLink;
            if (Adjacency.ContainsKey(s))
            {
                Adjacency[s].TryRemove(h,out tempLink);
                mod = true;
            }
            if (Adjacency.ContainsKey(h))
            {
                Adjacency[h].TryRemove(s,out tempLink);
                mod = true;
            }
            if (mod)
            {
                _topoVer++;
            }
        }

        public List<string> FindShortestPath(string start, string end)
        {
            if (_topoVer != _graphVer)
            {
                lock (_graphLock)
                {
                    _dijkstra = new Dijkstra<string>();
                    foreach (var adjs in Adjacency)
                    {
                        _dijkstra.AddVertex(adjs.Key, adjs.Value.ToDictionary((s => s.Key), s => s.Value.Cost));
                    }
                    _graphVer = _topoVer;
                }
            }

            return _dijkstra.FindShortestPath(start, end);
        }
    }
}
