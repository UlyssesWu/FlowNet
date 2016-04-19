using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet.Plugins
{
    static class Helper
    {
        public static bool IsDirectory(this string path)
        {
            return Directory.Exists(path);
        }
    }
}
