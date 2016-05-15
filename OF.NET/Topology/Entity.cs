using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet.Topology
{
    public abstract class Entity
    {
        public virtual bool IsSwitch { get; }
        public virtual PhysicalAddress MacAddress { get; }
        public virtual IPAddress IpAddress { get; }
        public virtual ulong DatapathId { get; }

        public override bool Equals(object obj)
        {
            Entity o = obj as Entity;

            if (o == null)
            {
                return false;
            }

            if (o.IsSwitch && this.IsSwitch && o.DatapathId == this.DatapathId) // && o.DatapathId != 0
            {
                return true;
            }
            if (!o.IsSwitch && !this.IsSwitch)
            {
                if (o.IpAddress != null && this.IpAddress != null && o.IpAddress == this.IpAddress)
                {
                    return true;
                }
                if (o.MacAddress != PhysicalAddress.None && o.MacAddress != null && this.MacAddress != PhysicalAddress.None && this.MacAddress != null && (o.MacAddress == this.MacAddress))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
