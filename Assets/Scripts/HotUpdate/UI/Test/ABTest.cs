using Core.AssetBundles.Management;
using Core.DI;
using HotUpdate.Game.Main.Test;
using HotUpdate.Game.Main.UI;
using UnityEngine;
using Logger = Core.Log.Logger;

namespace HotUpdate.UI.Test
{
    public class ABTest : MonoBehaviour
    {
        // Start is called before the first frame update
        private async void Start()
        {
            await Core.Registration.RegisterCore.InitCore();
            var spawner = DIContainer.Create<ObjectSpawner>();
            
            var poolObject = await spawner.SpawnAsync<GameObject>(AssetKeys.Uiroot);
            Logger.Log(poolObject.Obj);
            
            var boss = await spawner.SpawnAsync<Boss>(AssetKeys.Boss);
            Logger.Log(boss.Obj);
        }
    }
}
