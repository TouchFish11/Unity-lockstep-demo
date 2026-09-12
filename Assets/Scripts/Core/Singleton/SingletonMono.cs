using UnityEngine;

namespace Core.Singleton
{
    /// <summary>
    /// 继承Mono的单例基类（手动创建）
    /// </summary>
    public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance => _instance;

        /// <summary>
        /// 当前是否存活
        /// </summary>
        public bool IsLive
        {
            get
            {
                if (_instance == null)
                    return false;
                else
                    return true;
            }
        }

        protected virtual void Awake()
        {
            // 已经存在该单例，为了避免切换场景时重复创建
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}
