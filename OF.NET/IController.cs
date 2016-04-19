using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowNet.Plugins;
using FlowNet.Topology;

namespace FlowNet
{
    public interface IController
    {
        event ClientConnectedHandler OnClientConnected;

        IDictionary<string, object> Variables { get; }
        Topo Topo { get; }
        IDictionary<string,IPlugin> Plugins { get; }

        void LogInfo(string message);

        void LogError(string error);
    }
}
