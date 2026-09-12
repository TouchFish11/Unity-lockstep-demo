using System.Collections.Generic;
using Core.DI;
using Core.Log;
using Core.Pool;
using UnityEngine.Events;

namespace Core.Time
{
    /// <summary>
    /// 时间校验（定时）
    /// </summary>
    public class TimeChecker : ITimeChecker
    {
        [Inject] private IPoolManager _poolManager;
        // 存储定时时间：Key 为唯一标识，Value 为定时时间
        private Dictionary<int, DateTime> _dateTimeDic = new();

        /// <summary>
        /// 定时时间的唯一标识
        /// </summary>
        private static int TIME_KEY;

        /// <summary>
        /// 创建目标时间
        /// </summary>
        /// <param name="currentTime">当前时间结构体</param>
        /// <param name="targetDay">指定天数</param>
        /// <param name="targetHour">指定小时</param>
        /// <param name="targetMin">指定分钟</param>
        /// <param name="targetSec">指定秒</param>
        /// <returns>定时时间对应的Key</returns>
        public int CreateTargetTime(System.DateTime currentTime, int targetDay, int targetHour, int targetMin, int targetSec)
        {
            // 从对象池获取 DateTime 实例
            var tagetTime = _poolManager.GetData<DateTime>();
            // 初始化定时时间
            tagetTime = tagetTime.Init(currentTime, targetDay, targetHour, targetMin, targetSec);
            // 存储到字典
            _dateTimeDic.Add(++TIME_KEY, tagetTime);
            // 返回定时时间对应的Key
            return TIME_KEY;
        }

        /// <summary>
        /// 添加定时时间监听
        /// </summary>
        /// <param name="key">定时时间对应的Key</param>
        /// <param name="overCallBack">超时回调</param>
        public void AddListener(int key, UnityAction overCallBack)
        {
            GetDateTime(key).OverCallBack += overCallBack;
        }

        /// <summary>
        /// 计算剩余时间
        /// </summary>
        /// <param name="current">当前时间</param>
        /// <param name="key">定时时间Key</param>
        /// <returns>当前剩余时间（秒）</returns>
        public long CalcRemainTime(System.DateTime current, int key)
        {
            if (_dateTimeDic.ContainsKey(key))
            {
                return _dateTimeDic[key].CalcRemainTime(current);
            }

            Logger.LogError(ELogTags.Time, $"未找到指定的定时时间KEY：{key}");
            return 0;
        }

        /// <summary>
        /// 获取Key对应的定时时间
        /// </summary>
        /// <param name="key">定时时间Key</param>
        /// <returns>定时时间</returns>
        public DateTime GetDateTime(int key)
        {
            if (_dateTimeDic.TryGetValue(key, out var dateTime))
            {
                return dateTime;
            }

            Logger.LogError(ELogTags.Time, $"未找到指定的定时时间KEY：{key}");
            return null;
        }

        /// <summary>
        /// 清空
        /// </summary>
        public void Clear()
        {
            _dateTimeDic.Clear();
            _dateTimeDic = null;
        }
    }
}
