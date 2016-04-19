using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet.Topology
{
    public class Switch : IEntity
    {
        Dictionary<uint, Port> Ports = new Dictionary<uint, Port>();
         
    }
}
