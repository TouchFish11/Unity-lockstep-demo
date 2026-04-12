using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.AssetBundles.Management;
using Core.AssetBundles.Update.Collection;
using Core.DI;
using Core.Serialize.Json;
using Core.Utility;
using UnityEditor;
using UnityEngine;

namespace Editor.AssetBundle.Core
{
    /// <summary>
    /// 负责 AB 包的构建、清理、拷贝到 ServerData/StreamingAssets
    /// </summary>
    public class AssetBundleBuilder
    {
        private readonly Action<string> logAction;
        private readonly Action<string, float> progressAction;
        private readonly IJsonManager jsonManager = DIContainer.Create<JsonManager>();
        public const string AssetCatalogName = "AssetCatalog.json";
        
        public AssetBundleBuilder(Action<string> logAction = null, Action<string, float> progressAction = null)
        {
            this.logAction = logAction;
            this.progressAction = progressAction;
        }

        private void Log(string msg) => logAction?.Invoke(msg);
        private void Progress(string msg, float val) => progressAction?.Invoke(msg, val);

        /// <summary>
        /// 构建 AssetBundles
        /// </summary>
        public bool Build(string outputPath, BuildTarget target, BuildAssetBundleOptions options, string assetsInputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Log($"Output path does not exist：{outputPath}，Please create path");
                return false;
            }

            if (Directory.CreateDirectory(assetsInputPath).GetFiles().Length == 0)
            {
                Log("no files to be packaged at the resource input path");
                return false;
            }

            Log("--- Starting Build ---");
            AssetBundleUtility.ClearDirectory(outputPath);

            var startTime = DateTime.Now;
            var manifest = BuildPipeline.BuildAssetBundles(outputPath, options, target);
            var duration = DateTime.Now - startTime;

            if (manifest)
            {
                Log($"Build successful! Took {duration.TotalSeconds:F2} seconds.");
                Log($"Build Count：{manifest.GetAllAssetBundles().Length}");
                Log($"Build Include：{string.Join('、', manifest.GetAllAssetBundles())}.");
            }
            else
            {
                Log("Build failed! Check console for errors.");
                return false;
            }

            AssetDatabase.Refresh();

            // 重命名为 .assetBundle
            var dirInfo = new DirectoryInfo(outputPath);
            foreach (var file in dirInfo.GetFiles())
            {
                if (file.Extension == ".manifest" || file.Extension == ".meta")
                {
                    if (file.Extension == ".meta") file.Delete();
                    continue;
                }
                var newPath = Path.ChangeExtension(file.FullName, FileUtility.AbSuffix);
                if (File.Exists(newPath)) File.Delete(newPath);
                File.Move(file.FullName, newPath);
            }
            Log($"Rename Extension To：{FileUtility.AbSuffix}");
            AssetDatabase.Refresh();
            Log("--- Build End ---\n");

            var catalog = GenerateAssetCatalog(outputPath, target);
            if (catalog == null)
                return false;
            
            var scriptPath = Path.Combine(Application.dataPath, "Scripts", "HotUpdate", "Data", "Generated", "AssetKeys.cs");
            AssetKeyGenerator.Generate(catalog, scriptPath);
            return true;
        }

        /// <summary>
        /// 拷贝构建好的 AB 包到 ServerData 目录，并合并更新目录文件
        /// </summary>
        public void CopyToServerData(string outputPath, string serverDataPath, AssetBundlesCollections releaseCollection, BuildTarget target)
        { 
            if (!Directory.Exists(outputPath))
            {
                Log($"Output path does not exist：{outputPath}");
                return;
            }

            AssetBundleUtility.EnsureDirectoryExists(serverDataPath);
            Log("--- Start Copy To ServerData ---");

            var srcCatalogPath = Path.Combine(outputPath, AssetCatalogName);
            var dstCatalogPath = Path.Combine(serverDataPath, AssetCatalogName);

            if (!File.Exists(srcCatalogPath))
            {
                Log($"The source directory is missing {AssetCatalogName}. Please build it first.");
                return;
            }

            // 读取本次生成的资源目录
            var newCatalogJson = File.ReadAllText(srcCatalogPath);
            var newCatalog = jsonManager.FromJson<AssetCatalog>(newCatalogJson);

            AssetCatalog serverCatalog = null;
            if (File.Exists(dstCatalogPath))
            {
                serverCatalog = jsonManager.FromJson<AssetCatalog>(File.ReadAllText(dstCatalogPath));
            }

            // 拷贝所有 .assetBundle 文件（只拷贝变化的）
            foreach (var (abFileName, newAbInfo) in newCatalog.ABPackageCollection)
            {
                var srcFilePath = Path.Combine(outputPath, abFileName);
                var dstFilePath = Path.Combine(serverDataPath, abFileName);

                var needCopy = true;
                if (serverCatalog != null && serverCatalog.ABPackageCollection.TryGetValue(abFileName, out var oldInfo))
                {
                    if (oldInfo.Hash == newAbInfo.Hash)
                    {
                        needCopy = false;
                        Log($"跳过未变化：{abFileName}");
                    }
                }

                if (needCopy)
                {
                    File.Copy(srcFilePath, dstFilePath, true);
                    Log($"已拷贝：{abFileName}");
                }
            }

            // 处理移除：releaseCollection 中不存在的包，从服务器目录删除
            if (releaseCollection)
            {
                var toDeletes = new List<string>();
                foreach (var file in Directory.GetFiles(serverDataPath, $"*{FileUtility.AbSuffix}"))
                {
                    var fileName = Path.GetFileName(file);
                    var bundleName = Path.GetFileNameWithoutExtension(fileName);
                    if (!releaseCollection.assetBundleInfos.Exists(ab => ab.assetBundleName == bundleName))
                    {
                        toDeletes.Add(file);
                    }
                }
                foreach (var file in toDeletes)
                {
                    File.Delete(file);
                    Log($"已删除：{Path.GetFileName(file)}");
                }
            }
            
            // 合并资源文件
            var finalCatalog = serverCatalog ?? new AssetCatalog();

            // 更新 Bundles 信息（以新构建的为准），这里处理的是新增/变化的包信息
            foreach (var kv in newCatalog.ABPackageCollection)
            {
                if (!finalCatalog.ABPackageCollection.ContainsKey(kv.Key))
                {
                    finalCatalog.ABPackageCollection.Add(kv.Key, kv.Value);
                }
                else
                {
                    finalCatalog.ABPackageCollection[kv.Key] = kv.Value;
                }
            }

            // 移除服务器上已不存在的包信息，这里处理不存在的包信息
            if (releaseCollection)
            {
                var bundlesToRemove = finalCatalog.ABPackageCollection.Keys
                    .Where(fileName => !releaseCollection.assetBundleInfos.Exists(ab =>
                        $"{ab.assetBundleName}{FileUtility.AbSuffix}" == fileName))
                    .ToList();
                foreach (var fileName in bundlesToRemove)
                {
                    finalCatalog.ABPackageCollection.Remove(fileName);
                }
            }

            /*
             不需要再手动遍历所有包去清理依赖项（即：其它包依赖已经移除的包，不需要手动处理，之前的逻辑已经隐式处理过了）
             如果某个包既没有变化，也不是移除包的上层依赖，它根本没有被重打包，那么它的依赖列表在旧清单中是什么样，在新清单中还是什么样。
             如果被移除的包恰好在它的旧依赖列表里，这种在逻辑上不可能发生：
             如果包 A 依赖了被移除的包 B，那么 A 一定会被反向依赖逻辑捕获并重打包。
             如果 A 没有被重打包，说明它不依赖 B，自然旧清单里也不会有 B 的依赖记录。
            */
            
            // 更新 Assets 映射（合并策略）
            // 先移除那些属于已删除包的资源条目
            var assetsToRemove = finalCatalog.Assets
                .Where(entry => !finalCatalog.ABPackageCollection.ContainsKey(entry.bundleName))
                .Select(entry => entry.key)
                .ToList();
            foreach (var key in assetsToRemove)
            {
                finalCatalog.RemoveEntry(key);
            }

            // 将新构建产生的资源映射添加/更新进去
            foreach (var entry in newCatalog.Assets)
            {
                if(finalCatalog.ContainsKey(entry.key))
                {
                    finalCatalog[entry.key] = entry;
                }
                else
                {
                    finalCatalog.AddEntry(entry.key, entry);
                }
            }

            // 保存合并后的清单到服务器目录
            var finalJson = jsonManager.ToJson(finalCatalog);
            File.WriteAllText(dstCatalogPath, finalJson);
            Log($"{AssetCatalogName} 已合并更新。");

            AssetDatabase.Refresh();
            Log("--- End Copy To ServerData ---\n");
        }

        /// <summary>
        /// 将选中的 AB 包拷贝到 StreamingAssets
        /// </summary>
        public void MoveToStreamingAssets(string streamingAssetsPath, UnityEngine.Object[] selectedAssets)
        {
            AssetBundleUtility.EnsureDirectoryExists(streamingAssetsPath);

            if (selectedAssets.Length == 0) return;

            // 清空目标目录
            AssetBundleUtility.ClearDirectory(streamingAssetsPath);
            AssetDatabase.Refresh();

            // 在 Unity 编辑器中，当你选择一个 .assetBundle 文件时，
            // Unity 的 Selection 系统会自动把同名的 .manifest 文件也视为选中状态（虽然界面上可能只高亮了一个文件）
            int total = selectedAssets.Count(asset => AssetDatabase.GetAssetPath(asset).Contains(FileUtility.AbSuffix));
            for (int i = 0; i < selectedAssets.Length; i++)
            {
                Progress($"Processing：{selectedAssets[i].name}", (float)i / total);
                string assetPath = AssetDatabase.GetAssetPath(selectedAssets[i]);
                string fileName = Path.GetFileName(assetPath);
                if (!fileName.Contains(FileUtility.AbSuffix)) continue;
                AssetDatabase.CopyAsset(assetPath, Path.Combine(streamingAssetsPath, fileName));
            }
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 清理输出目录
        /// </summary>
        public void CleanOutputDirectory(string outputPath)
        {
            if (Directory.Exists(outputPath))
            {
                try
                {
                    Directory.Delete(outputPath, true);
                    Directory.CreateDirectory(outputPath);
                    Log($"Cleaned output directory: {outputPath}");
                }
                catch (Exception e)
                {
                    Log($"Failed to clean directory: {e.Message}");
                }
                AssetDatabase.Refresh();
            }
            else
            {
                Log($"Output directory does not exist: {outputPath}");
            }
        }
        
        /// <summary>
        /// 生成资源目录文件（包含包信息和资源映射）
        /// </summary>
        private AssetCatalog GenerateAssetCatalog(string outputPath, BuildTarget target)
        {
            var platformBundleName = AssetBundleUtility.GetPlatformBundleName(target);
            var catalogPath = Path.Combine(outputPath, platformBundleName);
            if (!File.Exists(catalogPath))
            {
                Log($"主 Manifest 文件不存在：{catalogPath}，无法生成资源目录。");
                return null;
            }

            var catalog = new AssetCatalog();
            UnityEngine.AssetBundle mainBundle = null;
            try
            {
                mainBundle = UnityEngine.AssetBundle.LoadFromFile(catalogPath);
                if (!mainBundle)
                {
                    Log("无法加载主 AssetBundleManifest");
                    return null;
                }
                var manifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                if (!manifest)
                {
                    Log("无法获取 AssetBundleManifest 对象");
                    return null;
                }

                var allBundleNames = manifest.GetAllAssetBundles();
                var dirInfo = new DirectoryInfo(outputPath);
                var index = 0;
                foreach (var bundleName in allBundleNames)
                {
                    Progress($"Generating combined manifest: {bundleName}", (float)index++ / allBundleNames.Length);

                    // 对应的物理文件（已重命名为 .assetBundle）
                    var fileName = $"{bundleName}{FileUtility.AbSuffix}";
                    var filePath = Path.Combine(outputPath, fileName);
                    if (!File.Exists(filePath))
                    {
                        Log($"警告：AB包文件不存在 {filePath}");
                        continue;
                    }

                    // 添加包信息
                    var deps = manifest.GetAllDependencies(bundleName);
                    var fileInfo = new FileInfo(filePath);
                    var hash = HashUtility.GenerateFileSHA256Hash(filePath);
                    var pkgInfo = new ABPackageInfo(fileName, fileInfo.Length, hash, deps);
                    catalog.ABPackageCollection.TryAdd(fileName, pkgInfo);

                    // 加载该包的 manifest 以获取内部资源列表
                    var bundleManifestPath = Path.Combine(outputPath, fileName);
                    var assetBundle = UnityEngine.AssetBundle.LoadFromFile(bundleManifestPath);
                    if (assetBundle)
                    {
                        var assetPaths = assetBundle.GetAllAssetNames();
                        foreach (var assetPath in assetPaths)
                        {
                            // 决定 key：使用文件名（不含扩展名），若担心重名可改用完整路径
                            var key = Path.GetFileNameWithoutExtension(assetPath);
                            // 如果 key 已存在，使用完整路径作为备用
                            if (catalog.ContainsKey(key))
                            {
                                var path = assetPath.ToLowerInvariant();
                                Log($"资源名称重复：{key}，已使用路径替代：{path}，请调整命名");
                                key = path;
                            }
                            var entry = new AssetMapEntry(key, fileName, assetPath);
                            catalog.AddEntry(key, entry);
                        }
                        assetBundle.Unload(false);
                    }
                }

                // 保存 JSON
                var json = jsonManager.ToJson(catalog);
                var savePath = Path.Combine(outputPath, AssetCatalogName);
                File.WriteAllText(savePath, json);
                Log($"资源目录已生成：{savePath}\n");
                AssetDatabase.Refresh();
                return catalog;
            }
            catch (Exception e)
            {
                Log($"生成合并清单失败：{e.Message}\n{e.StackTrace}");
            }
            finally
            {
                if (mainBundle) mainBundle.Unload(false);
                UnityEngine.AssetBundle.UnloadAllAssetBundles(true);
                EditorUtility.ClearProgressBar();
            }
            return null;
        }
    }
}