using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpServer;
using FlowNet.OpenFlow.OFP1_0;
using static FlowNet.OpenFlow.OFP1_0.Data; //TODO: Make a universial data layer

namespace FlowNet
{
    public class OfClientConnection : ClientConnection, IConnection
    {
        private OfController _controller;
        private bool _inited = false;
        private bool _versionChecked = false;
        private bool _featureChecked = false;
        private bool _configChecked = false;
        private bool _stateChecked = false;
        private int _version = 1;

        public byte MaxSupportedVersion { get { return 1; } }

        protected void HandleMessage(byte[] message, OfpHeader header)
        {
            //state = null;
            MemoryStream ms = new MemoryStream(message);
            //var header = ParesHeader(ms);
            if (header == null)
            {
                return;
            }

            Console.WriteLine(header.Type.ToString());

            if (!_inited)
            {
                Initialize(ms, header);
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
                    break;
                case OfpType.OFPT_SET_CONFIG:
                    break;
                case OfpType.OFPT_PACKET_IN:
                    break;
                case OfpType.OFPT_FLOW_REMOVED:
                    break;
                case OfpType.OFPT_PORT_STATUS:
                    break;
                case OfpType.OFPT_PACKET_OUT:
                    break;
                case OfpType.OFPT_FLOW_MOD:
                    break;
                case OfpType.PFPT_PORT_MOD:
                    break;
                case OfpType.OFPT_STATS_REQUEST:
                    break;
                case OfpType.OFPT_STATS_REPLY:
                    break;
                case OfpType.OFPT_BARRIER_REQUEST:
                    break;
                case OfpType.OFPT_BARRIER_REPLY:
                    break;
                case OfpType.OFPT_QUEUE_GET_CONFIG_REQUEST:
                    break;
                case OfpType.OFPT_QUEUE_GET_CONFIG_REPLY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ms.Close();
        }

        private void SwitchConfig(MemoryStream ms, OfpHeader header)
        {
            if (header.Type == OfpType.OFPT_GET_CONFIG_REQUEST)
            {
                return;
            }
            else if(header.Type == OfpType.OFPT_SET_CONFIG)
            {
                return;
            }
            else
            {
                OfpSwitchConfig config = new OfpSwitchConfig(ms,header);
                _controller.PluginSystem.SwitchConfig(config, this);
            }
        }

        private void Features(MemoryStream ms, OfpHeader header)
        {
            if (header.Type == OfpType.OFPT_FEATURES_REQUEST)
            {
                
            }
            else
            {
                OfpSwitchFeatures features = new OfpSwitchFeatures(ms,header);
                _controller.PluginSystem.SwitchFeatures(features, this);
            }
        }

        private void Vendor(MemoryStream ms, OfpHeader header)
        {
            OfpVendorHeader vendor = new OfpVendorHeader(ms,header);
            _controller.PluginSystem.Vendor(vendor, this);
        }

        private void Echo(MemoryStream ms, OfpHeader header)
        {
            OfpEcho echo = new OfpEcho(ms,header);
            if (echo.Header.Type == OfpType.OFPT_ECHO_REQUEST)
            {
                Write(new OfpEcho(true).ToByteArray());
            }
            _controller.PluginSystem.Echo(echo, this);
        }

        private void Error(MemoryStream ms, OfpHeader header)
        {
            OfpErrorMsg error = new OfpErrorMsg(ms,header);
            _controller.PluginSystem.Error(error, this);
        }

        private void Hello(MemoryStream ms, OfpHeader header)
        {
            OfpHello hello = new OfpHello(ms,header);
            _controller.PluginSystem.Hello(hello, this);
        }

        private void Initialize(MemoryStream ms, OfpHeader header)
        {
            if (!_versionChecked)
            {
                _version = header.Version;
                _versionChecked = true;
                var reply = new OfpSwitchFeaturesRequest();
                Write(reply.ToByteArray());
            }
            else if (header.Type == OfpType.OFPT_FEATURES_REPLY)
            {
                OfpSwitchFeatures features = new OfpSwitchFeatures(ms,header);
                Console.WriteLine(features.Actions);
                Console.WriteLine(features.Capabilities);
                Console.WriteLine(features.DatapathId);
                foreach (var ofpPhyPort in features.Ports)
                {
                    Console.WriteLine($"{ofpPhyPort.PortNo} {ofpPhyPort.Name} {ofpPhyPort.State.ToString()}");
                }
                _featureChecked = true;
                var reply = new OfpSwitchConfig(true);
                reply.Flags = OfpConfigFlags.OFPC_FRAG_NORMAL;
                reply.MissSendLen = 65535;
                Write(reply.ToByteArray());
            }
            else if (header.Type == OfpType.OFPT_GET_CONFIG_REPLY)
            {
                OfpSwitchConfig config = new OfpSwitchConfig(ms,header);
                return;
            }
            else if(header.Type == OfpType.OFPT_ECHO_REQUEST)
            {
                OfpEcho echo = new OfpEcho(ms,header);
                _version = echo.Header.Version;
                _versionChecked = true;
                var reply = new OfpEcho(true);
                reply.Data = echo.Data;
                Write(reply.ToByteArray());
            }
            else if(header.Type == OfpType.OFPT_ERROR)
            {
                OfpErrorMsg error = new OfpErrorMsg(ms, header);
                Console.WriteLine($"{error.Type.ToString()} {error.GetErrorCode()}");
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
            IoLoop();
        }

        private async void IoLoop()
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
                content = await ReadAsync((int)(header.Length - OfpHeader.Size));
            }

            //Debug.WriteLine("content get");

            //var reply = HandleMessage(content, header);

            //await WriteAsync(reply?.ToByteArray());

            //Debug.WriteLine("reply sent");

            //await OnMessageComplete(reply);

            //ProcessMessage().Wait();
        }

    }
}
