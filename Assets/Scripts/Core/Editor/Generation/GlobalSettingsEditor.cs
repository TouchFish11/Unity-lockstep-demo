using System;
using System.Collections.Generic;
using System.Reflection;
using Core.Net.Protocols;
using UnityEditor;
using UnityEngine;

namespace Core.Global.Configs.Editor
{
    /// <summary>
    /// GlobalSettings 的自定义 Inspector。
    /// 重点：NetModuleConfig → NetConfig → IMessageResolver（[SerializeReference]）通过下拉列表选择具体实现。
    /// </summary>
    [CustomEditor(typeof(GlobalSettings))]
    public class GlobalSettingsEditor : UnityEditor.Editor
    {
        // ---------------- 顶层模块 ----------------
        private SerializedProperty logModuleConfig;
        private SerializedProperty eventModuleConfig;
        private SerializedProperty poolModuleConfig;
        private SerializedProperty resourcesModuleConfig;
        private SerializedProperty uploadModuleConfig;
        private SerializedProperty updateModuleConfig;
        private SerializedProperty netModuleConfig;
        private SerializedProperty userModuleConfig;

        // ---------------- 网络模块内部 ----------------
        private SerializedProperty netConfig;
        private SerializedProperty serverIp;
        private SerializedProperty serverPort;
        private SerializedProperty resolver;
        private SerializedProperty clientType;
        private SerializedProperty kcpConfig;
        private SerializedProperty udpReceiveBufferSize;
        private SerializedProperty tcpReceiveTempBufferSize;
        private SerializedProperty tcpReceiveBufferSize;
        private SerializedProperty heartMsgSendIntervalTime;
        private SerializedProperty heartTimeoutThreshold;

        // ---------------- 折叠状态 ----------------
        private bool netFoldout = true;
        private bool logFoldout = true;
        private bool eventFoldout = true;
        private bool poolFoldout = true;
        private bool resourcesFoldout = true;
        private bool uploadFoldout = true;
        private bool updateFoldout = true;
        private bool userFoldout = true;

        // ---------------- IMessageResolver 实现类型缓存 ----------------
        private static readonly Type[] ResolverTypes = CollectResolverTypes();

        private void OnEnable()
        {
            logModuleConfig = serializedObject.FindProperty("logModuleConfig");
            eventModuleConfig = serializedObject.FindProperty("eventModuleConfig");
            poolModuleConfig = serializedObject.FindProperty("poolModuleConfig");
            resourcesModuleConfig = serializedObject.FindProperty("resourcesModuleConfig");
            uploadModuleConfig = serializedObject.FindProperty("uploadModuleConfig");
            updateModuleConfig = serializedObject.FindProperty("updateModuleConfig");
            netModuleConfig = serializedObject.FindProperty("netModuleConfig");
            userModuleConfig = serializedObject.FindProperty("userModuleConfig");

            netConfig = netModuleConfig?.FindPropertyRelative("netConfig");

            serverIp = netConfig?.FindPropertyRelative("serverIp");
            serverPort = netConfig?.FindPropertyRelative("serverPort");
            resolver = netConfig?.FindPropertyRelative("resolver");
            clientType = netConfig?.FindPropertyRelative("clientType");
            kcpConfig = netConfig?.FindPropertyRelative("kcpConfig");

            udpReceiveBufferSize = netModuleConfig?.FindPropertyRelative("udpReceiveBufferSize");
            tcpReceiveTempBufferSize = netModuleConfig?.FindPropertyRelative("tcpReceiveTempBufferSize");
            tcpReceiveBufferSize = netModuleConfig?.FindPropertyRelative("tcpReceiveBufferSize");
            heartMsgSendIntervalTime = netModuleConfig?.FindPropertyRelative("heartMsgSendIntervalTime");
            heartTimeoutThreshold = netModuleConfig?.FindPropertyRelative("heartTimeoutThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            DrawNetModuleSection();
            DrawLogModuleSection();
            DrawEventModuleSection();
            DrawPoolModuleSection();
            DrawResourcesModuleSection();
            DrawUploadModuleSection();
            DrawUpdateModuleSection();
            DrawUserModuleSection();

            serializedObject.ApplyModifiedProperties();
        }

        // =====================================================================
        // 头部
        // =====================================================================
        private void DrawHeader()
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("全局设置", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("资源", target, typeof(GlobalSettings), false);
            }
            EditorGUILayout.Space(4);
        }

        // =====================================================================
        // 网络模块（重点）
        // =====================================================================
        private void DrawNetModuleSection()
        {
            DrawSection("网络模块配置（NetModuleConfig）", ref netFoldout, () =>
            {
                if (netConfig == null)
                {
                    EditorGUILayout.HelpBox("未找到 NetModuleConfig.netConfig 字段，请确认其为可序列化字段。", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField("网络连接（NetConfig）", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;

                    DrawTextField(serverIp, "服务器 IP", "服务器 IP 地址");
                    DrawUShortField(serverPort, "服务器端口", "服务器端口号");
                    DrawResolverField();
                    DrawProperty(clientType, "协议类型", "网络协议类型（EClientType）");

                    EditorGUI.indentLevel--;
                }

                // 协议专属配置（互斥）：选中的协议可编辑，另一个仅显示不可编辑
                bool isKcp = IsKcpSelected();
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("协议专属配置（互斥）", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                using (new EditorGUI.DisabledScope(!isKcp))
                {
                    if (kcpConfig != null && !isKcp)
                        kcpConfig.isExpanded = true;
                    DrawProperty(kcpConfig, "专属 KCP 配置", "仅协议类型为 Kcp 时可编辑；为空则使用默认 KCP 配置");
                }

                using (new EditorGUI.DisabledScope(isKcp))
                {
                    DrawShortField(udpReceiveBufferSize, "UDP 接收缓冲区大小", "缓存帧同步数据包");
                    DrawShortField(tcpReceiveTempBufferSize, "TCP 接收临时缓冲区大小", "临时缓存接收的 TCP 消息");
                    DrawShortField(tcpReceiveBufferSize, "TCP 接收缓冲区大小", "缓存接收的 TCP 待处理消息");
                }

                EditorGUI.indentLevel--;

                // 心跳（通用，不分协议）
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("心跳（通用）", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                DrawShortField(heartMsgSendIntervalTime, "心跳消息发送间隔时间（ms）", "心跳消息发送间隔（ms）");
                DrawIntField(heartTimeoutThreshold, "心跳超时阈值（ms）", "当前时间与上次心跳时间的差值超过该阈值，视为连接超时");
                EditorGUI.indentLevel--;
            });
        }

        // =====================================================================
        // 其余模块
        // =====================================================================
        private void DrawLogModuleSection()
        {
            DrawSection("日志模块配置（LogModuleConfig）", ref logFoldout, () =>
            {
                DrawFields(logModuleConfig, "tag", "writeLogMaxIntervalTime", "filterLevel");
            });
        }

        private void DrawEventModuleSection()
        {
            DrawSection("事件模块配置（EventModuleConfig）", ref eventFoldout, () =>
            {
                DrawFields(eventModuleConfig, "eventTriggerMaxNumPerFrame");
            });
        }

        private void DrawPoolModuleSection()
        {
            DrawSection("对象池模块配置（PoolModuleConfig）", ref poolFoldout, () =>
            {
                DrawFields(poolModuleConfig, "isOpenLayout", "activeTimeThreshold", "poolMinSize", "poolMaxSize");
            });
        }

        private void DrawResourcesModuleSection()
        {
            DrawSection("资源加载模块配置（ResourcesModuleConfig）", ref resourcesFoldout, () =>
            {
                DrawFields(resourcesModuleConfig, "abLoadPath", "criticalActiveThreshold", "bundleSlidingWindowMaxCount", "maxDurationPerWindow");
            });
        }

        private void DrawUploadModuleSection()
        {
            DrawSection("上传模块配置（UploadModuleConfig）", ref uploadFoldout, () =>
            {
                DrawFields(uploadModuleConfig, "uploadServerIp");
            });
        }

        private void DrawUpdateModuleSection()
        {
            DrawSection("更新模块配置（UpdateModuleConfig）", ref updateFoldout, () =>
            {
                DrawFields(updateModuleConfig,
                    "resServerIp",
                    "reDownloadCompareFileMaxNum",
                    "reDownloadAbMaxNum",
                    "maxConcurrencyNum",
                    "connectTimeout",
                    "maxRetryCount",
                    "maxRetryWaitSeconds",
                    "speedUpdateInterval");
            });
        }

        private void DrawUserModuleSection()
        {
            DrawSection("用户模块配置（UserModuleConfig）", ref userFoldout, () =>
            {
                DrawFields(userModuleConfig, "userDataPath");
            });
        }

        // =====================================================================
        // 消息序列化器（下拉列表）
        // =====================================================================
        private void DrawResolverField()
        {
            if (resolver == null)
            {
                EditorGUILayout.HelpBox("未找到 NetConfig.resolver 字段，请确认其使用 [SerializeReference] 修饰。", MessageType.Warning);
                return;
            }

            if (resolver.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUILayout.HelpBox("resolver 字段不是 [SerializeReference]（ManagedReference）类型，请检查属性修饰。", MessageType.Error);
                return;
            }

            if (ResolverTypes.Length == 0)
            {
                EditorGUILayout.HelpBox("未在项目中找到 IMessageResolver 的具体实现类型。", MessageType.Warning);
                return;
            }

            // 构造选项：第一个为 "(None)"，其余为实现类型全名
            string[] options = new string[ResolverTypes.Length + 1];
            options[0] = "(None)";
            for (int i = 0; i < ResolverTypes.Length; i++)
            {
                options[i + 1] = ResolverTypes[i].Name;
            }

            object current = resolver.managedReferenceValue;
            Type currentType = current?.GetType();
            int currentIndex = 0;

            if (currentType != null)
            {
                for (int i = 0; i < ResolverTypes.Length; i++)
                {
                    if (ResolverTypes[i] == currentType)
                    {
                        currentIndex = i + 1;
                        break;
                    }
                }

                if (currentIndex == 0)
                {
                    EditorGUILayout.HelpBox($"当前实现 {currentType.FullName} 不在可选项列表中。", MessageType.Warning);
                }
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(
                new GUIContent("消息序列化器", "实现 IMessageResolver 的消息序列化器，通过下拉列表选择具体实现"),
                currentIndex,
                options);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "设置消息序列化器");

                if (newIndex == 0)
                {
                    resolver.managedReferenceValue = null;
                }
                else
                {
                    Type selectedType = ResolverTypes[newIndex - 1];
                    try
                    {
                        resolver.managedReferenceValue = Activator.CreateInstance(selectedType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"无法创建 {selectedType.FullName} 的实例：{ex.Message}");
                    }
                }
            }

            // 变更后重新读取，展示当前实现
            object displayValue = resolver.managedReferenceValue;
            Type displayType = displayValue?.GetType();
            if (displayType != null)
            {
                EditorGUILayout.HelpBox($"当前实现：{displayType.FullName}", MessageType.None);
            }

            DrawResolverInstanceFields();
        }

        private void DrawResolverInstanceFields()
        {
            if (resolver == null || resolver.managedReferenceValue == null)
                return;

            EditorGUILayout.LabelField("序列化器参数", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;

            SerializedProperty child = resolver.Copy();
            int depth = resolver.depth;
            bool enterChildren = true;
            bool drewAny = false;

            while (child.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (child.depth <= depth)
                    break;

                EditorGUILayout.PropertyField(child, true);
                drewAny = true;
            }

            if (!drewAny)
            {
                EditorGUILayout.HelpBox("该实现没有可配置的序列化字段。", MessageType.None);
            }

            EditorGUI.indentLevel--;
        }

        // =====================================================================
        // 通用绘制辅助
        // =====================================================================
        private void DrawSection(string title, ref bool foldout, Action body)
        {
            EditorGUILayout.Space(6);
            foldout = EditorGUILayout.Foldout(foldout, title, true, EditorStyles.foldoutHeader);
            if (!foldout)
                return;

            EditorGUI.indentLevel++;
            body();
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(2);
        }

        private void DrawFields(SerializedProperty module, params string[] fieldNames)
        {
            if (module == null)
            {
                EditorGUILayout.HelpBox("模块配置字段未找到。", MessageType.Warning);
                return;
            }

            foreach (string fieldName in fieldNames)
            {
                SerializedProperty prop = module.FindPropertyRelative(fieldName);
                if (prop == null)
                {
                    EditorGUILayout.HelpBox($"字段未找到：{fieldName}", MessageType.Warning);
                    continue;
                }
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        private void DrawProperty(SerializedProperty prop, string label, string tooltip)
        {
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"字段未找到：{label}", MessageType.Warning);
                return;
            }
            EditorGUILayout.PropertyField(prop, new GUIContent(label, tooltip), true);
        }

        private void DrawTextField(SerializedProperty prop, string label, string tooltip)
        {
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"字段未找到：{label}", MessageType.Warning);
                return;
            }
            EditorGUI.BeginChangeCheck();
            string value = EditorGUILayout.TextField(new GUIContent(label, tooltip), prop.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.stringValue = value;
            }
        }

        private void DrawShortField(SerializedProperty prop, string label, string tooltip)
        {
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"字段未找到：{label}", MessageType.Warning);
                return;
            }
            EditorGUI.BeginChangeCheck();
            int value = EditorGUILayout.IntField(new GUIContent(label, tooltip), prop.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.intValue = Mathf.Clamp(value, short.MinValue, short.MaxValue);
            }
        }

        private void DrawUShortField(SerializedProperty prop, string label, string tooltip)
        {
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"字段未找到：{label}", MessageType.Warning);
                return;
            }
            EditorGUI.BeginChangeCheck();
            int value = EditorGUILayout.IntField(new GUIContent(label, tooltip), prop.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.intValue = Mathf.Clamp(value, 0, ushort.MaxValue);
            }
        }

        private void DrawIntField(SerializedProperty prop, string label, string tooltip)
        {
            if (prop == null)
            {
                EditorGUILayout.HelpBox($"字段未找到：{label}", MessageType.Warning);
                return;
            }
            EditorGUI.BeginChangeCheck();
            int value = EditorGUILayout.IntField(new GUIContent(label, tooltip), prop.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.intValue = value;
            }
        }

        /// <summary>
        /// 判断当前协议类型是否为 Kcp。
        /// 用枚举成员名匹配（而非枚举数值），避免枚举底层值非连续时出错。
        /// </summary>
        private bool IsKcpSelected()
        {
            if (clientType == null)
                return false;
            if (clientType.enumValueIndex < 0 || clientType.enumValueIndex >= clientType.enumNames.Length)
                return false;
            return string.Equals(clientType.enumNames[clientType.enumValueIndex], "Kcp", StringComparison.OrdinalIgnoreCase);
        }

        // =====================================================================
        // IMessageResolver 实现类型收集
        // =====================================================================
        private static Type[] CollectResolverTypes()
        {
            List<Type> result = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch
                {
                    continue;
                }

                if (types == null)
                    continue;

                foreach (Type type in types)
                {
                    if (type == null)
                        continue;
                    if (!type.IsClass || type.IsAbstract)
                        continue;
                    if (type.ContainsGenericParameters)
                        continue;
                    if (typeof(IMessageResolver).IsAssignableFrom(type))
                        result.Add(type);
                }
            }

            result.Sort((a, b) => string.CompareOrdinal(a.FullName ?? a.Name, b.FullName ?? b.Name));
            return result.ToArray();
        }
    }
}
