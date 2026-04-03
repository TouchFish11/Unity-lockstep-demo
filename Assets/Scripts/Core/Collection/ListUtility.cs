using Core.Collection.Generic;
using Core.DI;
using Core.Pool;

namespace Core.Collection
{
    /// <summary>
    /// 列表工具类
    /// </summary>
    public static class ListUtility
    {
        /// <summary>
        /// 获取可复用的List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static UniList<T> GetUniList<T>()
        {
            return DIContainer.GetInstance<IPoolManager>().GetData<UniList<T>>();
        }

        /// <summary>
        /// 缓存可复用的List
        /// </summary>
        /// <param name="uniList"></param>
        /// <typeparam name="T"></typeparam>
        public static void CollectUniList<T>(UniList<T> uniList)
        {
            DIContainer.GetInstance<IPoolManager>().PushData(uniList);
        }
    }
}
