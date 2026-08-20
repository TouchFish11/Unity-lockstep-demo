using System;
using System.Collections.Generic;

namespace Net.Protocols
{
    /// <summary>
    /// 消息序列化器获取器
    /// </summary>
    public static class MessageSerializerGetter
    {
        private static readonly Dictionary<Type, IMessageResolver> serializers = new()
        {
            {typeof(DefaultMessageResolver),  new DefaultMessageResolver()},
        };

        public static Func<IMessageResolver> BinaryMessageSerializer => () => serializers[typeof(DefaultMessageResolver)];
    }
}
