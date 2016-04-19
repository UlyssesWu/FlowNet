using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using log4net;


namespace SharpServer
{
    public abstract class ClientConnectionBase
    {
        protected ILog _log = LogManager.GetLogger(typeof(ClientConnectionBase));

        protected TcpClient ControlClient { get; set; }
        /// <summary>
        /// 控制流
        /// </summary>
        protected NetworkStream ControlStream { get; set; }
        /// <summary>
        /// 远端地址（IP:端口号）
        /// </summary>
        protected IPEndPoint RemoteEndPoint { get; set; }
        /// <summary>
        /// 客户端IP
        /// </summary>
        protected string ClientIP { get; set; }

        //public abstract void Write(byte[] content);

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected abstract byte[] HandleMessage(byte[] message);

        public abstract void HandleClient(object obj);

        protected virtual void OnConnected()
        {
        }

        protected virtual void OnMessageComplete(object state)
        {
        }

        protected virtual long CopyStream(Stream input, Stream output, int bufferSize, Action<int> performanceCounterAction)
        {
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            long total = 0;
            //BUG? May cause WSACancelBlockingCall
            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
                performanceCounterAction(count);
            }

            return total;
        }

        //    /// <summary>
        //    /// 流传输
        //    /// </summary>
        //    /// <param name="input"></param>
        //    /// <param name="output"></param>
        //    /// <param name="bufferSize"></param>
        //    /// <param name="encoding">编码方式</param>
        //    /// <param name="performanceCounterAction">计数器操作</param>
        //    /// <returns></returns>
        //    protected virtual long CopyStream(Stream input, Stream output, int bufferSize, Encoding encoding, Action<int> performanceCounterAction)
        //    {
        //        char[] buffer = new char[bufferSize];
        //        int count = 0;
        //        long total = 0;

        //        using (StreamReader rdr = new StreamReader(input))
        //        {
        //            using (StreamWriter wtr = new StreamWriter(output, encoding))
        //            {
        //                while ((count = rdr.Read(buffer, 0, buffer.Length)) > 0)
        //                {
        //                    wtr.Write(buffer, 0, count);
        //                    total += count;
        //                    performanceCounterAction(count);
        //                }
        //            }
        //        }

        //        return total;
        //    }
    }
}
