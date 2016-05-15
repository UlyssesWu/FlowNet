using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet.Topology
{
    public class Link
    {
        public string Src;
        public string Dst;
        public ushort SrcPort;
        public ushort DstPort;
        public int Cost = 1;

        public Link(string src, ushort srcPort, string dst, ushort dstPort, int cost = 1)
        {
            Src = src;
            SrcPort = srcPort;
            Dst = dst;
            DstPort = dstPort;
            Cost = cost;
        }
    }
}
