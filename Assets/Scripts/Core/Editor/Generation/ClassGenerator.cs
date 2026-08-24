using System.IO;
using System.Text;
using UnityEditor;

namespace Core.Editor.Generation
{
    public abstract class ClassGenerator : IScriptGenerator
    {
        protected abstract string Note { get; }

        protected abstract string NameSpace { get; }

        protected abstract string ClassName { get; }
        
        public abstract string FilePath { get; }

        /// <summary>生成字段的访问修饰符</summary>
        /// <value>默认值：public</value>
        protected string accessModifier { get; private set; } = "public";

        /// <summary>生成字段的变量类型</summary>
        /// <value>默认值：string（字符串类型）</value>
        protected string variableType { get; private set; } = "string";
        
        /// <summary>生成字段的静态修饰符</summary>
        /// <value>默认值：static（静态）</value>
        protected string staticModifier { get; private set; } = "static";
        
        protected string constModifier { get; private set; } = "const";
        
        protected string partialModifier { get; private set; } = "partial";

        protected virtual StringBuilder AllocateCapacity(int capacity = 256)
        {
            return new StringBuilder(capacity);
        }
        
        public virtual void GenerateScript()
        {
            var sb = AllocateCapacity();
            // 前半部分
            ClassTemplate_FirstHalf(sb);
            // 内容
            ClassContent(sb);
            // 闭合类
            ClassTemplate_LatterHalf(sb);
            
            // 若目标文件已存在，先删除（确保覆盖最新内容）
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
            
            // 将构建好的代码字符串写入文件
            File.WriteAllText(FilePath, sb.ToString());
            // 刷新
            AssetDatabase.Refresh();
        }

        protected virtual void ClassTemplate_FirstHalf(StringBuilder stringBuilder)
        {
            // 构建命名空间和类的基础结构
            stringBuilder.AppendLine($"namespace {NameSpace}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("\t/// <summary>");
            stringBuilder.AppendLine($"\t/// {Note}");
            stringBuilder.AppendLine("\t/// </summary>");
            stringBuilder.AppendLine($"\tpublic {partialModifier} class {ClassName}");
            stringBuilder.AppendLine("\t{");
        }

        protected virtual void ClassTemplate_LatterHalf(StringBuilder stringBuilder)
        {
            // 闭合类和命名空间
            stringBuilder.AppendLine("\t}");
            stringBuilder.AppendLine("}");
        }

        protected abstract void ClassContent(StringBuilder stringBuilder);
    }
}
