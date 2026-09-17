using System;
using System.Collections;
using System.IO;
using System.Text;
using Core.Serialize.Json;
using Core.Utility;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.AssetBundles.Management
{
    /// <summary>
    /// 首包 AB 拷贝器（协程版）
    /// Android 上 StreamingAssets 是 jar:file:// 只读虚拟路径，LoadFromFile / File.ReadAllText 读不了，
    /// 需在启动时把首包 AB 一次性拷到 persistentDataPath，之后 LoadAbPath 走 persistent。
    /// 用协程而非 ToTask，因为 ToTask 依赖的 TaskFactory 在 InitCore 里才 Configure，拷贝必须在 InitCore 之前。
    /// </summary>
    public static class StreamingAssetsCopier
    {
        public static IEnumerator CopyFirstPackageToPersistent()
        {
            var persistentAbDir = Path.Combine(Application.persistentDataPath, "AssetBundles");
            var catalogName = FileSources.CatalogDefaultName;

            // 已拷过则跳过
            if (File.Exists(Path.Combine(persistentAbDir, catalogName)))
                yield break;

            Directory.CreateDirectory(persistentAbDir);

            // 1. 读 StreamingAssets 里的 catalog
            var catalogPath = $"{Application.streamingAssetsPath}/AssetBundles/{catalogName}";
            using (var req = UnityWebRequest.Get(catalogPath))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                    throw new Exception($"读取 StreamingAssets 失败: {catalogPath}, {req.error}");

                var catalogBytes = req.downloadHandler.data;
                File.WriteAllBytes(Path.Combine(persistentAbDir, catalogName), catalogBytes);

                // 2. 解析 catalog，拿到所有 AB 包名
                var catalog = JsonConvert.DeserializeObject<AssetCatalog>(
                    Encoding.UTF8.GetString(catalogBytes), NewtonsoftJsonUtility.CatalogSerializerSettings);

                // 3. 逐个拷贝 AB 包
                foreach (var pair in catalog.ABPackageCollection)
                {
                    var abFileName = pair.Value.Name.WithAbSuffix();
                    var abPath = $"{Application.streamingAssetsPath}/AssetBundles/{abFileName}";
                    using var abReq = UnityWebRequest.Get(abPath);
                    yield return abReq.SendWebRequest();
                    if (abReq.result != UnityWebRequest.Result.Success)
                        throw new Exception($"读取 StreamingAssets 失败: {abPath}, {abReq.error}");
                    File.WriteAllBytes(Path.Combine(persistentAbDir, abFileName), abReq.downloadHandler.data);
                }
            }
        }
    }
}
