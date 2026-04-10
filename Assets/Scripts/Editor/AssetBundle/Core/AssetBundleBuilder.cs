using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.AssetBundles.Update.Collection;
using Core.Serialize.Json;
using Core.Utility;
using UnityEditor;

namespace Editor.AssetBundle.Core
{
    /// <summary>
    /// 负责 AB 包的构建、清理、拷贝到 ServerData/StreamingAssets
    /// </summary>
    public class AssetBundleBuilder
    {
        private readonly Action<string> logAction;
        private readonly Action<string, float> progressAction;

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
            return true;
        }

        /// <summary>
        /// 拷贝构建好的 AB 包到 ServerData 目录，并合并更新清单文件
        /// </summary>
        public void CopyToServerData(string outputPath, string serverDataPath, 
                                     AssetBundlesCollections releaseCollection,
                                     JsonManager jsonManager, BuildTarget target)
        {
            if (!Directory.Exists(outputPath))
            {
                Log($"Output path does not exist：{outputPath}，Please create path");
                return;
            }

            AssetBundleUtility.EnsureDirectoryExists(serverDataPath);
            Log("--- Start Copy To ServerData ---");

            var serverFiles = Directory.GetFiles(serverDataPath);
            string listFilePath = Path.Combine(serverDataPath, FileUtility.ListFileDefaultName);
            string outputListFilePath = Path.Combine(outputPath, FileUtility.ListFileDefaultName);

            // 首次全量拷贝
            if (serverFiles.Length == 0)
            {
                var outputFiles = Directory.GetFiles(outputPath);
                foreach (var file in outputFiles)
                {
                    var fileName = Path.GetFileName(file);
                    if (fileName.EndsWith(".meta") || fileName.EndsWith(".manifest") || fileName == AssetBundleUtility.GetPlatformBundleName(target))
                        continue;
                    File.Copy(file, Path.Combine(serverDataPath, fileName), true);
                }
                Log("Full Copy To ServerData");
            }
            else
            {
                // 增量拷贝
                var serverCollection = jsonManager.FromJson<ABPackageCollection>(File.ReadAllText(listFilePath));
                ABPackageCollection outputCollection = null;
                try
                {
                    outputCollection = jsonManager.FromJson<ABPackageCollection>(File.ReadAllText(outputListFilePath));
                }
                catch { /* 可能不存在 */ }

                // 更新或新增
                if (outputCollection != null)
                {
                    foreach (var outInfo in outputCollection.Values)
                    {
                        if (serverCollection.TryGetValue(outInfo.Name, out var serverInfo))
                        {
                            serverInfo.Size = outInfo.Size;
                            serverInfo.Hash = outInfo.Hash;
                            // 合并依赖
                            var deps = serverInfo.Dependencies.ToList();
                            foreach (var dep in outInfo.Dependencies)
                                if (!deps.Contains(dep)) deps.Add(dep);
                            serverInfo.Dependencies = deps.ToArray();
                            Log($"Update Info：{outInfo.Name}");
                        }
                        else
                        {
                            serverCollection.TryAdd(outInfo.Name, outInfo);
                            Log($"Add NewInfo：{outInfo.Name}");
                        }
                        File.Copy(Path.Combine(outputPath, outInfo.Name), Path.Combine(serverDataPath, outInfo.Name), true);
                    }
                }

                // 移除不再需要的包（以 releaseCollection 为准）
                var toRemove = new List<string>();
                foreach (var info in serverCollection.Values)
                {
                    string bundleName = Path.GetFileNameWithoutExtension(info.Name);
                    if (releaseCollection != null && !releaseCollection.assetBundleInfos.Exists(ab => ab.assetBundleName == bundleName))
                        toRemove.Add(info.Name);
                }
                foreach (var name in toRemove)
                {
                    string bundleName = Path.GetFileNameWithoutExtension(name);
                    foreach (var info in serverCollection.Values)
                    {
                        var deps = info.Dependencies.ToList();
                        if (deps.Contains(bundleName))
                        {
                            deps.Remove(bundleName);
                            info.Dependencies = deps.ToArray();
                        }
                    }
                    serverCollection.Remove(name);
                    File.Delete(Path.Combine(serverDataPath, name));
                    Log($"Remove Info：{name}");
                }

                jsonManager.SaveToJson(serverCollection, listFilePath);
                AssetDatabase.Refresh();
            }

            Log("--- End Copy To ServerData ---\n");
            AssetDatabase.Refresh();
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

            int total = selectedAssets.Length / 2;
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
    }
}