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

        [SerializeField] private float rotateSpeed = 20f;
        
        private Animator _animator;
        private EOptType _currentOpt;
        private LogicAvatar _logic;
        private float _lerpTime;
        private int _lastVersion = -1;
        private const float LogicFrameTime = 0.06f;     // 比逻辑 tick 的 0.066f 略小，让 _lerpTime 在 66ms 内能稳到 1（甚至略超被钳），保证位置追平逻辑位置。
        
        private FixedVector3 _pendingDir;   // 本地玩家当前移动方向

        private bool isAttacking;
        private bool _isTriggerAttack;

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
            inputSystem.SwitchScheme("Android");
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
                _currentOpt = EOptType.Move;
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
                    _currentOpt = EOptType.Attack;
                }
            }
        }

        private void OnAndroidControl(InputAction.CallbackContext context)
        {
            if (context.action.name == ActionConfigs.Move)
            {
                _currentOpt = EOptType.Move;
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
                    _currentOpt = EOptType.Attack;
                }
            }
        }
        
        private void Update()
        {
            if (_logic == null) 
                return;

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
                    break;
                case ELogicAnimState.Move:
                    _animator.SetBool(Move, true);
                    break;
                case ELogicAnimState.Attack:
                    if (!_isTriggerAttack)
                    {
                        _isTriggerAttack = true;
                        isAttacking = true;
                        _animator.SetTrigger(Attack);
                    }
                    break;
            }
        }

        public void CollectInput(ref InputCommand cmd)
        {
            FixedVector3 sendDir;
            if (isAttacking)
            {
                _currentOpt = EOptType.Move;
                sendDir =  FixedVector3.Zero;
            }
            else
            {
                sendDir = _pendingDir;
            }
            
            cmd.optType = _currentOpt;
            cmd.dir = sendDir;
        }

        public void OnAttackEnd()
        {
            _isTriggerAttack = false;
            isAttacking = false;
        }
    }
}