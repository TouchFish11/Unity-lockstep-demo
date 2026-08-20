using Core.DI;
using Core.Log;
using kcp2k;
using Net.Protocols;
using Net.SyncModule.Interface;
using Net.SyncModule.Manager;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace Net
{
    public class MainTest : MonoBehaviour
    {
        private void Start()
        {
            Application.runInBackground = true;
            // 注册框架单例
            //DIContainer.RegisterSingletons();
            // 注入依赖
            //DIContainer.InjectDependencies();
            
            DIContainer.BindSingleton<INetGameProxy, NetGameProxy>();
            DIContainer.Bind<NetManager>().As<INetManager>().As<IHeartbeatService>().AsSingleton();
            
            // -----------------
            var config = new NetConfig
            {
                ServerIp = "127.0.0.1",
                ServerPort = 8080,
                Resolver = MessageSerializerGetter.BinaryMessageSerializer(),
                ClientType = EClientType.Kcp,
                KcpConfig = new KcpConfig(DualMode:false, Timeout: 30000)
            };
            
            var proxy = DIContainer.GetInstance<INetGameProxy>().Init(config);
            proxy.OnGameConnected += OnOnGameConnected;
            proxy.Connect();
        }

        private static void OnOnGameConnected(int clientId)
        {
            Logger.LogDebug(ELogTags.System, $"[Net Connect] 已初始化客户端ID:{clientId}");
        }

        private void OnDisable()
        {
            DIContainer.GetInstance<INetGameProxy>().Disconnect();
        }
    }
}
