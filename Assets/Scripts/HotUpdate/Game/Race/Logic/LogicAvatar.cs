using Core.Math;
using Core.Net.Protocols.FSync;

namespace HotUpdate.Game.Race.Logic
{
    public class LogicAvatar
    {
        private static readonly Fixed64 LogicDeltaTime = Fixed64.FromFloat(0.066f); // 66ms
        
        // 攻击配置（对齐攻击动画：16帧@30fps=533ms，命中≈第4帧=133ms）
        public const int AttackDuration = 8;                  // 攻击总时长（逻辑帧）≈ 533ms
        public const int AttackHitFrame = 3;                  // 命中帧：133ms（StartAttack 后 Tick 已 +1，故 3 = 攻击指令后第 2 帧）
        public const int Damage = 10;                         // 伤害
        public const int MaxHp = 50;                         // 最大生命
        public static readonly Fixed64 AttackRange = Fixed64.FromFloat(2f);   // 圆心到圆心
        public static readonly Fixed64 HalfAngleCos = Fixed64.Half;             // cos60° = 0.5，扇面 120°
        
        private readonly Fixed64 _speed;
        private int _attackTimer = -1;                        // -1=不在攻击中
        
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
        public Fixed64 Radius { get; }
        
        /// <summary>
        /// 朝向，非零移动时更新
        /// </summary>
        public FixedVector3 Facing { get; private set; }      
        public int Hp { get; private set; }
        public bool IsAttacking => _attackTimer >= 0;
        public bool IsHitFrame => _attackTimer == AttackHitFrame;
        public bool IsDead => Hp <= 0;
        public bool IsPlayer => PlayerId >= 0;
        
        public LogicAvatar(int playerId, Fixed64 speed) : this(playerId, speed, Fixed64.FromFloat(0.5f))
        {

        }
        
        public LogicAvatar(int playerId, Fixed64 speed, Fixed64 radius)
        {
            PlayerId = playerId;
            _speed = speed;
            Radius = radius;
            AnimState = ELogicAnimState.Idle;
            Facing = new FixedVector3(Fixed64.Zero, Fixed64.Zero, Fixed64.One);
            Hp = MaxHp;
        }

        public void Execute(in InputCommand cmd)
        {
            switch (cmd.optType)
            {
                case EOptType.Move:
                    Move(cmd.dir);
                    break;
                case EOptType.Attack:
                    StartAttack();
                    break;
                case EOptType.UseSkill:
                    break; // 本期不实现
            }
        }

        public void Move(FixedVector3 dir)
        {
            // 死亡不能动
            if (IsDead)
                return;
            
            // 攻击中锁移动
            if (IsAttacking)
                return;
            
            if (dir.SqrMagnitude() == Fixed64.Zero)
            {
                AnimState = ELogicAnimState.Idle;
                return;
            }

            PrevPosition = Position;
            // 收到的dir已经是单位向量的定点数，是否还需要归一化这里？
            Position += dir * _speed * LogicDeltaTime;
            Facing = dir.Normalized();
            AnimState = ELogicAnimState.Move;
            // 位置变了，版本号 +1
            Version++;          
        }

        public void StartAttack()
        {
            // 死亡/攻击中 都不能再起手
            if (IsDead || IsAttacking)
                return;
            
            _attackTimer = 0;
            AnimState = ELogicAnimState.Attack;
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
        
        /// <summary>
        /// 每逻辑帧推进攻击状态机（命中帧由 IsHitFrame 暴露，供 World 结算）
        /// </summary>
        public void Tick()
        {
            if (IsDead)
                return;
            
            if (_attackTimer < 0)
                return;
            
            _attackTimer++;
            if (_attackTimer >= AttackDuration)
            {
                _attackTimer = -1;
                AnimState = ELogicAnimState.Idle;
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