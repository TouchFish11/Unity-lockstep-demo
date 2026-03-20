using System.Collections.Generic;
using Input;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// 动画控制器
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorController : MonoBehaviour
    {
        [SerializeField] private List<AnimationConfig> animationConfigs;
        private AnimationConfig _currentConfig;
        private Animator _animator;
        
        public bool IsAttacking 
        {
            get 
            {
                var info = _animator.GetCurrentAnimatorStateInfo((int)EAnimationLayer.BaseLayer);
                return info.IsName(nameof(EAnimationType.Attack)) && info.normalizedTime < 1f;
            }
        }

        public bool IsDashing
        {
            get
            {
                var info = _animator.GetCurrentAnimatorStateInfo((int)EAnimationLayer.BaseLayer);
                return info.IsName(nameof(EAnimationType.Dash)) && info.normalizedTime < 1f;
            }
        }
        
        private void Awake()
        {
            _animator = this.GetComponent<Animator>();
            Play(EAnimationType.Idle);
        }

        private void Start()
        {
            InputManager.Instance.OnMove += OnMove;
            InputManager.Instance.OnAttack += OnAttack;
            InputManager.Instance.OnDash += OnDash;
            InputManager.Instance.OnJump += OnJump;
        }

        private void OnJump()
        {
            Play(EAnimationType.Jump);
        }

        private void OnDash()
        {
            Play(EAnimationType.Dash);
        }

        private void OnAttack()
        {
            Play(EAnimationType.Attack);
        }

        private void OnMove(Vector2 vector2)
        {
            if (_currentConfig.animationType == EAnimationType.Run && vector2 == Vector2.zero)
            {
                Play(EAnimationType.Idle);
            }
            else if (_currentConfig.animationType == EAnimationType.Idle && vector2 != Vector2.zero)
            {
                Play(EAnimationType.Run);
            }
        }
        
        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="type"></param>
        /// <returns>播放成功返回true，反正false</returns>
        public void Play(EAnimationType type)
        {
            // 是否忽略该类型动画
            if (IsIgnore(type))
            {
                return;
            }

            // 是否存在该类型动画
            if (!TryGetConfig(type, out var config))
            {
                return;
            }
            
            // 切换动画
            PlayInternal(
                config.animationHash, 
                !_currentConfig ? 0 : _currentConfig.crossFadeTime,
                (int)EAnimationLayer.BaseLayer, 
                0); 
            
            // 更新当前配置
            _currentConfig = config;
            RefreshIgnores();
        }

        internal void PlayInternal(int stateHashName, float normalizedTransitionDuration, int layer, float normalizedTimeOffset)
        {
            _animator.CrossFade(stateHashName, normalizedTransitionDuration, layer, normalizedTimeOffset);
        }

        /// <summary>
        /// 更新忽略时间
        /// </summary>
        private void UpdateIgnores()
        {
            for (var i = _currentConfig.ignores.Count - 1; i >= 0; i--)
            {
                var ignore = _currentConfig.ignores[i];
                ignore.Update(Time.deltaTime);
            }
        }
        
        /// <summary>
        /// 刷新忽略时间
        /// </summary>
        private void RefreshIgnores()
        {
            foreach (var currentConfigIgnore in _currentConfig.ignores)
            {
                currentConfigIgnore.Reset();
            }
        }
        
        /// <summary>
        /// 是否忽略该动画类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns>true为忽略，false为不忽略</returns>
        public bool IsIgnore(EAnimationType type)
        {
            if (!_currentConfig)
            {
                return false;
            }
            
            foreach (var currentConfigIgnore in _currentConfig.ignores)
            {
                // 当前动画不能被打断
                if (!currentConfigIgnore.IgnoreOver && (currentConfigIgnore.ignoreType & type) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试获取动画配置
        /// </summary>
        /// <param name="type"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public bool TryGetConfig(EAnimationType type, out AnimationConfig config)
        {
            var cacheConfig = animationConfigs.Find(config => config.animationType == type);
            if (cacheConfig)
            {
                config = cacheConfig;
                return true;
            }
            config = null;
            return false;
        }

        private void Update()
        {
            UpdateIgnores();
            
            // 当前动画是否播放完毕
            CheckAnimationFinished();
        }
        
        /// <summary>
        /// 检测非循环动画播放完成
        /// </summary>
        private void CheckAnimationFinished()
        {
            // 获取当前播放的动画信息
            var stateInfo = _animator.GetCurrentAnimatorStateInfo((int)EAnimationLayer.BaseLayer);
            if (!_currentConfig.loop && stateInfo.fullPathHash == _currentConfig.animationHash && stateInfo.normalizedTime >= 1f)
            {
                Debug.Log($"当前动画：{_currentConfig.animationStateName}：{stateInfo.fullPathHash}播放完成");
                if (_currentConfig.nextAnimConfig)
                {
                    PlayInternal(_currentConfig.nextAnimConfig.animationHash, _currentConfig.crossFadeTime, (int)EAnimationLayer.BaseLayer, 0);
                    _currentConfig = _currentConfig.nextAnimConfig;
                    RefreshIgnores();
                }
                else
                {
                    Play(EAnimationType.Idle);
                }
            }
        }
    }
}
