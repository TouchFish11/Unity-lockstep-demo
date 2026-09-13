using System;
using Core.Inputs;
using Core.Math;
using Core.Net.Protocols.FSync;
using Core.Net.SyncModule.Interface;
using Core.Time;
using HotUpdate.Game.Race.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HotUpdate.Game.Race.View
{
    public class ViewAvatar : MonoBehaviour, INetObject
    {
        private static readonly int Move = Animator.StringToHash(nameof(Move));
        private static readonly int Attack = Animator.StringToHash(nameof(Attack));
        private static readonly int Skill = Animator.StringToHash(nameof(Skill)); // 占位，改成 Animator 参数名

        [SerializeField] private float rotateSpeed = 20f;
        
        private IInputSystem _inputSystem;
        private Animator _animator;
        private LogicAvatar _logic;
        private float _lerpTime;
        private int _lastVersion = -1;
        private const float LogicFrameTime = 0.06f;     // 比逻辑 tick 的 0.066f 略小，让 _lerpTime 在 66ms 内能稳到 1（甚至略超被钳），保证位置追平逻辑位置。
        
        private FixedVector3 _pendingDir;   // 本地玩家当前移动方向

        private bool _pendingAttack;    // 一次按键发一次攻击
        private bool _attackTriggered;  // 检测 AnimState 从非 Attack 跳入 Attack，只 SetTrigger 一次
        private bool _pendingSkill;     // 一次按键发一次技能
        private bool _skillTriggered;   // 检测 AnimState 从非 Skill 跳入 Skill，只 SetTrigger 一次

        public float CurrentHp => _logic.Hp;
        
        public int MaxHp => _logic.MaxHp;
        
        private void Awake()
        {
            _animator = this.GetComponentInChildren<Animator>();
        }

        public void Bind(LogicAvatar logic, RuntimeAnimatorController controller)
        {
            _logic = logic;
            transform.position = logic.Position.ToVector3();
            _animator.runtimeAnimatorController = controller;
        }

        public void InitInput(IInputSystem inputSystem, PlayerInput playerInput)
        {
            inputSystem.InitPlayerInput(playerInput, InputCallback);
            inputSystem.Enable();
            _inputSystem = inputSystem;
        }
        
        private void InputCallback(InputAction.CallbackContext context)
        {
#if UNITY_STANDALONE_WIN
            OnPCControl(context);
#elif Unity_ANDROID
            OnAndroidControl(context);
#endif
        }

        private void OnPCControl(InputAction.CallbackContext context)
        {
            if (context.action.name == ActionConfigs.Move)
            {
                if (context.phase == InputActionPhase.Performed)
                {
                    var value = context.ReadValue<Vector2>();
                    _pendingDir = new FixedVector3(Fixed64.FromFloat(value.x), Fixed64.Zero, Fixed64.FromFloat(value.y));
                }
                else if (context.phase == InputActionPhase.Canceled)
                {
                    _pendingDir = FixedVector3.Zero;
                }
            }
            else if(context.action.name == ActionConfigs.Attack)
            {
                if (context.phase == InputActionPhase.Started)
                {
                    _pendingAttack = true;
                }
            }
            else if (context.action.name == ActionConfigs.Skill)
            {
                if (context.phase == InputActionPhase.Started && !_logic.IsCasting && !_logic.IsCooling)
                {
                    _pendingSkill = true;
                }
            }
        }

        private void OnAndroidControl(InputAction.CallbackContext context)
        {
            if (context.action.name == ActionConfigs.Move)
            {
                if (context.phase == InputActionPhase.Performed)
                {
                    var value = context.ReadValue<Vector2>();
                    _pendingDir = new FixedVector3(Fixed64.FromFloat(value.x), Fixed64.Zero, Fixed64.FromFloat(value.y));
                }
            }
            else if(context.action.name == ActionConfigs.Attack)
            {
                if (context.phase == InputActionPhase.Started)
                {
                    _pendingAttack = true;
                }
            }
            else if (context.action.name == ActionConfigs.Skill)
            {
                if (context.phase == InputActionPhase.Started && !_logic.IsCasting && !_logic.IsCooling)
                {
                    _pendingSkill = true;
                }
            }
        }
        
        private void Update()
        {
            if (_logic == null) 
                return;
            
            if (_logic.IsDead)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_logic.Version != _lastVersion) // 逻辑位置更新了，重置插值
            {
                _lastVersion = _logic.Version;
                _lerpTime = 0;
            }
            
            // 渲染移动
            _lerpTime += TimeUtil.DeltaTime / LogicFrameTime;
            this.transform.position = Vector3.Lerp(_logic.PrevPosition.ToVector3(), _logic.Position.ToVector3(), Mathf.Clamp01(_lerpTime));
            
            var dir = _logic.Position - _logic.PrevPosition;
            if (dir != FixedVector3.Zero)
            {
                // 旋转
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(dir.ToVector3()), TimeUtil.DeltaTime * rotateSpeed);
            }
            
            // 切换动画
            switch (_logic.AnimState)
            {
                case ELogicAnimState.Idle:
                    _animator.SetBool(Move, false);
                    _attackTriggered = false;
                    _skillTriggered = false;
                    break;
                case ELogicAnimState.Move:
                    _animator.SetBool(Move, true);
                    _attackTriggered = false;
                    _skillTriggered = false;
                    break;
                case ELogicAnimState.Attack:
                    if (!_attackTriggered)
                    {
                        _attackTriggered = true;
                        _animator.SetTrigger(Attack);
                    }
                    break;
                case ELogicAnimState.Skill:
                    if (!_skillTriggered)
                    {
                        _skillTriggered = true;
                        _animator.SetTrigger(Skill);
                    }
                    break;
            }
        }
        
        public void CollectInput(ref InputCommand cmd)
        {
            if (_pendingSkill)
            {
                _pendingSkill = false;
                cmd.optType = EOptType.UseSkill;
                cmd.skillId = AbilityTable.AoeSkillId;
                cmd.dir = FixedVector3.Zero;
            }
            else if (_pendingAttack)
            {
                _pendingAttack = false;
                cmd.optType = EOptType.Attack;
                cmd.dir = FixedVector3.Zero;    // 方向攻击不带方向，逻辑用 Facing
            }
            else
            {
                cmd.optType = EOptType.Move;
                cmd.dir = _pendingDir;
            }
        }

        private void OnDestroy()
        {
            _inputSystem?.ResetPlayerInput();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var radius = AbilityTable.Get(AbilityTable.AttackId).Range.ToInt();
            var angle = 120;
            var segments = 48;      // 弧线分段数
            
            Vector3 origin = transform.position;

            // 把 forward 压到 XZ 平面
            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.forward;
            forward.Normalize();

            // XZ 平面上的旋转轴固定是世界的 Y 轴
            Vector3 axis = Vector3.up;

            float half = angle * 0.5f;

            // 左右两条边
            Vector3 leftDir  = Quaternion.AngleAxis(-half, axis) * forward;
            Vector3 rightDir = Quaternion.AngleAxis( half, axis) * forward;

            Gizmos.DrawLine(origin, origin + leftDir  * radius);
            Gizmos.DrawLine(origin, origin + rightDir * radius);

            // 画弧
            Vector3 prev = origin + leftDir * radius;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float a = Mathf.Lerp(-half, half, t);
                Vector3 dir = Quaternion.AngleAxis(a, axis) * forward;
                Vector3 cur = origin + dir * radius;

                Gizmos.DrawLine(prev, cur);
                prev = cur;
            }

            // 可选：补上两条半径，把扇形封起来（如果只想看边框就删掉）
            Gizmos.DrawLine(origin, origin + leftDir * radius);
            Gizmos.DrawLine(origin, origin + rightDir * radius);
        }
    }
}