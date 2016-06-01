using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using SharpServer;
using FlowNet.OpenFlow.OFP1_0;
using PacketDotNet;
using static FlowNet.OpenFlow.OFP1_0.Data; //TODO: Make a universial data layer

namespace FlowNet
{
    public class OfClientConnection : ClientConnection, IConnection
    {
        public event ClientConnectionHandler OnConnectionReady;
        public OfpSwitchFeatures SwitchFeatures { get; private set; }
        public PhysicalAddress Mac => _mac;

        public bool Inited => _inited;

        private OfController _controller;
        private bool _inited = false;
        private bool _versionChecked = false;
        private bool _featureChecked = false;
        private bool _configChecked = false;
        private bool _stateChecked = false;
        private int _version = 1;
        private PhysicalAddress _mac = PhysicalAddress.None;

        public byte MaxSupportedVersion => 1;

        protected void HandleMessage(byte[] message, OfpHeader header)
        {
            //state = null;
            MemoryStream ms = new MemoryStream(message);
            //var header = ParesHeader(ms);
            if (header == null)
            {
                return;
            }

            _log.Debug($"[{_mac}] message={header.Type.ToString()}");
            //Console.WriteLine($"[{_mac}] message={header.Type.ToString()}");

            if (!_inited)
            {
                Initialize(ms, header);
                if (_versionChecked && _configChecked && _featureChecked)
                {
                    _inited = true;
                    _log.Info($"[{_mac}] inited.");
                    //Console.WriteLine($"[{_mac}] inited.");
                    OnConnectionReady?.Invoke(this);
                }
                return;
            }

            switch (header.Type)
            {
                case OfpType.OFPT_HELLO:
                    Hello(ms, header);
                    break;
                case OfpType.OFPT_ERROR:
                    Error(ms, header);
                    break;
                case OfpType.OFPT_ECHO_REQUEST:
                    Echo(ms, header);
                    break;
                case OfpType.OFPT_ECHO_REPLY:
                    Echo(ms, header);
                    break;
                case OfpType.OFPT_VENDOR:
                    Vendor(ms, header);
                    break;
                case OfpType.OFPT_FEATURES_REQUEST:
                    Features(ms, header);
                    break;
                case OfpType.OFPT_FEATURES_REPLY:
                    Features(ms, header);
                    break;
                case OfpType.OFPT_GET_CONFIG_REQUEST:
                    SwitchConfig(ms, header);
                    break;
                case OfpType.OFPT_GET_CONFIG_REPLY:
                    SwitchConfig(ms, header);
                    break;
                case OfpType.OFPT_SET_CONFIG:
                    SwitchConfig(ms, header);
                    break;
                case OfpType.OFPT_PACKET_IN:
                    PacketIn(ms, header);
                    break;
                case OfpType.OFPT_FLOW_REMOVED:
                    FlowRemoved(ms, header);
                    break;
                case OfpType.OFPT_PORT_STATUS:
                    PortStatus(ms, header);
                    break;
                case OfpType.OFPT_PACKET_OUT:
                    PacketOut(ms, header);
                    break;
                case OfpType.OFPT_FLOW_MOD:
                    FlowMod(ms, header);
                    break;
                case OfpType.OFPT_PORT_MOD:
                    //NotImplemented
                    break;
                case OfpType.OFPT_STATS_REQUEST:
                    Stats(ms, header);
                    break;
                case OfpType.OFPT_STATS_REPLY:
                    Stats(ms, header);
                    break;
                case OfpType.OFPT_BARRIER_REQUEST:
                    Barrier(ms, header);
                    break;
                case OfpType.OFPT_BARRIER_REPLY:
                    Barrier(ms, header);
                    break;
                case OfpType.OFPT_QUEUE_GET_CONFIG_REQUEST:
                    QueueConfig(ms, header);
                    break;
                case OfpType.OFPT_QUEUE_GET_CONFIG_REPLY:
                    QueueConfig(ms, header);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ms.Close();
        }

        private void QueueConfig(MemoryStream ms, OfpHeader header)
        {
            OfpQueueGetConfig config = new OfpQueueGetConfig(ms, header);
            _controller.PluginSystem.QueueConfig(config, this);
        }

        private void Barrier(MemoryStream ms, OfpHeader header)
        {
            if (header.Type == OfpType.OFPT_BARRIER_REQUEST)
            {
                return;
            }
            OfpBarrier barrier = new OfpBarrier();
        }

        private void Stats(MemoryStream ms, OfpHeader header)
        {
            if (header.Type == OfpType.OFPT_STATS_REQUEST)
            {
                return;
            }
            else
            {
                OfpStatsReply reply = new OfpStatsReply(ms, header);
                MemoryStream bodyms = new MemoryStream(reply.Body);
                switch (reply.Type)
                {
                    case OfpStatsTypes.OFPST_DESC:
                        OfpDescStats stats1 = new OfpDescStats(bodyms);
                        _controller.PluginSystem.DescStats(stats1, this);
                        break;
                    case OfpStatsTypes.OFPST_FLOW:
                        OfpFlowStats stats2 = new OfpFlowStats(bodyms);
                        _controller.PluginSystem.FlowStats(stats2, this);
                        break;
                    case OfpStatsTypes.OFPST_AGGREGATE:
                        OfpAggregateStats stats3 = new OfpAggregateStats(bodyms);
                        _controller.PluginSystem.AggregateStats(stats3, this);
                        break;
                    case OfpStatsTypes.OFPST_TABLE:
                        OfpTableStats stats4 = new OfpTableStats(bodyms);
                        _controller.PluginSystem.TableStats(stats4, this);
                        break;
                    case OfpStatsTypes.OFPST_PORT:
                        OfpPortStats stats5 = new OfpPortStats(bodyms);
                        _controller.PluginSystem.PortStats(stats5, this);
                        break;
                    case OfpStatsTypes.OFPST_QUEUE:
                        OfpQueueStats stats6 = new OfpQueueStats(bodyms);
                        _controller.PluginSystem.QueueStats(stats6, this);
                        break;
                    case OfpStatsTypes.OFPST_VENDOR:
                        OfpVendorStats stats7 = new OfpVendorStats(bodyms,(int)ms.Length);
                        _controller.PluginSystem.VendorStats(stats7, this);
                        break;
                    default:
                        //throw new ArgumentOutOfRangeException();
                        break;
                }
                bodyms.Dispose();
            }
        }

        private void FlowMod(MemoryStream ms, OfpHeader header)
        {
            return;
        }

        private void PacketOut(MemoryStream ms, OfpHeader header)
        {
            return;
        }

        private void PortStatus(MemoryStream ms, OfpHeader header)
        {
            OfpPortStatus status = new OfpPortStatus(ms, header);
            _controller.PluginSystem.PortStatus(status, this);
        }

        private void FlowRemoved(MemoryStream ms, OfpHeader header)
        {
            OfpFlowRemoved removed = new OfpFlowRemoved(ms, header);
            _controller.PluginSystem.FlowRemoved(removed, this);
        }

        private void PacketIn(MemoryStream ms, OfpHeader header)
        {
            OfpPacketIn packetIn = new OfpPacketIn(ms, header);
            Packet packet = null;
            if (Math.Abs(packetIn.TotalLen - packetIn.Data.Length) <= 8)
            {
                try
                {
                    packet = Packet.ParsePacket(LinkLayers.Ethernet, packetIn.Data);
                }
                catch (Exception)
                {
                    packet = null;
                }
            }
            _controller.PluginSystem.PacketIn(packetIn, packet, this);
        }

        private void SwitchConfig(MemoryStream ms, OfpHeader header)
        {
            if (header.Type == OfpType.OFPT_GET_CONFIG_REQUEST)
            {
                return;
            }
            else if (header.Type == OfpType.OFPT_SET_CONFIG)
            {
                return;
            }
            else
            {
                OfpSwitchConfig config = new OfpSwitchConfig(ms, header);
                _controller.PluginSystem.SwitchConfig(config, this);
            }
        }

        private void Features(MemoryStream ms, OfpHeader header)
        {
            if (header.Type == OfpType.OFPT_FEATURES_REQUEST)
            {
                return;
            }
            else
            {
                OfpSwitchFeatures features = new OfpSwitchFeatures(ms, header);
                _controller.PluginSystem.SwitchFeatures(features, this);
            }
        }

        private void Vendor(MemoryStream ms, OfpHeader header)
        {
            OfpVendorHeader vendor = new OfpVendorHeader(ms, header);
            _controller.PluginSystem.Vendor(vendor, this);
        }

        private void Echo(MemoryStream ms, OfpHeader header)
        {
            OfpEcho echo = new OfpEcho(ms, header);
            if (echo.Header.Type == OfpType.OFPT_ECHO_REQUEST)
            {
                Write(new OfpEcho(true).ToByteArray());
            }
            _controller.PluginSystem.Echo(echo, this);
        }

        private void Error(MemoryStream ms, OfpHeader header)
        {
            OfpErrorMsg error = new OfpErrorMsg(ms, header);
            Debug.WriteLine($"[{_mac}] ERROR:{error.Type} - {error.GetErrorCode()}");
            _log.Info($"[{_mac}] ERROR:{error.Type} - {error.GetErrorCode()}");
            //Console.WriteLine($"[{_mac}] ERROR:{error.Type} - {error.GetErrorCode()}");
            _controller.PluginSystem.Error(error, this);
        }

        private void Hello(MemoryStream ms, OfpHeader header)
        {
            OfpHello hello = new OfpHello(ms, header);
            _controller.PluginSystem.Hello(hello, this);
        }

        private void Initialize(MemoryStream ms, OfpHeader header)
        {
            if (!_versionChecked)
            {
                _version = header.Version;
                _versionChecked = true;
                var reply = new OfpSwitchFeaturesRequest();
                _log.Debug("Request for features");
                //Console.WriteLine("Request for features");
                Write(reply.ToByteArray());
            }
            else if (header.Type == OfpType.OFPT_FEATURES_REPLY)
            {
                OfpSwitchFeatures features = new OfpSwitchFeatures(ms, header);
                _mac = features.DatapathId.GetPhysicalAddress();
                _log.Info($"A switch connected - MAC={_mac}");
                //Console.WriteLine($"A switch connected - MAC={_mac}");
                foreach (var ofpPhyPort in features.Ports)
                {
                    //Console.WriteLine($"\tPortNum={ofpPhyPort.PortNo} \tPortName={ofpPhyPort.Name} \tPortState={ofpPhyPort.State.ToString()}");
                }
                SwitchFeatures = features;
                _featureChecked = true;
                var reply = new OfpSwitchConfig(true);
                reply.Flags = OfpConfigFlags.OFPC_FRAG_NORMAL;
                reply.MissSendLen = 65535;
                Write(reply.ToByteArray());
                var reply2 = new OfpSwitchConfigRequest();
                Write(reply2.ToByteArray());
            }
            else if (header.Type == OfpType.OFPT_GET_CONFIG_REPLY)
            {
                OfpSwitchConfig config = new OfpSwitchConfig(ms, header);
                _log.Info($"[{_mac}] Config: Flag={config.Flags} MissSendLen={config.MissSendLen}");
                //Console.WriteLine($"[{_mac}] Config: Flag={config.Flags} MissSendLen={config.MissSendLen}");
                _configChecked = true;
                return;
            }
            else if (header.Type == OfpType.OFPT_ECHO_REQUEST)
            {
                OfpEcho echo = new OfpEcho(ms, header);
                _version = echo.Header.Version;
                _versionChecked = true;
                var reply = new OfpEcho(true);
                reply.Data = echo.Data;
                Write(reply.ToByteArray());
            }
            else if (header.Type == OfpType.OFPT_ERROR)
            {
                OfpErrorMsg error = new OfpErrorMsg(ms, header);
                _log.Info($"[{_mac}] Error: type={error.Type.ToString()} code={error.GetErrorCode()}");
                //Console.WriteLine($"[{_mac}] Error: type={error.Type.ToString()} code={error.GetErrorCode()}");
                return;
            }

            else
            {
                return;
            }
        }

        private async Task OnMessageComplete(IOfpMessage reply)
        {
        }

        private OfpHeader ParesHeader(byte[] stream)
        {
            OfpHeader header;
            try
            {
                header = new OfpHeader(stream);
            }
            catch (Exception)
            {
                header = null;
                //throw;
            }
            return header;
        }

        private OfpHeader ParesHeader(Stream stream)
        {
            var pos = stream.Position;
            OfpHeader header;
            try
            {
                header = new OfpHeader(stream);
            }
            catch (Exception)
            {
                header = null;
                //throw;
            }
            stream.Position = pos;
            return header;
        }

        protected override byte[] HandleMessage(byte[] message)
        {
            return null;
        }

        protected override void OnConnected()
        {
            _controller = (OfController) this.CurrentServer;
            var reply = new OfpHello();
            reply.Header.Version = MaxSupportedVersion;
            Write(reply.ToByteArray());
            //ProcessMessage();
            Task.Run(() => IoLoop());
        }

        private void IoLoop()
        {
            while (ControlClient.Connected)
            {
                ProcessMessage().Wait();
            }
            Dispose();
        }

        private async Task ProcessMessage()
        {
            var head = await ReadAsync(8);
            if (head == null)
            {
                return;
            }
            var header = ParesHeader(head);
            if (header == null)
            {
                //ProcessMessage().Wait();
                return;
            }

            //Debug.WriteLine($"{header.Type} {header.Length}");

            byte[] content = new byte[0];

            if (header.Length > 8)
            {
                content = await ReadAsync((int) (header.Length - OfpHeader.Size));
            }

            HandleMessage(content, header);

            //Debug.WriteLine("content get");

            //var reply = HandleMessage(content, header);

            //await WriteAsync(reply?.ToByteArray());

            //Debug.WriteLine("reply sent");

            //await OnMessageComplete(reply);

            //ProcessMessage().Wait();
        }
    }
}
