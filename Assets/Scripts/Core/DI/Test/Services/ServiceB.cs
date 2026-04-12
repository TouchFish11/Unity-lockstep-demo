using UnityEngine;

namespace Core.DI.Test.Services
{
    public class ServiceB
    {
        private ServiceA ServiceA;
        
        public ServiceB(ServiceA ServiceA)
        {
            this.ServiceA = ServiceA;
        }
        
        public void DoSomething()
        {
            //Debug.Log($"Service B:{FactoryC},{ServiceA}");
        }
    }
}
