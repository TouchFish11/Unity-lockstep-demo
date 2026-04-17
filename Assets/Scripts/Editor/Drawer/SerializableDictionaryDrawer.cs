using System.Collections.Generic;
using Core.Collection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Drawer
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<int, string>))]
// 按需添加你创建的所有具体字典类型
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;
        private Dictionary<string, ReorderableList> listCache = new();

        private ReorderableList GetList(SerializedProperty property)
        {
            string key = property.propertyPath;
            if (!listCache.TryGetValue(key, out var list) || list.serializedProperty.serializedObject.targetObject == null)
            {
                var keysProp = property.FindPropertyRelative("keys");
                var valuesProp = property.FindPropertyRelative("values");

                list = new ReorderableList(property.serializedObject, keysProp, true, true, true, true)
                {
                    drawHeaderCallback = (Rect rect) =>
                    {
                        EditorGUI.LabelField(rect, property.displayName);
                    },

                    drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        if (index >= keysProp.arraySize || index >= valuesProp.arraySize)
                            return;

                        rect.y += Spacing / 2;
                        rect.height = EditorGUIUtility.singleLineHeight;

                        var keyProp = keysProp.GetArrayElementAtIndex(index);
                        var valueProp = valuesProp.GetArrayElementAtIndex(index);

                        float halfWidth = (rect.width - 5) / 2;
                        var keyRect = new Rect(rect.x, rect.y, halfWidth, rect.height);
                        var valueRect = new Rect(rect.x + halfWidth + 5, rect.y, halfWidth, rect.height);

                        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
                        EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
                    },

                    elementHeightCallback = (int index) =>
                    {
                        if (index < 0 || index >= keysProp.arraySize || index >= valuesProp.arraySize)
                            return EditorGUIUtility.singleLineHeight + Spacing;

                        var keyProp = keysProp.GetArrayElementAtIndex(index);
                        var valueProp = valuesProp.GetArrayElementAtIndex(index);
                        float keyHeight = EditorGUI.GetPropertyHeight(keyProp, true);
                        float valueHeight = EditorGUI.GetPropertyHeight(valueProp, true);
                        return Mathf.Max(keyHeight, valueHeight) + Spacing;
                    },

                    onAddCallback = (ReorderableList l) =>
                    {
                        keysProp.arraySize++;
                        valuesProp.arraySize++;
                        // 为新元素设置安全的默认值
                        int newIndex = keysProp.arraySize - 1;
                        SetDefaultValue(keysProp.GetArrayElementAtIndex(newIndex));
                        SetDefaultValue(valuesProp.GetArrayElementAtIndex(newIndex));
                        property.serializedObject.ApplyModifiedProperties();
                    }
                };

                listCache[key] = list;
            }

            return list;
        }

        private void SetDefaultValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.String:
                    prop.stringValue = "";
                    break;
                case SerializedPropertyType.Integer:
                    prop.intValue = 0;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = 0f;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = false;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = Color.white;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = Vector3.zero;
                    break;
                case SerializedPropertyType.Enum:
                    if (prop.enumNames.Length > 0) prop.enumValueIndex = 0;
                    break;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var list = GetList(property);
            list.DoList(position);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var list = GetList(property);
            return list.GetHeight();
        }
    }
}