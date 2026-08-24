using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Core.Editor.Generation.Detail
{
    /// <summary>
    /// AB包键集合类生成器
    /// </summary>
    public class AbKeyCollectionClassGenerator : ClassGenerator
    {
        protected override string Note => "AB包键集合";
        protected override string NameSpace => "HotUpdate.Common";
        protected override string ClassName => "AbKeyCollection";
        public override string FilePath => $"{Application.dataPath}/Scripts/HotUpdate/Common/{ClassName}.cs";

        private readonly List<string> abNames;
        
        public AbKeyCollectionClassGenerator(IEnumerable<string> abNames)
        {
            this.abNames = new List<string>(abNames);
        }
        
        protected override void ClassContent(StringBuilder firstHalf)
        {
            // 遍历所有扫描到的文件，为每个文件生成静态字符串属性
            foreach (var abName in abNames)
            {
                // 生成一行：public static string 文件名 => "文件名";
                firstHalf.AppendLine($"\t\t{accessModifier} {staticModifier} {variableType} {abName} => \"{abName.ToLower()}\";");
            }
        }
    }
}
