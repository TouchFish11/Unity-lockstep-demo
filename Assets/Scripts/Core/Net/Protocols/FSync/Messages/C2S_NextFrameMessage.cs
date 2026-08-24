using Net.Protocols;
using Net.Protocols.Configs;

namespace Core.Net.Protocols.FSync.Messages
{
    /// <summary>
    /// 客户端下一帧的操作，收集、发送时使用
    /// </summary>
    public class C2S_NextFrameMessage : FrameMessage
    {
        /// <summary>
        /// ֡下一帧帧ID
        /// </summary>
        public int FrameID { get; set; }

        /// <summary>
        /// 下一帧的操作
        /// </summary>
        public OptMessage OptMessage { get; set; }
        
        public static int FrameMessageID => MessageIDConfig.C2S_NextFrame_ID;
    
        public override int GetMsgLength()
        {
            // 消息ID (int) + 消息体长度(int) + 帧ID(int) + OptMessage长度
            return sizeof(int) + sizeof(int) + sizeof(int) + OptMessage.GetMsgLength();
        }

        public override byte[] Serialize()
        {
            var index = 0;
            var length = GetMsgLength();
            var bytes = new byte[length];
            // 写入消息ID
            MessageUtil.WriteField(bytes, FrameMessageID, ref index);
            // 写入消息体长度
            MessageUtil.WriteField(bytes, length, ref index);
            MessageUtil.WriteField(bytes, FrameID, ref index);
            MessageUtil.WriteField(bytes, FrameID, ref index);
            MessageUtil.WriteField(bytes, OptMessage, ref index);
            return bytes;
        }

        public override int Deserialize(byte[] bytes, int beginIndex = 0)
        {
            var index = beginIndex;
            MessageUtil.ReadInt(bytes, ref index);  // 反序列化消息ID
            var length = MessageUtil.ReadInt(bytes, ref index);  // 反序列化消息体长度
            FrameID = MessageUtil.ReadInt(bytes, ref index);
            OptMessage = MessageUtil.ReadFrameMessage<OptMessage>(bytes, ref index);
            return index - beginIndex;
        }
    }
}
