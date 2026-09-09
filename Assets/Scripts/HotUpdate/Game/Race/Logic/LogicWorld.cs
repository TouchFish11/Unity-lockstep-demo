using System.Collections.Generic;
using Core.GlobalEvent;
using Core.GlobalEvent.Events.Net;
using Core.Math;
using Core.Net.Protocols.FSync;
using HotUpdate.Game.Race.Logic;

public class LogicWorld
{
    private readonly List<LogicAvatar> _avatars = new();          // 固定序遍历
    private readonly Dictionary<int, LogicAvatar> _byId = new();  // 仅按键查询，不遍历
    private readonly List<AiController> _aiControllers = new();   // 固定序，确定性
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

    public void AddAi(AiController ai) => _aiControllers.Add(ai);

    public LogicAvatar GetAvatar(int playerId) => _byId.GetValueOrDefault(playerId);

    public void Clear()
    {
        _avatars.Clear();
        _byId.Clear();
        _aiControllers.Clear();
    }

    public void Unsubscribe()
    {
        _eventCenter.UnsubscribeEvent<FrameCommandsEvent>(OnFrameCommands);
    }

    private void OnFrameCommands(FrameCommandsEvent evt) => Tick(evt.Commands);

    /// <summary>
    /// 每逻辑帧一次：玩家命令 → AI → 碰撞
    /// </summary>
    public void Tick(List<InputCommand> playerCommands)
    {
        // 玩家命令
        foreach (var cmd in playerCommands)
        {
            if (_byId.TryGetValue(cmd.raceId, out var avatar))
            {
                avatar.Execute(cmd);
            }
        }

        // AI（固定序遍历，确定性）
        foreach (var ai in _aiControllers)
        {
            ai.Tick();
        }

        // 碰撞
        ResolveCollisions();
    }

    private void ResolveCollisions()
    {
        const int Iterations = 3;   // 多次迭代处理多体挤压
        for (var iter = 0; iter < Iterations; iter++)
        {
            for (var i = 0; i < _avatars.Count; i++)
            {
                for (var j = i + 1; j < _avatars.Count; j++)
                {
                    Separate(_avatars[i], _avatars[j]);
                }
            }
        }
    }

    private static void Separate(LogicAvatar a, LogicAvatar b)
    {
        var delta = b.Position - a.Position;
        var distSq = delta.SqrMagnitude();
        var minDist = a.Radius + b.Radius;
        if (distSq >= minDist * minDist)
            return;   // 不重叠

        // 完全重合：沿固定轴推开，避免除零
        if (distSq == Fixed64.Zero)
        {
            var half = minDist / Fixed64.FromInt(2);
            a.Nudge(new FixedVector3(-half, Fixed64.Zero, Fixed64.Zero));
            b.Nudge(new FixedVector3(half, Fixed64.Zero, Fixed64.Zero));
            return;
        }

        var dist = Fixed64.Sqrt(distSq);
        var overlap = minDist - dist;
        var dir = delta / dist;                  // a → b 的单位向量
        var correction = dir * (overlap / Fixed64.FromInt(2));    // 各推一半
        a.Nudge(FixedVector3.Zero - correction); // FixedVector3 无一元负号，用 Zero 减
        b.Nudge(correction);
    }
}