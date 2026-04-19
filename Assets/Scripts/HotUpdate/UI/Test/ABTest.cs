using System.Threading.Tasks;
using Core.AssetBundles.Management;
using Core.DI;
using HotUpdate.Game.Main.Test;
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
            
            // var poolObject = await spawner.SpawnAsync<GameObject>(AssetKeys.Uiroot);
            // Logger.Log(poolObject.Obj);
            //
            // var boss = await spawner.SpawnAsync<Boss>(AssetKeys.Boss);
            // Logger.Log(boss.Obj);
            //
            // var sphere = await spawner.SpawnAsync<Monster>(AssetKeys.Sphere);
            // Logger.Log(sphere.Obj);
            //
            // boss.Collect();
            // sphere.Collect();
            //
            // boss = await spawner.SpawnAsync<Boss>(AssetKeys.Boss);
            // Logger.Log(boss.Obj);
            
            var pos = await spawner.SpawnsAsync<GameObject>(AssetKeys.Boss, AssetKeys.Sphere);
            foreach (var posObj in pos.Objs)
            {
                Logger.Log(posObj);
            }
            
            // await ABTest_TextAsset();
            // await ABTest_TextAsset();
            //
            // await Task.WhenAll(ABTest_GameObject(), ABTest_GameObject());
        }
        
        private static async Task ABTest_GameObject()
        {
            var handle = await GameAsset.LoadAllAssetAsync<GameObject>(AssetBundleKeys.Prefab);
            foreach (var o in handle.Asset)
            {
                Object.Instantiate(o);
            }
            //GameAsset.Release(handle);
        }

        private static async Task ABTest_TextAsset()
        {
            var handle = await GameAsset.LoadAllAssetAsync<TextAsset>(AssetBundleKeys.Hotupdate);
            foreach (var textAsset in handle.Asset)
            {
                Logger.Log(textAsset.name);
            }
            GameAsset.Release(handle);
            Logger.Log($"-------------------------");
        }
    }
}
