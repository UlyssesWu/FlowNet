using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;

namespace FlowNet.Topology
{
    /// <summary>
    /// 端口
    /// </summary>
    class Port
    {
        public uint No;
        public string Name => PortInfo.Name;
        public OfpPhyPort PortInfo;
    }
}
