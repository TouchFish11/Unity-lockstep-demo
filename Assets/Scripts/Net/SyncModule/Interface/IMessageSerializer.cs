using Net.Protocols;
using Net.SyncModule.Manager;

namespace Net.SyncModule.Interface
{
    /// <summary>
    /// 消息序列化器接口
    /// </summary>
    public interface IMessageSerializer
    {
        byte[] Serialize(Message message, EProtocolChannel channel);

        Message Deserialize(byte[] data, EProtocolChannel channel);
    }
}
