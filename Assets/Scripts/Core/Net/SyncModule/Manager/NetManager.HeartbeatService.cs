using System;
using System.Threading.Tasks;
using Core.Exceptions;
using Core.Log;
using Core.Net.Protocols;
using Core.Net.Protocols.Tcp.Messages.Common;
using Core.Net.SyncModule.Interface;

namespace Core.Net.SyncModule.Manager
{
    internal partial class NetManager : IHeartbeatService
    {
        private bool _isRunning;
        private readonly HeartMessage _heartMessage;

        public int Interval { get; set; } = 5000;
        
        public long LastRttMilliseconds { get; private set; }
        
        public DateTime? LastHeartbeatUtc { get; private set; }
        
        public TimeSpan TimeoutThreshold { get; set; } = TimeSpan.FromMinutes(1);
        
        public event Action<long> OnRttCalc;

        public bool IsAlive => DateTime.UtcNow - LastHeartbeatUtc < TimeoutThreshold;

        private NetManager()
        {
            _heartMessage = new HeartMessage();
        }
        
        public async void Start()
        {
            try
            {
                _isRunning = true;
                while (_client.IsConnected && _isRunning)
                {
                    if (!IsAlive && LastHeartbeatUtc != null)
                        throw ExceptionHelper.Throw($"连接超时（心跳阈值：{Interval}；最近心跳时间：{LastHeartbeatUtc}");
                    
                    SendHeartbeatAsync();
                    Logger.LogDebug(ELogTags.Network, $"已发送心跳消息");
                    await Task.Delay(Interval);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(EErrorCode.Timeout, e.Message);
                Logger.LogException(ELogTags.Network, e);
            }
        }
        
        public void Stop()
        {
            _isRunning = false;
        }

        public void SendHeartbeatAsync()
        {
            _heartMessage.SessionID = SessionId;
            _heartMessage.ClientTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Send(_heartMessage, EProtocolChannel.Resolve);
        }

        public void CalcRtt(long clientSendTime)
        {
            LastHeartbeatUtc = DateTimeOffset.UtcNow.DateTime;
            LastRttMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - clientSendTime;
            OnRttCalc?.Invoke(LastRttMilliseconds);
        }
    }
}
