using UnityEngine;

namespace Core.DI.Test.Services
{
    public class ServiceA
    {
        private ServiceB ServiceB;

        public ServiceA(ServiceB ServiceB)
        {
            this.ServiceB = ServiceB;
        }
        
        public void DoSomething()
        {
            //Debug.Log($"Service A:{FactoryC},{ServiceB}");
        }
    }
}
