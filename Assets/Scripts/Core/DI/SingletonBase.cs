using System;
using System.Reflection;

namespace Core.DI
{
    /// <summary>
    /// 单例基类
    /// </summary>
    /// <typeparam name="T">单例类型</typeparam>
    public sealed class SingletonBase<T> where T : class
    {
        private readonly Lazy<T> _instance = new(CreateInstance);
    
        public T Instance => _instance.Value;

        private static T CreateInstance()
        {
            var type = typeof(T);
            var info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (info == null) return null;
            return info.Invoke(null) as T;
        }
    }
}
