using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet.Topology
{
    public class Host : Entity
    {
        public override bool IsSwitch => false;
        private IPAddress _ip;
        private PhysicalAddress _mac;
        public override ulong DatapathId => 0;
        public override PhysicalAddress MacAddress => _mac;
        public override IPAddress IpAddress => _ip;

        public Host(PhysicalAddress mac)
        {
            _mac = mac;
        }

        public void SetIpAddress(IPAddress address)
        {
            _ip = address;
        }

        public bool HasIpAddress => !Equals(_ip, IPAddress.Any) && !Equals(_ip, IPAddress.Broadcast) && !Equals(_ip, IPAddress.None);

        //public void SetMacAddress(PhysicalAddress address)
        //{
        //    _mac = address;
        //}

    }
}
