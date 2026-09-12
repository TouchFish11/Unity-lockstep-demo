using Core.Pool;
using UnityEngine.Events;

namespace Core.Time
{
    /// <summary>
    /// 定时器
    /// </summary>
    public class Timer : IPoolData
    {
        // 定时器唯一ID
        private int _id;
        // 剩余时间(ms)
        private int _nowTime;
        // 总时长(ms)
        private int _maxTime;
        // 当前剩余间隔时间(ms)
        private int _nowIntervalTime;
        // 最大间隔时间(ms)
        private int _maxIntervalTime;
        // 定时到时的回调
        private  UnityAction _allTimeOverCallBack;
        // 间隔到时的回调
        private  UnityAction _intervalTimeOverCallBack;
        // 是否正在计时
        private bool _isRunning;

        /// <summary>
        /// 初始化定时器
        /// </summary>
        /// <param name="id">唯一ID</param>
        /// <param name="maxTime">总时长</param>
        /// <param name="timeOverCallBack">到时回调</param>
        /// <param name="maxIntervalTime">可选的间隔时长</param>
        /// <param name="intervalTimeOverCallBack">可选的间隔到时回调</param>
        public void InitTimer(int id, int maxTime, UnityAction timeOverCallBack, int maxIntervalTime = 0, UnityAction intervalTimeOverCallBack = null)
        {
            _id = id;
            _maxTime = _nowTime = maxTime;
            _maxIntervalTime = _nowIntervalTime = maxIntervalTime;
            _allTimeOverCallBack = timeOverCallBack;
            _intervalTimeOverCallBack = intervalTimeOverCallBack;
            _isRunning = true;
        }

        /// <summary>
        /// 重置定时器
        /// </summary>
        public void ResetTimer()
        {
            _nowTime = _maxTime;
            _nowIntervalTime = _maxIntervalTime;
            _isRunning = true;
        }

        /// <summary>
        /// 定时到时的回调
        /// </summary>
        public void OverInvoke()
        {
            _allTimeOverCallBack?.Invoke();
        }

        /// <summary>
        /// 间隔到时的回调
        /// </summary>
        public void IntervalInvoke()
        {
            _intervalTimeOverCallBack?.Invoke();
            _nowIntervalTime = _maxIntervalTime;
        }

        public void ResetData()
        {
            _id = -1;
            _nowTime = _maxTime = 0;
            _nowIntervalTime = _maxIntervalTime = 0;
            IsRunning = false;
            // 清空委托
            _allTimeOverCallBack = null;
            _intervalTimeOverCallBack = null;
        }

        /// <summary>
        /// 是否正在计时
        /// </summary>
        public bool IsRunning { get => _isRunning; set => _isRunning = value; }

        /// <summary>
        /// 剩余总时长
        /// </summary>
        public int NowTime { get => _nowTime; set => _nowTime = value; }

        /// <summary>
        /// 剩余的间隔时长
        /// </summary>
        public int NowIntervalTime { get => _nowIntervalTime; set => _nowIntervalTime = value; }

        /// <summary>
        /// 定时器唯一ID
        /// </summary>
        public int Id { get => _id; }
    }
}
