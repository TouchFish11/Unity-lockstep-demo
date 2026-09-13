using System.Collections.Generic;
using Core.Math;
using Core.Net.Protocols.FSync;

namespace HotUpdate.Game.Race.Logic
{
    public class LogicAvatar
    {
        private static readonly Fixed64 LogicDeltaTime = Fixed64.FromFloat(0.066f); // 66ms
        
        private readonly CharacterConfig _config;
        private int _abilityTimer = -1;                       // -1=不在施法中
        private AbilityConfig _currentAbility;                // 当前施放的能力，null=无
        private readonly Dictionary<int, int> _cooldowns = new(); // abilityId → 剩余冷却帧
        
        /// <summary>
        /// 玩家比赛ID，AI也复用
        /// </summary>
        public int PlayerId { get; }
        
        /// <summary>
        /// 当前逻辑位置
        /// </summary>
        public FixedVector3 Position { get; private set; }
        
        /// <summary>
        /// 上一次的逻辑位置
        /// </summary>
        public FixedVector3 PrevPosition { get; private set; }
        
        /// <summary>
        /// 动画状态
        /// </summary>
        public ELogicAnimState AnimState { get; private set; }
        
        /// <summary>
        /// 移动位置版本号，碰撞导致的位移不影响
        /// </summary>
        public int Version { get; private set; }

        /// <summary>
        /// 碰撞半径
        /// </summary>
        public Fixed64 Radius => _config.radius;
        
        /// <summary>
        /// 朝向，非零移动时更新
        /// </summary>
        public FixedVector3 Facing { get; private set; }      
        
        /// <summary>
        /// 最大血量
        /// </summary>
        public int MaxHp => _config.maxHp;
        
        /// <summary>
        /// 当前血量
        /// </summary>
        public int Hp { get; private set; }
        
        /// <summary>
        /// 是否正在释放攻击
        /// </summary>
        public bool IsCasting  => _abilityTimer >= 0;
        
        /// <summary>
        /// 技能正在冷却
        /// </summary>
        public bool IsCooling => _cooldowns.TryGetValue(AbilityTable.AoeSkillId, out var cd) && cd > 0;
        
        /// <summary>
        /// 是否处于命中帧
        /// </summary>
        public bool IsHitFrame => _currentAbility != null && _abilityTimer == _currentAbility.HitFrame;
        
        /// <summary>
        /// 当前技能配置
        /// </summary>
        public AbilityConfig CurrentAbility => _currentAbility;
        
        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool IsDead => Hp <= 0;
        
        /// <summary>
        /// 是否是玩家/怪物
        /// </summary>
        public bool IsPlayer => _config.isPlayer;
        
        public LogicAvatar(int playerId, CharacterConfig config) 
        {
            PlayerId = playerId;
            _config = config;
            AnimState = ELogicAnimState.Idle;
            Facing = new FixedVector3(Fixed64.Zero, Fixed64.Zero, Fixed64.One);
            Hp = config.maxHp;
        }

        public void Spawn(FixedVector3 pos)
        {
            Position = pos;
            PrevPosition = pos;   // 同时设，避免开局从原点"滑"到出生点
        }
        
        public void Execute(in InputCommand cmd)
        {
            switch (cmd.optType)
            {
                case EOptType.Move:
                    Move(cmd.dir);
                    break;
                case EOptType.Attack:
                    StartAbility(AbilityTable.AttackId);
                    break;
                case EOptType.UseSkill:
                    StartAbility(cmd.skillId);
                    break;
            }
        }

        public void Move(FixedVector3 dir)
        {
            // 死亡不能动
            if (IsDead)
                return;
            
            // 攻击中锁移动
            if (IsCasting)
                return;
            
            if (dir.SqrMagnitude() == Fixed64.Zero)
            {
                AnimState = ELogicAnimState.Idle;
                return;
            }

            PrevPosition = Position;
            // 收到的dir已经是单位向量的定点数，是否还需要归一化这里？
            Position += dir * _config.speed * LogicDeltaTime;
            Facing = dir.Normalized();
            AnimState = ELogicAnimState.Move;
            // 位置变了，版本号 +1
            Version++;          
        }

        public void StartAbility(int abilityId)
        {
            if (IsDead || IsCasting)
                return;

            var cfg = AbilityTable.Get(abilityId);
            if (cfg == null)
                return;

            if (_cooldowns.TryGetValue(abilityId, out var cd) && cd > 0)
                return;

            _currentAbility = cfg;
            _abilityTimer = 0;
            AnimState = cfg.AnimState;
            if (cfg.Cooldown > 0)
                _cooldowns[abilityId] = cfg.Cooldown;
        }
        
        /// <summary>
        /// 转向指定方向（怪物攻击前朝向玩家）
        /// </summary>
        public void FaceToward(FixedVector3 dir)
        {
            if (dir.SqrMagnitude() != Fixed64.Zero)
            {
                Facing = dir.Normalized();
            }
        }
        
        public void Tick()
        {
            if (IsDead)
                return;

            TickCooldowns();

            if (_abilityTimer < 0)
                return;

            _abilityTimer++;
            if (_abilityTimer >= _currentAbility.Duration)
            {
                _abilityTimer = -1;
                _currentAbility = null;
                AnimState = ELogicAnimState.Idle;
            }
        }
        
        private void TickCooldowns()
        {
            if (_cooldowns.Count == 0)
                return;

            // 快照键，避免迭代中改字典
            foreach (var key in new List<int>(_cooldowns.Keys))
            {
                if (_cooldowns[key] > 0)
                    _cooldowns[key]--;
            }
        }
        
        public void TakeDamage(int damage)
        {
            Hp -= damage;
            if (Hp <= 0)
            {
                Hp = 0; // 只扣血不死亡，锁在 0
            }
        }

        /// <summary>
        /// 碰撞修正：只平移 Position，不改 PrevPosition/Version（插值目标跟随最终位置）
        /// </summary>
        public void Nudge(FixedVector3 delta)
        {
            Position += delta;
        }
    }
}