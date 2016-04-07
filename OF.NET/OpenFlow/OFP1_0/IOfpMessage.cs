using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static FlowNet.OpenFlow.OFP1_0.Data;

namespace FlowNet.OpenFlow.OFP1_0
{
    /// <summary>
    /// 消息
    /// </summary>
    public interface IOfpMessage : IToByteArray
    {
        /// <summary>
        /// OF头
        /// </summary>
        OfpHeader Header { get; }
    }
}
