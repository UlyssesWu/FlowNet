using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;

namespace FlowNet
{
    /// <summary>
    /// 暴露给插件的交换机连接接口
    /// </summary>
    public interface IConnection
    {
        event ClientConnectionHandler OnConnectionReady;
        PhysicalAddress Mac { get; }
        OfpSwitchFeatures SwitchFeatures { get; }
        Task<byte[]> ReadAsync(int length);
        Task WriteAsync(byte[] content);
        void Write(byte[] content);
    }
}
