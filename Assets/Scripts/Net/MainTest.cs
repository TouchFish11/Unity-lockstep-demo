using Core.DI;
using kcp2k;
using Net.Sync;
using UnityEngine;

namespace Net
{
    public class MainTest : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            DIContainer.BindSingleton<NetGameProxy>();
            DIContainer.InjectDependencies();
            
            
            var config = new NetConfig
            {
                ServerIp = "127.0.0.1",
                ServerPort = 8080,
                Serializer = MessageSerializerGetter.BinaryMessageSerializer(),
                ClientType = EClientType.Kcp,
                KcpConfig = new KcpConfig()
            };

            var proxy = DIContainer.GetDependency<INetGameProxy>().Init(config);
            proxy.OnGameConnected += OnOnGameConnected;
            
            proxy.Connect();
        }

        private void OnOnGameConnected(int obj)
        {
            
        }
    }
}
