using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DI;
using Core.HotUpdate;
using Core.Log;
using Core.Singleton;

namespace Core.Reflection
{
    /// <summary>
    /// 工厂管理器
    /// 管理器所有实现IFactory的工厂
    /// </summary>
    public class FactoryManager : IFactoryManager, IInitializable
    {
        [Inject] private IHotUpdateManager _hotUpdateManager;
        
        public int InitPriority => 1;
        // 工厂实例类型Type到工厂接口的映射
        private readonly Dictionary<Type, IFactory> typeToFactoryMap = new();

        private FactoryManager()
        {

        }

        public Task InitAsync()
        {
            var coreAssembly = _hotUpdateManager.GetCoreModule();
            FactoryUtility.ScanAllFactory(typeToFactoryMap, coreAssembly);
            return Task.CompletedTask;
        }

        public void InitHotFactorys()
        {
            var hotAssemblies = _hotUpdateManager.GetHotAssemblies();
            FactoryUtility.ScanAllFactory(typeToFactoryMap, hotAssemblies);
        }
        
        public TISubFactory GetFactory<TISubFactory, TFactory>() where TISubFactory : class, IFactory where TFactory : TISubFactory
        {
            if (typeToFactoryMap.TryGetValue(typeof(TFactory), out var factory))
            {
                return (TISubFactory)factory;
            }
            
            Logger.LogError($"未找到该工厂类型,{typeof(TFactory)}");
            return null;
        }
    }
}
