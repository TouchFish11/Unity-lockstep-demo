using System;
using System.Net;
using System.Net.Sockets;
using Net.Sync;
using Net.SyncModule.Manager;
using Logger = Core.Log.Logger;

namespace Net.SyncModule.Clients
{
    /// <summary>
    /// 帧同步Tcp客户端
    /// </summary>
    public class TcpClient : IProtocolClient
    {
        private Socket _tcpSocket;
        // 消息缓冲区
        private readonly byte[] _bytesBuffer;
        
        public event Action<byte[], EProtocolChannel> OnDataReceived;
        
        public event Action OnConnected;
        
        public event Action OnDisconnected;
        
        public event Action<string> OnError;

        public TcpClient(short bufferSize = 4096)
        {
            _bytesBuffer = new byte[bufferSize];
        }
        
        public async void Connect(string serverIp, ushort serverPort)
        {
            try
            {
                if (_tcpSocket != null && _tcpSocket.Connected)
                    return;
                
                _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await _tcpSocket.ConnectAsync(IPAddress.Parse(serverIp), serverPort);
                OnConnected?.Invoke();
                ReceiveAsync();
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                OnError?.Invoke(e.Message);
            }
        }

        public async void SendAsync(byte[] data, EProtocolChannel channel)
        {
            try
            {
                await _tcpSocket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                OnError?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// 异步接收
        /// </summary>
        private async void ReceiveAsync()
        {
            try
            {
                while (true)
                {
                    var receive = await _tcpSocket.ReceiveAsync(new ArraySegment<byte>(_bytesBuffer), SocketFlags.None);
                    var copyBuffer = new byte[receive];
                    Array.Copy(_bytesBuffer, 0, copyBuffer, 0, receive);
                    OnDataReceived?.Invoke(copyBuffer, EProtocolChannel.Unreliable);
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                OnError?.Invoke(e.Message);
            }
        }

        public void Tick()
        {
            
        }

        public void Disconnect()
        {
            if (_tcpSocket == null)
                return;

            if (_tcpSocket.Connected)
            {
                _tcpSocket.Shutdown(SocketShutdown.Send);
                _tcpSocket.Disconnect(false);
                _tcpSocket.Close();
                _tcpSocket.Dispose();
            }
            _tcpSocket = null;
            OnDisconnected?.Invoke();
        }
    }
}
