using System;

namespace Core.DI.Test
{
    public class BagManager
    {
        public void Test()
        {
            DIContainer.GetInstance<BagManager>().Test();
        }
    }
}
