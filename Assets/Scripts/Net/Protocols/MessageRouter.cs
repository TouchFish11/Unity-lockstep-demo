using System;
using System.Collections.Generic;
using Core.DI;
using Net.Protocols.FSync.Handlers;
using Net.Protocols.Tcp.Messages.Common;
using Net.SyncModule.Interface;

namespace Net.Protocols
{
    /// <summary>
    /// 消息路由器
    /// </summary>
    public class MessageRouter
    {
        private readonly Dictionary<Type, IMessageHandler> _handlers = new();

        public MessageRouter()
        {
            RegisterHandler(typeof(S2C_ConnectMessage), DIContainer.Create<ConnectMessageHandler>());
            RegisterHandler(typeof(FSFrameHandler), DIContainer.Create<FSFrameHandler>());
            // ...
        }

        private void RegisterHandler(Type msgType, IMessageHandler handler)
        {
            _handlers.Add(msgType, handler);
        }

        public void Dispatch(Message message)
        {
            if (!_handlers.TryGetValue(message.GetType(), out var handler)) 
                throw new Exception($"Handler {message.GetType()} not registered");
            
            // 处理器处理消息
            handler.HandleMessage(message);
        }
    }
}
