using System.Threading.Tasks;
using Core.Tasks;
using UnityEngine.Events;

namespace Core.Service.Login
{
    /// <summary>
    /// 登录服务
    /// </summary>
    public class LoginService : ILoginService
    {
        public event UnityAction<bool> OnAutoLoginCompleted;

        public async Task LoginAsync(LoginData loginData)
        {
            // 启动客户端
            //NetManager.Instance.StartClient("127.0.0.1", 8080);
            // 等待连接成功
            //await TaskUtility.WaitUntil(() => NetManager.Instance.GetTcpClient().ConnectData != null);
            // 执行回调
            //OnAutoLoginCompleted?.Invoke(NetManager.Instance.Connected);
        }

        public void SaveLoginData(LoginData loginData)
        {
            //DIContainer.GetInstance<IBinaryDataManager>().SaveAsync(FileSources.LocalLoginDataFileName, loginData);
        }

        public Task<LoginData> LoadLoginData()
        {
            //return await DIContainer.GetInstance<IBinaryDataManager>().LoadAsync<LoginData>(FileSources.LocalLoginDataFileName);
            return (Task<LoginData>)Task.CompletedTask;
        }
    }
}
