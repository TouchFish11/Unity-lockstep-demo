using Core.DI.Test.Services;
using UnityEngine;

namespace Core.DI.Test
{
    public class DITest : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
            // 创建单例
            // DIContainer.BindSingleton<ServiceA>();
            // DIContainer.BindSingleton<ServiceB>();
            // DIContainer.BindSingleton<FactoryC>();
            
            DIContainer.InjectDependencies();
        
            DIContainer.GetInstance<ServiceA>().DoSomething();
            DIContainer.GetInstance<ServiceB>().DoSomething();
            DIContainer.GetInstance<FactoryC>().DoSomething();
        }
    }
}
