using System.Threading.Tasks;
using Core.DI;
using Core.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace Core.AssetBundles.Management.Test
{
    public class ABTest : MonoBehaviour
    {
        [SerializeField] private Image imgTest;
        
        private ObjectSpawner  _objectSpawner;
        
        // Start is called before the first frame update
        private async void Start()
        {
            await Registration.RegisterCore.InitCore();

            //Test1();

            //Test2();

            //Test3();

            //Test4();

            //Test5();

            //Test6();

            //Test7();

            //Test8();

            //Test9();

            Test10();

            //Test11();
        }

        // 加载单个资源
        private async void Test1()
        {
            var handle = await GameAsset.LoadAssetAsync<GameObject>("Sphere");
            EngineUtility.Instantiate(handle.Asset);
        }
        
        // 串行加载同类同个资源
        private async void Test2()
        {
            var handle1 = await GameAsset.LoadAssetAsync<GameObject>("Sphere");
            EngineUtility.Instantiate(handle1.Asset);
            var handle2 = await GameAsset.LoadAssetAsync<GameObject>("Sphere");
            EngineUtility.Instantiate(handle2.Asset);
            
            GameAsset.Release(handle1);
            GameAsset.Release(handle2);
        }

        // 加载包所有同类资源
        private async void Test3()
        {
            var handle = await GameAsset.LoadAllAssetAsync<GameObject>("ui");
            foreach (var handleAsset in handle.Assets)
            {
                Debug.Log(handleAsset.name);
            }
            GameAsset.Release(handle);
        }
        
        // 批量加载同类资源
        private async void Test4()
        {
            var handle = await GameAsset.LoadAssetsAsync<GameObject>("UIRoot", "Battle_Panel");
            foreach (var handleAsset in handle.Assets)
            {
                Debug.Log(handleAsset.name);
            }
            
            GameAsset.Release(handle);
        }

        // 并行加载同类同一个资源
        private async void Test5()
        {
            var task1 = GameAsset.LoadAssetAsync<GameObject>("Sphere");
            var task2 = GameAsset.LoadAssetAsync<GameObject>("Sphere");
            
            var handles = await Task.WhenAll(task1, task2);
            foreach (var assetHandle in handles)
            {
                EngineUtility.Instantiate(assetHandle.Asset);
            }
        }
        
        // 并行加载同类不同资源
        private async void Test6()
        {
            var task1 = GameAsset.LoadAssetAsync<GameObject>("UIRoot");
            var task2 = GameAsset.LoadAssetAsync<GameObject>("Battle_Panel");
            
            var handles = await Task.WhenAll(task1, task2);
            foreach (var assetHandle in handles)
            {
                EngineUtility.Instantiate(assetHandle.Asset);
            }
        }

        // 加载资源后释放
        private async void Test7()
        {
            var handle = await GameAsset.LoadAssetAsync<GameObject>("Sphere");
            EngineUtility.Instantiate(handle.Asset);
            GameAsset.Release(handle);
        }
        
        // 加载资源后释放
        private async void Test8()
        {
            var handle = await GameAsset.LoadAssetAsync<TextAsset>("HotUpdate.Base.dll");
            Debug.Log(handle.Asset.name);
            GameAsset.Release(handle);
        }
        
        private async void Test9()
        {
            var handle = await GameAsset.LoadAllAssetAsync<TextAsset>("hotupdate");
            foreach (var handleAsset in handle.Assets)
            {
                Debug.Log(handleAsset.name);
            }

            GameAsset.Release(handle);
        }
        
        private async void Test10()
        {
            var handle1 = await GameAsset.LoadAssetAsync<Sprite>("banner");
            var handle2 = await GameAsset.LoadAssetAsync<Sprite>("btn-round-check");
            Debug.Log(handle1.Asset.name);
            Debug.Log(handle2.Asset.name);
            
            //imgTest.sprite = handle2.Asset;
            
            GameAsset.Release(handle1);
            GameAsset.Release(handle2);
        }
        
        private async void Test11()
        {
            var handle1 = await GameAsset.LoadAssetAsync<GameObject>("UIRoot");
            var handle2 = await GameAsset.LoadAssetAsync<GameObject>("Battle_Panel");
            
            Debug.Log(handle1.Asset.name);
            Debug.Log(handle2.Asset.name);
            
            GameAsset.Release(handle1);
            GameAsset.Release(handle2);
        }
    }
}
