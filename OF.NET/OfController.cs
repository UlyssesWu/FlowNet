using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SharpServer;

namespace FlowNet
{
    public class OfController : Server<OfClientConnection>
    {
        private const string FlowNet = "FlowNet";
        private const int FlowNetPort = 6633; //OF1.0:6633 OF1.3+:6653
        
        public OfController(int port = FlowNetPort, string logHeader = FlowNet) : base(port, logHeader)
        {
        }

        public OfController(IPAddress ipAddress, int port = FlowNetPort, string logHeader = FlowNet) : base(ipAddress, port, logHeader)
        {
        }

        public OfController(IPEndPoint[] localEndPoints, string logHeader = FlowNet) : base(localEndPoints, logHeader)
        {
        }
    }
}
