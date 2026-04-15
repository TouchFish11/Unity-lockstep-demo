using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Editor.HybridCLR
{
    /// <summary>
    /// 热更新编辑器
    /// </summary>
    public class HybridClrEditor
    {
        [MenuItem("GameTool/Analysis Dll Dependence")]
        public static void AnalysisDependence()
        {
            var assemblies = CompilationPipeline.GetAssemblies();
            foreach (var asm in assemblies)
            {
                if(asm.name.Contains("HotUpdate"))
                    continue;
                
                Debug.Log($"程序集: {asm.name} -> 依赖：{string.Join(',', new List<Assembly>(asm.assemblyReferences).ConvertAll(a => a.name))}");
            }
        }
    }
}
