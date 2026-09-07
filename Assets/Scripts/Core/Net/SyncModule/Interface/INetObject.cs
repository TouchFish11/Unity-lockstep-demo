using Core.Net.Protocols.FSync;
using Core.Net.Protocols.FSync.Messages;

namespace Core.Net.SyncModule.Interface
{
    /// <summary>
    /// 网络对象
    /// </summary>
    public interface INetObject
    {
        void CollectInput(ref InputCommand cmd);
        
        // /// <summary>
        // /// 收集客户端输入
        // /// </summary>
        // /// <param name="optMessage"></param>
        // void CollectInput(OptMessage optMessage);
        //
        // /// <summary>
        // /// 同步当前帧
        // /// </summary>
        // /// <param name="optMessage"></param>
        // void SyncFrame(OptMessage optMessage);
    }
}
