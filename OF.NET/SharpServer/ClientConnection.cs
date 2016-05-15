using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SharpServer
{
    public abstract class ClientConnection : ClientConnectionBase, IDisposable
    {
        public event EventHandler<EventArgs> Disposed;

        /// <summary>
        /// 所属服务器
        /// </summary>
        public object CurrentServer { get; set; } // MARK:尝试建立Connection与Server的联系
        /// <summary>
        /// 编号
        /// </summary>
        public ulong ID = 0;

        public bool ShouldQuit = false;

        #region Private Fields

        private bool _disposed = false;
        private byte[] _buffer = new byte[1516];
        private Encoding _controlStreamEncoding = Encoding.ASCII; //MARK: use ASCII

        #endregion
        
        /// <summary>
        /// 从control connection stream中开始异步读.
        /// </summary>
        protected virtual void Read()
        {
            Read(ControlStream);
        }

        /// <summary>
        /// 异步读
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public virtual async Task<byte[]> ReadAsync(int length)
        {
            return await ReadAsync(ControlStream, length);
        }

        public virtual async Task WriteAsync(byte[] content)
        {
            if (content == null)
            {
                return;
            }
            var stream = ControlStream;
            if (_disposed || !stream.CanWrite)
            {
                Dispose();
                return;
            }

            //_log.Debug(content);

            try
            {
                await stream.WriteAsync(content, 0, content.Length);
                //stream.BeginWrite(content, 0, content.Length, WriteCallback, stream);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                Dispose();
            }
        }

        /// <summary>
        /// Begins an asynchronous read from the provided <paramref name="stream"/>.
        /// <para>从提供的流<paramref name="stream"/>中开始异步读。</para>
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        public virtual void Read(Stream stream)
        {
            if (_disposed || !stream.CanRead)
            {
                Dispose();
                return;
            }

            try
            {
                stream.BeginRead(_buffer, 0, _buffer.Length, ReadCallback, stream);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                Dispose();
            }
        }

        /// <summary>
        /// Begins an asynchronous read from the provided <paramref name="stream"/>.
        /// <para>从提供的流<paramref name="stream"/>中开始异步读。</para>
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        protected virtual async Task<byte[]> ReadAsync(Stream stream, int length)
        {
            if (_disposed || !stream.CanRead)
            {
                Dispose();
                return null;
            }

            try
            {
                var buffer = new byte[length];
                int count = await stream.ReadAsync(buffer, 0, length);
                if (count == 0)
                {
                    Debug.WriteLine("count = 0");
                    // End read returns 0 bytes if the socket closed...
                    Dispose();
                    return null;
                }
                return buffer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _log.Error(ex);
                Dispose();
            }
            return null;
        }

        /// <summary>
        /// Asynchronously writes <paramref name="content"/> to the control connection stream.
        /// </summary>
        /// <param name="content">The text to write.</param>
        public void Write(byte[] content)
        {
            Write(ControlStream, content);
        }

        /// <summary>
        /// Asynchronously writes <paramref name="content"/> to the provided <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="content">The content to write.</param>
        protected virtual void Write(Stream stream, byte[] content)
        {
            if (_disposed || !stream.CanWrite)
            {
                Dispose();
                return;
            }

            //_log.Debug(content);

            try
            {
                stream.Write(content,0,content.Length);
                //stream.BeginWrite(content, 0, content.Length, WriteCallback, stream);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                Dispose();
            }
        }

        /// <summary>
        /// 初始化Connection，专用于服务一个成功连接的TcpClient
        /// <para>Sets up the class to handle the communication to the given TcpClient.</para>
        /// </summary>
        /// <param name="client">The TcpClient to communicate with.</param>
        public async override void HandleClient(object obj)
        {
            TcpClient client = obj as TcpClient;

            ControlClient = client;

            RemoteEndPoint = (IPEndPoint)ControlClient.Client.RemoteEndPoint;

            ClientIP = RemoteEndPoint.Address.ToString();

            ControlStream = ControlClient.GetStream();
            
            OnConnected();
        }

        protected virtual void OnDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (ControlClient != null)
                    {
                        ControlClient.Close();
                    }

                    if (ControlStream != null)
                    {
                        ControlStream.Close();
                    }
                }
            }

            _disposed = true;
            OnDisposed();
        }

        // TODO: Make CopyStream async.
/*
        protected virtual long CopyStream(Stream input, Stream output, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int count = 0;
            long total = 0;

            while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, count);
                total += count;
            }

            return total;
        }
*/
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Private Methods

        private void WriteCallback(IAsyncResult result)
        {
            if (result == null)
            {
                Dispose();
                return;
            }

            Stream stream = (Stream)result.AsyncState;

            if (_disposed || !stream.CanWrite)
            {
                Dispose();
                return;
            }

            try
            {
                stream.EndWrite(result);
            }
            catch (IOException ex)
            {
                _log.Error(ex);
                Dispose();
            }
        }

        private void ReadCallback(IAsyncResult result)
        {
            if (result == null)
            {
                Dispose();
                return;
            }

            Stream stream = result.AsyncState as Stream;

            if (_disposed || !stream.CanRead)
            {
                Dispose();
                return;
            }

            int bytesRead = 0;

            try
            {
                bytesRead = stream.EndRead(result);
            }
            catch (IOException ex)
            {
                _log.Error(ex);
            }

            // End read returns 0 bytes if the socket closed...
            if (bytesRead == 0)
            {
                Dispose();
                return;
            }

            object state = null;
            byte[] r = HandleMessage(_buffer);

            if (ControlClient != null && ControlClient.Connected)
            {
                if (r != null)  //FIXED:可以不响应
                {
                    Write(r);
                }

                if (ShouldQuit)
                {
                    Dispose();
                    return;
                }

                OnMessageComplete(state);

                r = null;

                Read();
            }
        }

        #endregion
    }
}
