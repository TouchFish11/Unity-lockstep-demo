using System;

namespace Core.Editor.ConfigData
{
    /// <summary>
    /// 字段模板
    /// </summary>
    [Serializable]
    public class FieldTemplate
    {
        public E_FieldType fieldType; // 字段类型
        public string fieldName; // 字段名称（如taskId）
        public string fieldDescription;    // 字段描述
        public bool key;    // 是否为主键

        public FieldTemplate(string fieldName, E_FieldType fieldType)
        {
            this.fieldName = fieldName;
            this.fieldType = fieldType;
        }
    }
}
