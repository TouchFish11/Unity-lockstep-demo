using Core.Pool;
using UnityEngine.Events;

namespace Core.Time
{
    /// <summary>
    /// 定时时间（目标日期时间）
    /// </summary>
    public class DateTime : IPoolData
    {
        // 指定目标天数
        private int _targetDay;
        // 指定目标小时
        private int _targetHour;
        // 指定目标分钟
        private int _targetMinute;
        // 指定目标秒
        private int _targetSecond;
        // 真实目标时间
        private System.DateTime realTargetTime;
        // 剩余时间为0时的回调
        public event UnityAction OverCallBack;

        /// <summary>
        /// 初始化定时时间
        /// </summary>
        /// <param name="currentTime">当前时间结构体</param>
        /// <param name="targetDay">指定天数</param>
        /// <param name="targetHour">指定小时</param>
        /// <param name="targetMin">指定分钟</param>
        /// <param name="targetSec">指定秒</param>
        /// <returns>定时时间</returns>
        public DateTime Init(System.DateTime currentTime, int targetDay, int targetHour, int targetMin, int targetSec)
        {
            _targetDay = targetDay;
            _targetHour = targetHour;
            _targetMinute = targetMin;
            _targetSecond = targetSec;

            // 校验天数
            CheckDays(currentTime);

            return this;
        }

        /// <summary>
        /// 计算剩余时间
        /// </summary>
        /// <param name="currentTime">当前时间结构体</param>
        /// <returns>剩余时间（秒）</returns>
        public long CalcRemainTime(System.DateTime currentTime)
        {
            // 如果当前时间已经超过目标时间，那么目标时间顺延到下一次的指定时间
            if (currentTime > realTargetTime)
            {
                // 校验天数
                CheckDays(currentTime);

                // 剩余时间小于等于0时执行回调
                if ((long)(realTargetTime - currentTime).TotalSeconds <= 0)
                {
                    OverCallBack?.Invoke();
                }

                return (long)(realTargetTime - currentTime).TotalSeconds;
            }
            // 否则目标时间就是今天的指定时间
            return (long)(realTargetTime - currentTime).TotalSeconds;
        }

        public void ResetData()
        {
            OverCallBack = null;
        }

        /// <summary>
        /// 校验天数
        /// </summary>
        /// <param name="currentTime">当前时间结构体</param>
        private void CheckDays(System.DateTime currentTime)
        {
            if (_targetDay != 0)
            {
                if (currentTime.Day + _targetDay > System.DateTime.DaysInMonth(currentTime.Year, currentTime.Month))
                {
                    int deltaDay = 0;
                    // 获取当前月共有多少天
                    int daysInMonth = System.DateTime.DaysInMonth(currentTime.Year, currentTime.Month);
                    // 跨月天数 = 当前天数 + 目标天数 - 当前月总天数
                    deltaDay = currentTime.Day + _targetDay - daysInMonth;
                    // 如果跨月天数大于当前月总天数
                    while (deltaDay > daysInMonth)
                    {
                        // 当前时间加一个月
                        currentTime = currentTime.AddMonths(1);
                        // 跨月天数减去当前月的天数
                        deltaDay = deltaDay - currentTime.Day;
                    }

                    realTargetTime = new System.DateTime(currentTime.Year, currentTime.Month, deltaDay, _targetHour, _targetMinute, _targetSecond);
                }
                else
                {
                    realTargetTime = new System.DateTime(currentTime.Year, currentTime.Month, currentTime.Day + _targetDay, _targetHour, _targetMinute, _targetSecond);
                }
            }
            else
            {
                realTargetTime = new System.DateTime(currentTime.Year, currentTime.Month, currentTime.Day + 1, _targetHour, _targetMinute, _targetSecond);
                if (currentTime < realTargetTime.AddDays(-1))
                {
                    realTargetTime = realTargetTime.AddDays(-1);
                }
            }
        }
    }
}
