using System;
using System.Text;
using Core.Net.Protocols.FSync.Messages;

namespace Core.Net.Protocols
{
    /// <summary>
    /// 消息工具类
    /// </summary>
    public static class MessageUtil
    {
        /// <summary>
        /// 写入字段
        /// </summary>
        /// <typeparam name="T">支持byte、short、int、long、float、double、bool、char、string、FrameMessage</typeparam>
        /// <param name="bytes">字节数组</param>
        /// <param name="value">字段值</param>
        /// <param name="index">当前写入位置</param>
        public static void WriteField<T>(byte[] bytes, T value, ref int index)
        {
            switch (value)
            {
                case byte byteValue:
                    bytes[index] = byteValue;
                    index += 1;
                    break;
                case short shortValue:
                    BitConverter.GetBytes(shortValue).CopyTo(bytes, index);
                    index += 2;
                    break;
                case int intValue:
                    BitConverter.GetBytes(intValue).CopyTo(bytes, index);
                    index += 4;
                    break;
                case long longValue:
                    BitConverter.GetBytes(longValue).CopyTo(bytes, index);
                    index += 8;
                    break;
                case float floatValue:
                    BitConverter.GetBytes(floatValue).CopyTo(bytes, index);
                    index += 4;
                    break;
                case double doubleValue:
                    BitConverter.GetBytes(doubleValue).CopyTo(bytes, index);
                    index += 8;
                    break;
                case bool boolValue:
                    BitConverter.GetBytes(boolValue).CopyTo(bytes, index);
                    index += 1;
                    break;
                case char charValue:
                    BitConverter.GetBytes(charValue).CopyTo(bytes, index);
                    index += 2;
                    break;
                case string stringValue:
                    var strBytes = Encoding.UTF8.GetBytes(stringValue);
                    var length = strBytes.Length;
                    BitConverter.GetBytes(length).CopyTo(bytes, index);
                    index += 4;
                    strBytes.CopyTo(bytes, index);
                    index += length;
                    break;
                case FrameMessage frameMessage:
                    frameMessage.Serialize().CopyTo(bytes, index);
                    index += frameMessage.GetMsgLength();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        /// <summary>
        /// 读取byte数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  byte ReadByte(byte[] bytes, ref int index)
        {
            var byteValue = bytes[index];
            index += 1;
            return byteValue;
        }

        /// <summary>
        /// 读取short数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  short ReadShort(byte[] bytes, ref int index)
        {
            var shortValue = BitConverter.ToInt16(bytes, index);
            index += 2;
            return shortValue;
        }

        /// <summary>
        /// 读取int数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  int ReadInt(byte[] bytes, ref int index)
        {
            var intValue = BitConverter.ToInt32(bytes, index);
            index += 4;
            return intValue;
        }

        /// <summary>
        /// 读取long数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  long ReadLong(byte[] bytes, ref int index)
        {
            var longValue = BitConverter.ToInt64(bytes, index);
            index += 8;
            return longValue;
        }

        /// <summary>
        /// 读取float数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  float Readfloat(byte[] bytes, ref int index)
        {
            var floatValue = BitConverter.ToSingle(bytes, index);
            index += 4;
            return floatValue;
        }

        /// <summary>
        /// 读取double数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  double ReadDouble(byte[] bytes, ref int index)
        {
            var doubleValue = BitConverter.ToDouble(bytes, index);
            index += 8;
            return doubleValue;
        }

        /// <summary>
        /// 读取char数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  char ReadChar(byte[] bytes, ref int index)
        {
            var charValue = BitConverter.ToChar(bytes, index);
            index += 2;
            return charValue;
        }

        /// <summary>
        /// 读取bool数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  bool ReadBool(byte[] bytes, ref int index)
        {
            var boolValue = BitConverter.ToBoolean(bytes, index);
            index += 1;
            return boolValue;
        }

        /// <summary>
        /// 读取string数据
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static  string ReadString(byte[] bytes, ref int index)
        {
            var length = ReadInt(bytes, ref index);
            var strValue = Encoding.UTF8.GetString(bytes, index, length);
            index += length;
            return strValue;
        }

        /// <summary>
        /// 读取FrameMessage数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T ReadFrameMessage<T>(byte[] bytes, ref int index) where T : FrameMessage, new()
        {
            var frameCommand = new T();
            index += frameCommand.Deserialize(bytes, index);
            return frameCommand;
        }
    }
}
