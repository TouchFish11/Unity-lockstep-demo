using System.Collections.Generic;
using Core.GlobalEvent;
using Core.Math;
using Core.Net.Events;
using Core.Net.Protocols.FSync;
using HotUpdate.Game.Race.Logic;

public class LogicWorld
{
    private readonly List<LogicAvatar> _avatars = new();          // 固定序遍历
    private readonly Dictionary<int, LogicAvatar> _byId = new();  // 仅按键查询，不遍历
    private readonly List<AiController> _aiControllers = new();   // 固定序，确定性
    private readonly IEventCenter _eventCenter;
    private bool _raceEnded;

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
    /// 每逻辑帧一次：玩家命令 → AI → 推进攻击 → 命中结算 → 碰撞
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
            ai.Tick(_avatars);
        }
        
        // 推进攻击状态机（所有 avatar，含 AI；AI 本期不攻击故为 no-op）
        foreach (var avatar in _avatars)
        {
            avatar.Tick();
        }
        
        // 命中结算：本帧处于命中帧的 attacker，扇形内扣血
        foreach (var attacker in _avatars)
        {
            // 死者不能被攻击
            if (attacker.IsDead)
                continue;
            
            if (attacker.IsHitFrame)
            {
                ResolveAttack(attacker);
            }
        }

        // 碰撞
        ResolveCollisions();
        
        // 胜负判定
        CheckMatchEnd();
    }
    
    /// <summary>
    /// 命中结算：attacker 朝向扇形 + 距离内的目标扣血
    /// </summary>
    private void ResolveAttack(LogicAvatar attacker)
    {
        var rangeSq = LogicAvatar.AttackRange * LogicAvatar.AttackRange;
        foreach (var target in _avatars)
        {
            if (target == attacker)
                continue;
            
            // 死者不再受伤
            if (target.IsDead)
                continue; 
            
            // 同阵营不伤害（玩家不打玩家，怪物不打怪物）
            if (attacker.IsPlayer == target.IsPlayer)
                continue;
            
            var toTarget = target.Position - attacker.Position;
            if (toTarget.SqrMagnitude() > rangeSq)
                continue;
            
            var dirToTarget = toTarget.Normalized();
            if (dirToTarget.Dot(attacker.Facing) >= LogicAvatar.HalfAngleCos)
            {
                target.TakeDamage(LogicAvatar.Damage);
            }
        }
    }
    
    /// <summary>
    /// 胜负判定：怪物全死=胜利，玩家全死=失败（只触发一次）
    /// </summary>
    private void CheckMatchEnd()
    {
        if (_raceEnded)
            return;

        var anyAlivePlayer = false;
        var anyAliveMonster = false;
        foreach (var avatar in _avatars)
        {
            if (avatar.IsDead)
                continue;
            
            if (avatar.IsPlayer)
                anyAlivePlayer = true;
            else
                anyAliveMonster = true;
        }

        if (!anyAliveMonster || !anyAlivePlayer)
        {
            _raceEnded = true;
            var evt = EventSource.Get<RaceEndEvent>();
            evt.Win = !anyAliveMonster; // 怪物全死 = 胜利
            _eventCenter.TriggerEvent(evt);
        }
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
        // 死者不参与碰撞
        if (a.IsDead || b.IsDead)
            return;
        
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