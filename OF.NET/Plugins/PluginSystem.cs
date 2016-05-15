using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FlowNet.OpenFlow.OFP1_0;

namespace FlowNet.Plugins
{
    internal class PluginSystem : MessageHandler
    {
        private CompositionContainer _container;
        private OfController _controller;

        [ImportMany]
        private IEnumerable<Lazy<IPlugin, IPlugInfo>> _plugins;

        public OrderedDictionary<string, IPlugin> Plugins = new OrderedDictionary<string, IPlugin>();

        //public PluginHandler Handler = new PluginHandler(null);

        public PluginSystem(OfController controller)
        {
            _controller = controller;
        }

        private void UpdatePluginsDictionary()
        {
            Plugins = new OrderedDictionary<string, IPlugin>();
            var ps = from lzPlugin in _plugins
                     orderby lzPlugin.Value.Priority descending
                     select lzPlugin;
            foreach (var p in ps)
            {
                if (!Plugins.ContainsKey(p.Metadata.Name))
                {
                    Plugins.Add(p.Metadata.Name, p.Value);
                }
            }
            foreach (var plugin in Plugins.Values)
            {
                plugin.Init(_controller);
            }
        }

        public IPlugInfo GetPlugInfo(string name)
        {
            return (from plugin in _plugins where plugin.Metadata.Name == name select plugin.Metadata).FirstOrDefault();
        }

        public void Init(string path)
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            AddCatalog(path, catalog);

            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);
            //_container.ReleaseExport();
            //Fill the imports of this object
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Debug.WriteLine(compositionException.ToString());
            }
            UpdatePluginsDictionary();
        }

        private void AddCatalog(string path, AggregateCatalog catalog)
        {
            if (path.IsDirectory())
            {
                catalog.Catalogs.Add(new DirectoryCatalog(path));
            }
            else
            {
                catalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFile(path)));
            }
        }

        public override bool Hello(OfpHello hello, IConnection handler)
        {
            foreach (var plugin in Plugins.Values)
            {
                if (plugin.Active)
                {
                    try
                    {
                        bool result = plugin.MessageHandler.Hello(hello, handler);
                        if (result)
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            return true;
        }

        public override bool Error(OfpErrorMsg error, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.Error(error, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理Echo
        /// </summary>
        /// <param name="echo"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Echo(OfpEcho echo, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.Echo(echo, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理Vendor
        /// </summary>
        /// <param name="vendor"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Vendor(OfpVendorHeader vendor, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.Vendor(vendor, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理交换机功能
        /// </summary>
        /// <param name="features"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool SwitchFeatures(OfpSwitchFeatures features, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.SwitchFeatures(features, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理交换机配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool SwitchConfig(OfpSwitchConfig config, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.SwitchConfig(config, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理包进入消息
        /// </summary>
        /// <param name="packetIn"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool PacketIn(OfpPacketIn packetIn, object packet, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.PacketIn(packetIn, packet, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理流删除消息
        /// </summary>
        /// <param name="removed"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool FlowRemoved(OfpFlowRemoved removed, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.FlowRemoved(removed, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理端口状态消息
        /// </summary>
        /// <param name="status"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool PortStatus(OfpPortStatus status, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.PortStatus(status, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理Barrier消息
        /// </summary>
        /// <param name="barrier"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool Barrier(OfpBarrier barrier, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.Barrier(barrier, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理队列配置消息
        /// </summary>
        /// <param name="queueConfig"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool QueueConfig(OfpQueueGetConfig queueConfig, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.QueueConfig(queueConfig, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
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
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.DescStats(stats, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理流统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool FlowStats(OfpFlowStats stats, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.FlowStats(stats, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理多流统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool AggregateStats(OfpAggregateStats stats, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.AggregateStats(stats, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理表统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool TableStats(OfpTableStats stats, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.TableStats(stats, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理端口统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool PortStats(OfpPortStats stats, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.PortStats(stats, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理队列统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool QueueStats(OfpQueueStats stats, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.QueueStats(stats, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }

        /// <summary>
        /// 处理生产商统计消息
        /// </summary>
        /// <param name="stats"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public virtual bool VendorStats(OfpVendorStats stats, IConnection handler)
        {
            foreach (var plugin in Plugins.Values.Where(plugin => plugin.Active))
            {
                try
                {
                    bool result = plugin.MessageHandler.VendorStats(stats, handler);
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return true;
        }
        #endregion
    }

    //internal class PluginHandler : MessageHandler
    //{
    //    public PluginHandler(IHandler handler) : base(handler)
    //    {
    //    }

    //    public override bool Hello(OfpHello hello)
    //    {
    //    }
    //}
}
