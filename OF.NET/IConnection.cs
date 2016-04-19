using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowNet
{
    public interface IConnection
    {
        Task<byte[]> ReadAsync(int length);
        Task WriteAsync(byte[] content);
        void Write(byte[] content);
    }
}
