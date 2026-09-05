using System.IO;
using UnityEditor;
using UnityEngine;

namespace Core.Editor.HybridCLR
{
    /// <summary>
    /// 热更新编辑器
    /// </summary>
    public class HybridCLRTool
    {
        public const string HybridCLRRoot =
            @"D:\UnityProject\AnimationSystem\HybridCLRData\AssembliesPostIl2CppStrip\StandaloneWindows64\";

        public const string TargetRoot = @"D:\UnityProject\AnimationSystem\Assets\Editor\ArtRes\HotUpdateAOT\";
        
        [MenuItem("GameTool/Copy AOT Dlls")]
        public static void CopyAOTDlls()
        {
#if True
            foreach (var aotDll in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                var srcPath = $"{HybridCLRRoot}{aotDll}";
                var dstPath = $"{TargetRoot}{aotDll}.bytes";
                File.Copy(srcPath, dstPath, true);
            }
#endif
            AssetDatabase.Refresh();
            Debug.Log("Copy AOT Dlls Done!");
        }
    }
}
