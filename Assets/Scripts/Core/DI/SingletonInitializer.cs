using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Mono;
using Core.Singleton;

namespace Core.DI
{
    public class SingletonInitializer
    {
        public static async Task InitAsync(List<IInitializable> initializers)
        {
            Sort(initializers);
            
            foreach (var initializable in initializers)
            {
                await initializable.InitAsync();
            }
        }

        private static void Sort(List<IInitializable> initializers)
        {
            initializers.Sort((i1, i2) =>
            {
                if (i1.InitPriority > i2.InitPriority) return 1;
                if (i1.InitPriority < i2.InitPriority) return -1;
                return 0;
            });
        }
        
        /// <summary>
        /// 注册到适配器中
        /// </summary>
        public static void InitQuit(IMonoAdapter monoAdapter, List<IApplicationExitNotify> applicationExitNotifies)
        {
            monoAdapter.AddApplicationExitNotifies(applicationExitNotifies.ToArray());
        }
    }
}
