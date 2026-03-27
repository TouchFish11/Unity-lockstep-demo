using UnityEngine;

namespace Core.DI.Test.Services
{
    public class ServiceB
    {
        [Inject]  private FactoryC FactoryC;
        [Inject]  private ServiceA ServiceA;
        
        public void DoSomething()
        {
            Debug.Log($"Service B:{FactoryC},{ServiceA}");
        }
    }
}
