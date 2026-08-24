using System.Text;
using UnityEngine.InputSystem;

namespace Core.Editor.Generation.Detail
{
    public class SingleBindingSourcesClassGenerator : ClassGenerator
    {
        private readonly InputActionAsset _actionAsset;
        protected override string Note => "所有Action的绑定Key集合，由编辑器工具生成";
        protected override string NameSpace => "Core.Inputs";
        protected override string ClassName => "BindingSources";
        public override string FilePath => $"Assets/Scripts/Core/Inputs/{ClassName}.Generate.cs";
        
        public SingleBindingSourcesClassGenerator(InputActionAsset inputActionAsset)
        {
            _actionAsset = inputActionAsset;
        }
        
        protected override void ClassContent(StringBuilder stringBuilder)
        {
            foreach (var map in _actionAsset.actionMaps)
            {
                foreach (var inputAction in map.actions)
                {
                    if (inputAction.type == InputActionType.Button)
                        continue;
                    
                    foreach (var binding in inputAction.bindings)
                    {
                        if (binding.isComposite)
                        {
                            continue;
                        }
                        
                        stringBuilder.AppendLine($"\t\t{accessModifier} {constModifier} {variableType} {binding.name} = \"{binding.name}\";");
                        stringBuilder.AppendLine();    
                    }
                }
            }
        }
    }
}
