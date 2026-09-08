using Core.DI;
using Core.Log;
using Core.Net.Protocols;
using Core.Net.SyncModule.Interface;
using Core.Net.SyncModule.Manager;
using Core.Registration;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.Update
{
    public class MainTest : MonoBehaviour
    {
        // private async void Awake()
        // {
        //     Application.runInBackground = true;
        //     await RegisterCore.InitCore();
        // }
        //
        // private void Start()
        // {
        //     // -----------------
        //
        // }
        //
        // private void OnTcpRtt(long rtt)
        // {
        //     Logger.LogDebug(ELogTags.System, $"[Net] TCP RTT:{rtt}ms");
        // }
        //
        // private static void OnConnected(int clientId, int[] clientIds)
        // {
        //     Logger.LogDebug(ELogTags.System, $"[Net] 已初始化客户端ID:{clientId}");
        // }
        //
        // private void OnDisconnected()
        // {
        //     Logger.LogDebug(ELogTags.System, $"[Net] 网络连接已断开");
        // }
        //
        // private void OnDisable()
        // {
        //     _netManager.Disconnect();
        // }
    }
}
