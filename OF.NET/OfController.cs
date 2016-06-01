using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FlowNet.Plugins;
using FlowNet.Topology;
using log4net;
using SharpServer;

namespace FlowNet
{
    public delegate void ClientConnectionHandler(IConnection connection);

    public class OfController : Server<OfClientConnection>, IController
    {
        protected ILog _log = LogManager.GetLogger(typeof(OfController));

        public event ClientConnectionHandler OnClientConnected;
        public event ClientConnectionHandler OnClientDisconnecting;
        public IDictionary<string, object> Variables { get; } = new Dictionary<string, object>();

        public Topo Topo => _topo;

        public IDictionary<string, IPlugin> Plugins => PluginSystem.Plugins;

        internal PluginSystem PluginSystem { get; private set; }
        private Topo _topo = new Topo();
        private const string FlowNet = "FlowNet";
        private const int FlowNetPort = 6633; //OF1.0:6633 OF1.3+:6653
        
        public OfController(int port = FlowNetPort, string logHeader = FlowNet) : base(port, logHeader)
        {
            Init();
        }

        public OfController(IPAddress ipAddress, int port = FlowNetPort, string logHeader = FlowNet) : base(ipAddress, port, logHeader)
        {
            Init();
        }

        public OfController(IPEndPoint[] localEndPoints, string logHeader = FlowNet) : base(localEndPoints, logHeader)
        {
            Init();
        }

        private void Init()
        {
            PluginSystem = new PluginSystem(this);
            LogDebug($"#FlowNet Start at {DateTime.Now}");
        }
        
        public void LoadPlugins(string path)
        {
            PluginSystem.Init(path);
        }

        public void LoadLogConfigsFromFile(string path)
        {
            if (File.Exists(path))
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            }
        }

        public void LogInfo(string message)
        {
            _log.Info(message);
        }

        public void LogDebug(string debug)
        {
            _log.Debug(debug);
        }

        public void LogError(string error)
        {
            _log.Error(error);
        }

        protected override void OnConnected(OfClientConnection connection)
        {
            OnClientConnected?.Invoke(connection);
        }

        protected override void OnDisconnecting(OfClientConnection connection)
        {
            LogInfo($"[{connection.Mac}] Disconnected.");
            OnClientDisconnecting?.Invoke(connection);
        }
    }
}
