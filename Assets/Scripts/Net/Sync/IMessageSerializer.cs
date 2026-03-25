namespace Net.Sync
{
    /// <summary>
    /// 消息序列化器接口
    /// </summary>
    public interface IMessageSerializer
    {
        byte[] Serialize(Message message);

        Message Deserialize(byte[] data);
    }
}
