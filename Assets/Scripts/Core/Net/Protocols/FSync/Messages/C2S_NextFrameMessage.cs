using Core.Net.Protocols.Configs;

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
        
        public int FrameMessageID => MessageIDConfig.C2S_NextFrame_ID;
        
        public override int GetMsgLength()
        {
            // 帧ID(int) + OptMessage长度
            return sizeof(int) + OptMessage.GetMsgLength();
        }
        
        public override byte[] Serialize()
        {
            var index = 0;
            var bytes = new byte[GetMsgLength()];
            MessageUtil.WriteField(bytes, FrameID, ref index);      // 写入帧ID
            MessageUtil.WriteField(bytes, OptMessage, ref index);   // 写入下一帧的操作
            return bytes;
        }
        
        public override int Deserialize(byte[] bytes, int beginIndex = 0)
        {
            var index = beginIndex;
            FrameID = MessageUtil.ReadInt(bytes, ref index);                            // 反序列化帧ID
            OptMessage = MessageUtil.ReadFrameMessage<OptMessage>(bytes, ref index);    // 反序列操作
            return index - beginIndex;
        }
    }
}
