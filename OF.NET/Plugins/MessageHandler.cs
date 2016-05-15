using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;

namespace FlowNet.Plugins
{
    public abstract class MessageHandler
    {
        /// <summary>
        /// 处理Hello
        /// </summary>
        /// <param name="hello"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Hello(OfpHello hello, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理错误
        /// </summary>
        /// <param name="error"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Error(OfpErrorMsg error, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理Echo
        /// </summary>
        /// <param name="echo"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Echo(OfpEcho echo, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理Vendor
        /// </summary>
        /// <param name="vendor"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Vendor(OfpVendorHeader vendor, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理交换机功能
        /// </summary>
        /// <param name="features"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool SwitchFeatures(OfpSwitchFeatures features, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理交换机配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool SwitchConfig(OfpSwitchConfig config, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理包进入消息
        /// </summary>
        /// <param name="packetIn"></param>
        /// <param name="packet"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool PacketIn(OfpPacketIn packetIn, object packet, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理流删除消息
        /// </summary>
        /// <param name="removed"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool FlowRemoved(OfpFlowRemoved removed, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理端口状态消息
        /// </summary>
        /// <param name="status"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool PortStatus(OfpPortStatus status, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理Barrier消息
        /// </summary>
        /// <param name="barrier"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Barrier(OfpBarrier barrier, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理队列配置消息
        /// </summary>
        /// <param name="queueConfig"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool QueueConfig(OfpQueueGetConfig queueConfig, IConnection handler)
        {
            return false;
        }

        #region 统计消息

        /// <summary>
        /// 处理描述统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool DescStats(OfpDescStats stats, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理流统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool FlowStats(OfpFlowStats stats, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理多流统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool AggregateStats(OfpAggregateStats stats, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理表统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool TableStats(OfpTableStats stats, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理端口统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool PortStats(OfpPortStats stats, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理队列统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool QueueStats(OfpQueueStats stats, IConnection handler)
        {
            return false;
        }

        /// <summary>
        /// 处理生产商统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool VendorStats(OfpVendorStats stats, IConnection handler)
        {
            return false;
        }
#endregion

    }
}
