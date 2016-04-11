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
    public class OfClientConnection : ClientConnection
    {
        private bool _inited = false;
        private bool _versionChecked = false;
        private bool _featureChecked = false;
        private bool _configChecked = false;
        private bool _stateChecked = false;
        public bool _topoChecked = false;
        private int _version = 1;

        public byte MaxSupportedVersion { get { return 1; } }

        protected IOfpMessage HandleMessage(byte[] message, OfpHeader header)
        {
            //state = null;
            MemoryStream ms = new MemoryStream(message);
            //var header = ParesHeader(ms);
            if (header == null)
            {
                return null;
            }

            Console.WriteLine(header.Type.ToString());

            if (!_inited)
            {
                return Initialize(ms, header);
            }
            switch (header.Type)
            {
                case OfpType.OFPT_HELLO:
                    return Hello(ms, header);
                    break;
                case OfpType.OFPT_ERROR:
                    return Error(ms, header);
                    break;
                case OfpType.OFPT_ECHO_REQUEST:
                    break;
                case OfpType.OFPT_ECHO_REPLY:
                    break;
                case OfpType.OFPT_VENDOR:
                    break;
                case OfpType.OFPT_FEATURES_REQUEST:
                    break;
                case OfpType.OFPT_FEATURES_REPLY:
                    break;
                case OfpType.OFPT_GET_CONFIG_REQUEST:
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
            var reply = new OfpHello();
            reply.Header.Version = MaxSupportedVersion;
            return reply;
        }

        private IOfpMessage Error(MemoryStream ms, OfpHeader header)
        {
            throw new NotImplementedException();
        }

        private IOfpMessage Hello(MemoryStream ms, OfpHeader header)
        {
            var reply = new OfpSwitchFeaturesRequest();
            return reply;
        }

        private IOfpMessage Initialize(MemoryStream ms, OfpHeader header)
        {
            if (!_versionChecked)
            {
                _version = header.Version;
                _versionChecked = true;
                var reply = new OfpSwitchFeaturesRequest();
                return reply;
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
                return reply;
            }
            else if (header.Type == OfpType.OFPT_GET_CONFIG_REPLY)
            {
                OfpSwitchConfig config = new OfpSwitchConfig(ms,header);
                return null;
            }
            else if(header.Type == OfpType.OFPT_ECHO_REQUEST)
            {
                OfpEcho echo = new OfpEcho(ms,header);
                _version = echo.Header.Version;
                _versionChecked = true;
                var reply = new OfpEcho(true);
                reply.Data = echo.Data;
                return reply;
            }
            else if(header.Type == OfpType.OFPT_ERROR)
            {
                OfpErrorMsg error = new OfpErrorMsg(ms, header);
                Console.WriteLine($"{error.Type.ToString()} {error.GetErrorCode()}");
                return null;
            }

            else
            {
                return null;
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

            var reply = HandleMessage(content, header);

            await WriteAsync(reply?.ToByteArray());

            //Debug.WriteLine("reply sent");

            await OnMessageComplete(reply);

            //ProcessMessage().Wait();
        }

        //protected override void OnMessageComplete(object state)
        //{
        //}
        //protected override byte[] HandleMessage(byte[] message,out object state)
        //{
        //    state = null;
        //    MemoryStream ms = new MemoryStream(message);
        //    var header = ParesHeader(ms);
        //    if (header == null)
        //    {
        //        return null;
        //    }
        //    Console.WriteLine(header.Type.ToString());
        //    if (!_inited)
        //    {
        //        return Initialize(ms, header, ref state);
        //    }
        //    switch (header.Type)
        //    {
        //        case OfpType.OFPT_HELLO:
        //            return Hello(ms, header);
        //            break;
        //        case OfpType.OFPT_ERROR:
        //            return Error(ms, header);
        //            break;
        //        case OfpType.OFPT_ECHO_REQUEST:
        //            break;
        //        case OfpType.OFPT_ECHO_REPLY:
        //            break;
        //        case OfpType.OFPT_VENDOR:
        //            break;
        //        case OfpType.OFPT_FEATURES_REQUEST:
        //            break;
        //        case OfpType.OFPT_FEATURES_REPLY:
        //            break;
        //        case OfpType.OFPT_GET_CONFIG_REQUEST:
        //            break;
        //        case OfpType.OFPT_GET_CONFIG_REPLY:
        //            break;
        //        case OfpType.OFPT_SET_CONFIG:
        //            break;
        //        case OfpType.OFPT_PACKET_IN:
        //            break;
        //        case OfpType.OFPT_FLOW_REMOVED:
        //            break;
        //        case OfpType.OFPT_PORT_STATUS:
        //            break;
        //        case OfpType.OFPT_PACKET_OUT:
        //            break;
        //        case OfpType.OFPT_FLOW_MOD:
        //            break;
        //        case OfpType.PFPT_PORT_MOD:
        //            break;
        //        case OfpType.OFPT_STATS_REQUEST:
        //            break;
        //        case OfpType.OFPT_STATS_REPLY:
        //            break;
        //        case OfpType.OFPT_BARRIER_REQUEST:
        //            break;
        //        case OfpType.OFPT_BARRIER_REPLY:
        //            break;
        //        case OfpType.OFPT_QUEUE_GET_CONFIG_REQUEST:
        //            break;
        //        case OfpType.OFPT_QUEUE_GET_CONFIG_REPLY:
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //    ms.Close();
        //    var reply = new OfpHello();
        //    reply.Header.Version = MaxSupportedVersion;
        //    return reply.ToByteArray();
        //}
    }
}
