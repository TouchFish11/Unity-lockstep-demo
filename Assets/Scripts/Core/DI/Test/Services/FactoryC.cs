using UnityEngine;

namespace Core.DI.Test.Services
{
    public class FactoryC
    {
        [Inject] private ServiceB ServiceB;
        [Inject] private ServiceA ServiceA;
        
        public void DoSomething()
        {
            Debug.Log($"FactoryC:{ServiceA},{ServiceB}");
        }
    }
}
