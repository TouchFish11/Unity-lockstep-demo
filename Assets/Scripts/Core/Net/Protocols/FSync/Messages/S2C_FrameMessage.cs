using System.Collections.Generic;
using Core.Net.Protocols.Configs;

namespace Core.Net.Protocols.FSync.Messages
{
    /// <summary>
    /// 服务器发送给客户端的帧消息
    /// </summary>
    public class S2C_FrameMessage : FrameMessage
    {
        public int FrameMessageID => MessageIDConfig.S2C_Frame_ID;
        
        /// <summary>
        /// 帧消息列表，可能包含多帧消息
        /// </summary>
        public List<S2C_OneFrameMessage> FrameMessages { get; private set; }
        
        public override int GetMsgLength()
        {
            var length = 0;
            // 列表内容长度
            length += 4;
            // 列表内容
            foreach (var oneFrameMessage in FrameMessages)
            {
                length += oneFrameMessage.GetMsgLength();
            }
        
            return length;
        }
        
        public override byte[] Serialize()
        {
            var index = 0;
            var length = GetMsgLength();
            var bytes = new byte[length];
            // 写入消息列表长度
            MessageUtil.WriteField(bytes, length, ref index);
            // 写入消息内容
            foreach (var oneFrameMessage in FrameMessages)
            {
                MessageUtil.WriteField(bytes, oneFrameMessage, ref index);
            }
            return bytes;
        }

        public override int Deserialize(byte[] bytes, int beginIndex = 0)
        {
            var index = beginIndex;
            var length = MessageUtil.ReadInt(bytes, ref index);  // 反序列化消息内容长度
            FrameMessages = new List<S2C_OneFrameMessage>(length);
            // 反序列化消息内容
            for (var i = 0; i < length; i++)
            {
                FrameMessages.Add(MessageUtil.ReadFrameMessage<S2C_OneFrameMessage>(bytes, ref index));
            }
            return index - beginIndex;
        }
    }
}
