using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// 动画配置
    /// </summary>
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "Animation/AnimationConfig")]
    public class AnimationConfig : ScriptableObject
    {
        // 动画层级
        public EAnimationLayer layer;
        // 动画状态名称
        public string animationStateName;
        // 动画hash，由状态名获取
        public int animationHash;
        // 动画类型
        public EAnimationType animationType;
        // 是否循环
        public bool loop;
        // 过渡到下一个动画的时间
        public float crossFadeTime = 0.1f;
        // 忽略的动画类型，决定当前动画不能被这些类型打断
        public List<AnimationIgnore> ignores;
        // 该动画的下一个动画配置，没有则为null
        public AnimationConfig nextAnimConfig;

        private void OnValidate()
        {
            var nameWithLayer = $"{layer}.{animationStateName}";
            animationHash = Animator.StringToHash(nameWithLayer);
        }
    }
}
