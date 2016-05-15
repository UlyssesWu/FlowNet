using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet.Plugins
{
    public interface IPlugin: IDisposable
    {
        bool Active { get; set; }
        int Priority { get; set; }

        void Init(IController controller);
        MessageHandler MessageHandler { get; }
    }

    public interface IPlugInfo
    {
        string Name { get; }
        string Description { get; }
    }
}
