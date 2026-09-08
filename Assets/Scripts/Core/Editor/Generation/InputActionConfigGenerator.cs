using System.Text;
using UnityEngine.InputSystem;

namespace Core.Editor.Generation.Detail
{
    /// <summary>
    /// 输入动作配置生成器
    /// </summary>
    public class InputActionConfigGenerator : ClassGenerator
    {
        private readonly InputActionAsset _actionAsset;
        protected override string Note => "动作配置，避免硬编码Action名称，由编辑器工具生成";
        protected override string NameSpace => "Core.Inputs";
        protected override string ClassName => "ActionConfigs";
        public override string FilePath => $"Assets/Scripts/Core/Inputs/{ClassName}.Generate.cs";

        public InputActionConfigGenerator(InputActionAsset inputActionAsset)
        {
            _actionAsset = inputActionAsset;
        }

        protected override void ClassContent(StringBuilder stringBuilder)
        {
            foreach (var map in _actionAsset.actionMaps)
            {
                foreach (var inputAction in map.actions)
                {
                    stringBuilder.AppendLine($"\t\t{accessModifier} {constModifier} {variableType} {inputAction.name} = \"{inputAction.name}\";");
                    stringBuilder.AppendLine();
                }
            }
        }
    }
}