using Core.DI;
using Core.Log;
using Core.Net.Protocols;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.Registration;
using kcp2k;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Net
{
    public class MainTest : MonoBehaviour
    {
        private async void Start()
        {
            Application.runInBackground = true;
            await RegisterCore.InitCore();
            
            // -----------------
            var config = new NetConfig
            {
                ServerIp = "127.0.0.1",
                ServerPort = 8080,
                Resolver = MessageSerializerSource.DefaultMessageResolver,
                ClientType = EClientType.Tcp,
                KcpConfig = new KcpConfig(DualMode:false, Timeout: 30000)
            };
            
            var proxy = DIContainer.Resolve<INetGameProxy>().Init(config);
            proxy.OnConnected += OnConnected;
            proxy.OnDisconnected += OnDisconnected;
            proxy.TcpRtt += OnTcpRtt;
            proxy.Connect();
        }

        private void OnTcpRtt(long rtt)
        {
            Logger.LogDebug(ELogTags.System, $"[Net] TCP RTT:{rtt}ms");
        }

        private static void OnConnected(int clientId)
        {
            Logger.LogDebug(ELogTags.System, $"[Net] 已初始化客户端ID:{clientId}");
        }
        
        private void OnDisconnected()
        {
            Logger.LogDebug(ELogTags.System, $"[Net] 网络连接已断开");
        }

        private void OnDisable()
        {
            DIContainer.GetInstance<INetGameProxy>().Disconnect();
        }
    }
}
