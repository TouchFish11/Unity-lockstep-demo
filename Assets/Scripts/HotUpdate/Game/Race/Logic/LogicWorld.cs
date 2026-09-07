using System.Collections.Generic;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;

namespace HotUpdate.Game.Race.Logic
{
    public class LogicWorld
    {
        private readonly List<LogicAvatar> _avatars = new();          // 固定序遍历
        private readonly Dictionary<int, LogicAvatar> _byId = new();  // 仅按键查询，不遍历
        private readonly IEventCenter _eventCenter;

        public LogicWorld(IEventCenter eventCenter)
        {
            _eventCenter = eventCenter;
            _eventCenter.SubscribeEvent<FrameCommandsEvent>(OnFrameCommands);
        }

        public void AddAvatar(LogicAvatar avatar)
        {
            _avatars.Add(avatar);
            _byId[avatar.PlayerId] = avatar;
        }

        public LogicAvatar GetAvatar(int playerId) => _byId.GetValueOrDefault(playerId);

        public void Clear()
        {
            _avatars.Clear();
            _byId.Clear();
        }

        public void Unsubscribe()
        {
            _eventCenter.UnsubscribeEvent<FrameCommandsEvent>(OnFrameCommands);
        }

        private void OnFrameCommands(FrameCommandsEvent evt)
        {
            foreach (var cmd in evt.Commands)
            {
                if (_byId.TryGetValue(cmd.playerId, out var avatar))
                    avatar.Execute(cmd);
            }
        }
    }
}