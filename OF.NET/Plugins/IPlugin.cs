using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet.Plugins
{
    public interface IPlugin
    {
        bool Active { get; set; }
        void Init(IController controller);
        MessageHandler MessageHandler { get; }
    }

    public interface IPlugInfo
    {
        string Name { get; }
        int Priority { get; }
        string Description { get; }
    }
}
