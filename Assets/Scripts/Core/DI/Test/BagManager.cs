using Core.GlobalEvent;
using Core.Log;
using Core.Net;
using Core.Serialize.Json;

namespace Core.DI.Test
{
    public class BagManager
    {
        [Inject] private IJsonManager jsonManager;
        [Inject] private IUWRManager uWRManager;
        [Inject] private IEventCenter eventCenter;
        
        public void Test()
        {
            Logger.Log($"{nameof(BagManager)}.{nameof(Test)}: jsonManager is initialized {jsonManager}");
            Logger.Log($"{nameof(BagManager)}.{nameof(Test)}: uWRManager is initialized {uWRManager}");
            Logger.Log($"{nameof(BagManager)}.{nameof(Test)}: eventCenter is initialized {eventCenter}");
        }
    }
}
