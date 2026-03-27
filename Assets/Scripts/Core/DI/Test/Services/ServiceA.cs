using UnityEngine;

namespace Core.DI.Test.Services
{
    public class ServiceA
    {
        [Inject] private FactoryC FactoryC;
        [Inject] private ServiceB ServiceB;
        
        public void DoSomething()
        {
            Debug.Log($"Service A:{FactoryC},{ServiceB}");
        }
    }
}
