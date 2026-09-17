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
        public static string HybridCLRRoot => $"{Application.dataPath}/../HybridCLRData/AssembliesPostIl2CppStrip/{EditorUserBuildSettings.activeBuildTarget}/";

        public static string TargetRoot => $"{Application.dataPath}/Editor/ArtRes/HotUpdateAOT/";
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
