using System;
using System.IO;
using Core.DI;
using Core.Serialize.Json;
using UnityEditor;
using UnityEngine;

namespace Editor.Json
{
    public class SOToJsonExporter : EditorWindow
    {
        [MenuItem("GameTool/Export SO To JSON")]
        public static void ExportSelectedSOToJson()
        {
            var selected = Selection.activeObject;
            if (!selected)
            {
                Debug.LogError("请先选择一个SO");
                return;
            }
            
            // 序列化数据
            var json = Activator.CreateInstance<JsonManager>().ToJson(selected, settings: Core.Utility.NewtonsoftJsonUtility.SerializerSettings);
            
            // 保存到文件
            var path = EditorUtility.SaveFilePanel(
                "保存JSON文件",
                $"{Application.dataPath}/Editor/ArtRes/GameConfig/",
                $"{selected.GetType()}",
                "json");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            File.WriteAllText(path, json);
            Debug.Log($"已导出到: {path}");
            
            // 刷新AssetDatabase
            AssetDatabase.Refresh();
        }
    }
}