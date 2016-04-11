using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace SharpServer
{
    /// <summary>
    /// 通用Server
    /// </summary>
    /// <typeparam name="T">特定客户端连接方式</typeparam>
    public class Server<T> : IDisposable where T : ClientConnection, new()
    {
        private static readonly object _listLock = new object();

        private ILog _log = LogManager.GetLogger(typeof(Server<T>));

        private List<T> _state;

        protected List<TcpListener> Listeners;
        protected List<T> Connections {
            get { return _state; }
        }

        private bool _disposed = false;
        private bool _disposing = false;
        private bool _listening = false;
        private List<IPEndPoint> _localEndPoints;
        private string _logHeader;
        private ulong _connectId = 0;

        public Server(int port, string logHeader = null)
            : this(IPAddress.Any, port, logHeader)
        {
        }

        public Server(IPAddress ipAddress, int port, string logHeader = null)
            : this(new IPEndPoint[] { new IPEndPoint(ipAddress, port) }, logHeader)
        {
        }

        public Server(IPEndPoint[] localEndPoints, string logHeader = null)
        {
            _localEndPoints = new List<IPEndPoint>(localEndPoints);
            _logHeader = logHeader??"";
        }

        /// <summary>
        /// 启动服务器
        /// <exception cref="Exception">端口无法使用</exception>
        /// </summary>
        public void Start()
        {
            if (_disposed)
                throw new ObjectDisposedException("AsyncServer");

            _log.Info("#" + _logHeader + DateTime.Now);
            _state = new List<T>();
            Listeners = new List<TcpListener>();

            foreach (var localEndPoint in _localEndPoints)
            {
                if (localEndPoint == null)
                {
                    continue;   //ADDED:BUG found in Mono
                }
                TcpListener listener = new TcpListener(localEndPoint);

                try
                {
                    listener.Start();
                    _log.Info("Listening "+ listener.LocalEndpoint);
                }
                catch (SocketException ex)
                {
                    Dispose();
                    var exp = new Exception("The current local end point is currently in use. Please specify another IP or port to listen on.");
                    _log.Error(exp);
                    throw exp;
                }
                //开始异步等待连接
                listener.BeginAcceptTcpClient(HandleAcceptTcpClient, listener);

                Listeners.Add(listener);
            }

            _listening = true;

            OnStart();
        }

        public void Stop()
        {
            _log.Info("# Stopping Server");
            _listening = false;
            try
            {
                foreach (var listener in Listeners)
                {
                    listener.Stop();
                }
            }
            catch (Exception)
            {
                //throw;
            }

            try
            {
                Listeners.Clear();
                Connections.ForEach(t => t.Dispose());
            }
            catch (Exception)
            {
                //throw;
            }

            OnStop();
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnConnectAttempt()
        {
        }

        private void HandleAcceptTcpClient(IAsyncResult result)
        {
            //一个客户成功连接
            OnConnectAttempt();

            TcpListener listener = result.AsyncState as TcpListener;

            if (_listening)
            {
                TcpClient client;
                try
                {
                    //立即再次新开监听
                    listener.BeginAcceptTcpClient(HandleAcceptTcpClient, listener);
                    //对本次的连接结果创建TcpClient处理
                    client = listener.EndAcceptTcpClient(result);

                    _connectId++;

                    var connection = new T { CurrentServer = this,ID = _connectId };

                    //Debug.WriteLine($"{_connectId} connected");

                    connection.Disposed += new EventHandler<EventArgs>(AsyncClientConnection_Disposed);

                    connection.HandleClient(client);

                    lock (_listLock)
                        _state.Add(connection);
                }
                catch (SocketException e)
                {
                    //throw;
                }
            }
        }

        private void AsyncClientConnection_Disposed(object sender, EventArgs e)
        {
            // Prevent removing if we are disposing of this object. The list will be cleaned up in Dispose(bool).
            if (!_disposing)
            {
                T connection = (T)sender;

                lock (_listLock)
                    _state.Remove(connection);
            }
        }

        public void Dispose()
        {
            _disposing = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _disposing = true;

            if (!_disposed)
            {
                if (disposing)
                {
                    Stop();

                    lock (_listLock)
                    {
                        foreach (var connection in _state)
                        {
                            connection?.Dispose();
                        }

                        _state = null;
                    }
                }
            }

            _disposed = true;
        }
    }
}