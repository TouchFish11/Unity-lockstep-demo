using UnityEngine;

namespace Core.Pool
{
    /// <summary>
    /// 对象池管理器接口
    /// </summary>
    public interface IPoolManager
    {
        /// <summary>
        /// 获取未继承Mono的对象
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="nameSpace">可选参数：命名空间</param>
        /// <returns></returns>
        T GetData<T>(string nameSpace = "") where T : class, IPoolData, new();

        /// <summary>
        /// 获取缓存的对象
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <returns></returns>
        GameObject Get(string assetName);
        
        /// <summary>
        /// 缓存未继承Mono的对象
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="data">数据对象</param>
        /// <param name="nameSpace">可选参数：命名空间</param>
        void PushData<T>(T data, string nameSpace = "") where T : class, IPoolData, new();

        /// <summary>
        /// 缓存继承Mono的对象
        /// </summary>
        /// <param name="obj">游戏对象</param>
        void PushObj(GameObject obj);

        /// <summary>
        /// 清除指定资源缓存
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <returns>销毁的资源数量</returns>
        int ClearCache(string assetName);
        
        /// <summary>
        /// 清空缓存池
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 获取指定资源缓存的数量
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        int GetUnUsedCount(string assetName);
    }
}
