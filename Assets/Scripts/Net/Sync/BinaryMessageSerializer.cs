using System;

namespace Net.Sync
{
    /// <summary>
    /// 二进制消息序列化器
    /// </summary>
    public class BinaryMessageSerializer : IMessageSerializer
    {
        public byte[] Serialize(Message message)
        {
            return message.Serialize();
        } 

        public Message Deserialize(byte[] data)
        {
            var nowIndex = 0;
            // 解析消息ID
            var msgID = BitConverter.ToInt32(data, 0);
            nowIndex += 4;
            // 通过工厂创建消息
            return MessageFactory.CreateMessage(msgID, data, nowIndex);
        }
    }
}
