using System;
using System.Collections.Generic;
using System.Reflection;
using Core.DI;
using Core.Exceptions;
using Core.HotUpdate;
using Core.Log;
using Core.Net.Protocols.FSync.Handlers;
using Core.Net.Protocols.FSync.Messages;
using Core.Net.Protocols.Tcp;
using Core.Net.Protocols.Tcp.Handlers.S2C;
using Core.Net.Protocols.Tcp.Messages.Battle.S2C;
using Core.Net.Protocols.Tcp.Messages.Common;

namespace Core.Net.Protocols
{
    /// <summary>
    /// 消息路由器
    /// </summary>
    internal class MessageRouter
    {
        private readonly IHotUpdateManager _hotUpdateManager;
        private readonly Dictionary<Type, IMessageHandler> _handlers = new();

        public MessageRouter(IHotUpdateManager hotUpdateManager)
        {
            _hotUpdateManager = hotUpdateManager;
        }

        /// <summary>
        /// 注册所有的消息处理器
        /// </summary>
        public void RegisterHandlers()
        {
            // foreach (var type in hotUpdateManager.GetCoreModule().GetTypes())
            // {
            //     if(!typeof(IMessageHandler).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
            //         continue;
            //
            //     var messageHandler = (IMessageHandler)DIContainer.Create(type);
            //     var messageType = messageHandler.MessageType;
            //     if (!messageType.IsDefined(typeof(MessageDirAttribute), false))
            //         continue;
            //
            //     var messageDirAttribute = messageType.GetCustomAttribute<MessageDirAttribute>();
            //     if (messageDirAttribute.MessageHandle is EMessageHandle.Resolve or EMessageHandle.Both)
            //     {
            //         _handlers.Add(messageHandler.MessageType, messageHandler);
            //     }
            // }
            
            _handlers.Add(typeof(S2C_ConnectMessage), DIContainer.Create<S2C_ConnectMessageHandler>());
            _handlers.Add(typeof(S2C_DisConnectMessage), DIContainer.Create<S2C_DisConnectMessageHandler>());
            _handlers.Add(typeof(HeartMessage), DIContainer.Create<S2C_HeartMessageHandler>());
            _handlers.Add(typeof(LeaveRaceMessage), DIContainer.Create<S2C_LeaveRaceMessageHandler>());
            _handlers.Add(typeof(S2C_MatchSuccessMessage), DIContainer.Create<S2C_MatchSuccessMessageHandler>());
            _handlers.Add(typeof(S2C_PrepareRaceMessage), DIContainer.Create<S2C_PrepareRaceMessageHandler>());
            _handlers.Add(typeof(ReconnectRaceMessage), DIContainer.Create<S2C_ReconnectRaceMessageHandler>());
            _handlers.Add(typeof(S2C_StartRaceMessage), DIContainer.Create<S2C_StartRaceMessageHandler>());
            _handlers.Add(typeof(S2C_FrameMessage), DIContainer.Create<S2C_FrameMessageHandler>());
            _handlers.Add(typeof(S2C_ClientJoinMessage), DIContainer.Create<S2C_ClientJoinMessageHandler>());
            _handlers.Add(typeof(S2C_ClientConfirmMatchStateMessage), DIContainer.Create<S2C_ClientConfirmMatchStateMessageHandler>());
            // ...
        }

        /// <summary>
        /// 调度消息到对应的处理器处理
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="Exception"></exception>
        public void Dispatch(Message message)
        {
            if (!_handlers.TryGetValue(message.GetType(), out var handler)) 
                throw ExceptionHelper.Throw($"Handler {message.GetType()} not registered");
            
            // 处理器处理消息
            handler.HandleMessage(message);
        }
    }
}
