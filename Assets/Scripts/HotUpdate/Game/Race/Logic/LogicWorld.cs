using System.Collections.Generic;
using Core.GlobalEvent;
using Core.Math;
using Core.Net.Events;
using Core.Net.Protocols.FSync;
using HotUpdate.Game.Race.Present;

namespace HotUpdate.Game.Race.Logic
{
    public class LogicWorld
    {
        private readonly List<LogicAvatar> _avatars = new();          // 固定序遍历
        private readonly Dictionary<int, LogicAvatar> _byId = new();  // 仅按键查询，不遍历
        private readonly List<AiController> _aiControllers = new();   // 固定序，确定性
        private readonly IEventCenter _eventCenter;
        private bool _raceEnded;
        private PresentEventsEvent _presentEvent;
        private LevelConfig _level;

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

        public void AddAi(AiController ai)
        {
            _aiControllers.Add(ai);
        }

        public void SetLevel(LevelConfig level)
        {
            _level = level;
        }

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
            ReplayRecorder.Record(playerCommands);   // 回放录制
            
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
                    ResolveAbility(attacker);
                    var up = new FixedVector3(Fixed64.Zero, Fixed64.One, Fixed64.Zero);
                    AddEffect(attacker.CurrentAbility.EffectId, attacker.Position + up, attacker.Facing);
                }
            }

            // 碰撞
            ResolveCollisions();
        
            // 环境碰撞（边界 clamp + 推出障碍物）
            ResolveEnvironment();
            
            // 胜负判定
            CheckMatchEnd();
            
            // 帧末批量发特效事件
            FlushEffects();
        }
    
        private void ResolveAbility(LogicAvatar attacker)
        {
            var cfg = attacker.CurrentAbility;
            var rangeSq = cfg.Range * cfg.Range;
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

                var hit = cfg.Shape switch
                {
                    EAbilityShape.Fan    => toTarget.Normalized().Dot(attacker.Facing) >= cfg.HalfAngleCos,
                    EAbilityShape.Circle => true,
                    _                    => false,
                };

                if (hit)
                {
                    target.TakeDamage(cfg.Damage);
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

        private void AddEffect(int effectId, FixedVector3 pos, FixedVector3 dir)
        {
            _presentEvent ??= EventSource.Get<PresentEventsEvent>();
            _presentEvent.Events.Add(new PresentEvent
            {
                Type = EPresentEventType.SpawnEffect,
                Id = effectId,
                Pos = pos,
                Dir = dir,
            });
        }
        
        private void FlushEffects()
        {
            if (_presentEvent == null)
                return;
            _eventCenter.TriggerEvent(_presentEvent);
            _presentEvent = null;
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
        
        private void ResolveEnvironment()
        {
            if (_level == null)
                return;

            foreach (var avatar in _avatars)
            {
                if (avatar.IsDead)
                    continue;

                // clamp 到边界（带半径余量）
                var clamped = ClampToBounds(avatar.Position, avatar.Radius);
                // clamped是<=avatar.Position
                avatar.Nudge(clamped - avatar.Position);

                // 推出障碍物（角色全量推出，障碍物不动）
                foreach (var ob in _level.Obstacles)
                {
                    SeparateFromObstacle(avatar, ob);
                }
            }
        }
        
        private FixedVector3 ClampToBounds(FixedVector3 pos, Fixed64 radius)
        {
            var loX = _level.BoundsMin.x + radius;
            var hiX = _level.BoundsMax.x - radius;
            var loZ = _level.BoundsMin.z + radius;
            var hiZ = _level.BoundsMax.z - radius;
            return new FixedVector3(Clamp(pos.x, loX, hiX), pos.y, Clamp(pos.z, loZ, hiZ));
        }
        
        private static Fixed64 Clamp(Fixed64 v, Fixed64 lo, Fixed64 hi)
        {
            if (v < lo)
                return lo;
            if (v > hi)
                return hi;
            return v;
        }
        
        private static void SeparateFromObstacle(LogicAvatar a, LevelObstacle ob)
        {
            var delta = a.Position - ob.Center;
            var distSq = delta.SqrMagnitude();
            var minDist = a.Radius + ob.Radius;
            // 不重叠
            if (distSq >= minDist * minDist)
                return;

            if (distSq == Fixed64.Zero)
            {
                // 完全重合：沿 +X 推开
                a.Nudge(new FixedVector3(minDist, Fixed64.Zero, Fixed64.Zero));
                return;
            }

            var dist = Fixed64.Sqrt(distSq);
            var overlap = minDist - dist;
            var dir = delta / dist;          // 障碍物 → 角色
            a.Nudge(dir * overlap);          // 角色全量推出，障碍物不动
        }
    }
}