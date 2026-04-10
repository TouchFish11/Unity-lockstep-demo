using System.IO;
using System.Security.Cryptography;
using Core.Utility;
using UnityEditor;

namespace Editor.AssetBundle.Core
{
    public static class AssetBundleUtility
    {
        /// <summary>
        /// 计算文件的 SHA256 哈希值
        /// </summary>
        public static string GenerateFileSHA256Hash(string filePath)
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha256.ComputeHash(stream);
            return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// 获取当前平台对应的主包名（如 StandaloneWindows64.assetBundle）
        /// </summary>
        public static string GetPlatformBundleName(BuildTarget target)
        {
            return $"{target}{FileUtility.AbSuffix}";
        }

        /// <summary>
        /// 确保目录存在，不存在则创建
        /// </summary>
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 清空目录下的所有文件
        /// </summary>
        public static void ClearDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            var dirInfo = new DirectoryInfo(path);
            foreach (var file in dirInfo.GetFiles())
                file.Delete();
        }
    }
}
