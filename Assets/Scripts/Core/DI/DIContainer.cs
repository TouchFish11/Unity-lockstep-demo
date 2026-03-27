using System;
using System.Collections.Generic;
using System.Reflection;

namespace Core.DI
{
    /// <summary>
    /// 依赖容器
    /// </summary>
    public class DIContainer
    {
        private static readonly Dictionary<Type, object> _dependencies = new();
        private const BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// 创建类型单例
        /// </summary>
        /// <param name="args">类型构造函数的参数</param>
        /// <typeparam name="T">作为单例的类型</typeparam>
        public static void BindSingleton<T>() where T : class
        {
            var constructorInfo = typeof(T).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            _dependencies.Add(typeof(T), constructorInfo?.Invoke(null));
        }

        /// <summary>
        /// 注入实例，用于后续注入的实例，可以其它依赖通过GetDependency主动获取。相同类型只能对应唯一实例
        /// </summary>
        /// <param name="instance">类型实例</param>
        /// <typeparam name="T">实例类型</typeparam>
        public static void InjectInstance<T>(T instance)
        {
            _dependencies.TryAdd(typeof(T), instance);
        }
        
        /// <summary>
        /// 注入依赖项，遍历所有注入的类型单例，注入其各自的依赖项。在所有CreateSingleton方法调用完后执行
        /// </summary>
        public static void InjectDependencies()
        {
            foreach (var instance in _dependencies.Values)
            {
                var fieldInfos = instance.GetType().GetFields(_bindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    if(!Attribute.IsDefined(fieldInfo, typeof(InjectAttribute))) continue;
                    
                    var value = _dependencies.GetValueOrDefault(fieldInfo.FieldType);
                    
                    if(value == null) continue;
                    fieldInfo.SetValue(instance, value);
                }
            }
        }

        /// <summary>
        /// 获取依赖
        /// </summary>
        /// <typeparam name="T">传入实例实现的接口类型</typeparam>
        /// <returns></returns>
        public static T GetDependency<T>() where T : class
        {
            foreach (var kvp in _dependencies)
            {
                if (kvp.Key.IsAssignableFrom(typeof(T)))
                {
                    return (T)kvp.Value;
                }
            }

            return null;
        }
        
        /// <summary>
        /// 移除依赖缓存，传入具体类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveDependency<T>()
        {
            return _dependencies.Remove(typeof(T));
        }

        /// <summary>
        /// 情况所有缓存
        /// </summary>
        public static void Clear()
        {
            _dependencies.Clear();
        }
    }
}
