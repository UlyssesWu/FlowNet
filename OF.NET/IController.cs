using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowNet.Plugins;
using FlowNet.Topology;

namespace FlowNet
{
    /// <summary>
    /// 暴露给插件的控制器接口
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// 交换机连接事件
        /// </summary>
        event ClientConnectionHandler OnClientConnected;
        /// <summary>
        /// 交换机断开连接事件
        /// </summary>
        event ClientConnectionHandler OnClientDisconnecting;

        /// <summary>
        /// 变量字典
        /// </summary>
        IDictionary<string, object> Variables { get; }
        /// <summary>
        /// 拓扑
        /// </summary>
        Topo Topo { get; }
        /// <summary>
        /// 插件
        /// </summary>
        IDictionary<string,IPlugin> Plugins { get; }

        void LogInfo(string message);

        void LogError(string error);

        void LogDebug(string debug);

    }
}
