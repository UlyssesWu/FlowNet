using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;

namespace FlowNet.Topology
{
    /// <summary>
    /// 交换机
    /// </summary>
    public class Switch : Entity
    {
        public readonly IConnection Connection;
        public override bool IsSwitch => true;
        public override IPAddress IpAddress => IPAddress.None;
        public override PhysicalAddress MacAddress => _features?.DatapathId.GetPhysicalAddress() ?? PhysicalAddress.None;
        public override ulong DatapathId => _features?.DatapathId ?? 0;
        public OfpSwitchFeatures Features {
            get { return _features; }
            set { _features = value; } }

        private OfpSwitchFeatures _features;

        public Switch(OfpSwitchFeatures features,IConnection connection)
        {
            Features = features;
            Connection = connection;
        }

        public Switch()
        { }
    }
}
