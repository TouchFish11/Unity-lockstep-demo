namespace Core.Net.Protocols.Tcp
{
    /// <summary>
    /// TCP消息
    /// </summary>
    public abstract class TcpMessage : Message
    {
        /// <summary>
        /// 发送消息的客户端唯一ID，由服务器下发
        /// </summary>
        public int SessionID { get; set; }
        
        /// <summary>
        /// 获取消息唯一ID
        /// </summary>
        /// <returns></returns>
        public abstract int GetMsgID();
        
        public sealed override int GetMsgLength()
        {
            // 客户端ID + [子类消息体]
            return sizeof(int) + GetBodyLength();
        }
        
        /// <summary>
        /// 子类消息体长度，不包含客户端ID（由基类负责统一序列化）
        /// </summary>
        /// <returns></returns>
        protected abstract int GetBodyLength();

        /// <summary>
        /// 序列化
        /// </summary>
        /// <returns></returns>
        public sealed override byte[] Serialize()
        {
            var index = 0;
            var bytes = new byte[GetMsgLength()];
            MessageUtil.WriteField(bytes, SessionID, ref index);    // 序列化客户端ID
            SerializeBody(bytes, ref index);                        // 序列化子消息体
            return bytes;
        }

        /// <summary>
        /// 序列化消息体
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        protected abstract void SerializeBody(byte[] bytes, ref int index);

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="beginIndex"></param>
        /// <returns></returns>
        public sealed override int Deserialize(byte[] bytes, int beginIndex = 0)
        {
            var index = beginIndex;
            SessionID = MessageUtil.ReadInt(bytes, ref index);      // 反序列化客户端ID
            DeserializeBody(bytes, ref index);                      // 反序列化子消息体
            return index - beginIndex;
        }

        /// <summary>
        /// 反序列化消息体
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        protected abstract void DeserializeBody(byte[] bytes, ref int index);
    }
}
