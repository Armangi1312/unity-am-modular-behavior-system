using AM.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AM.Editor
{
    internal class ProcessorCreatorWindow : EditorWindow
    {
        private string className = "NewProcessor";

        private string namespaceName = "";
        private List<Type> allProcessorBaseTypes = new();
        private string[] processorBaseTypeNames = Array.Empty<string>();
        private int selectedProcessorIndex = -1;
        private int prevProcessorIndex = -2;

        private Type constraintSettingInterface = null;
        private Type constraintContextInterface = null;

        private List<Type> filteredSettingTypes = new();
        private List<Type> filteredContextTypes = new();
        private bool[] settingSelected = Array.Empty<bool>();
        private bool[] contextSelected = Array.Empty<bool>();

        private Vector2 scrollPos;

        [MenuItem("AM/Processor Creator")]
        public static void Open()
        {
            var window = GetWindow<ProcessorCreatorWindow>("Processor Creator");
            window.minSize = new Vector2(500f, 640f);
            window.Show();
            window.ScanTypes();
        }


        private void ScanTypes()
        {
            allProcessorBaseTypes.Clear();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null) continue;
                    if (t.IsInterface || t.ContainsGenericParameters) continue;
                    if (!t.IsClass) continue;
                    if (!ImplementsProcessorInterface(t)) continue;

                    allProcessorBaseTypes.Add(t);
                }
            }

            allProcessorBaseTypes.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.Ordinal));


            processorBaseTypeNames = allProcessorBaseTypes
                .Select(t => t.IsAbstract ? $"[Abstract]  {t.Name}" : t.Name)
                .ToArray();

            selectedProcessorIndex = allProcessorBaseTypes.Count > 0 ? 0 : -1;
            prevProcessorIndex = -2;

            RefreshSettingContextCandidates();
        }

        private void RefreshSettingContextCandidates()
        {
            constraintSettingInterface = null;
            constraintContextInterface = null;
            filteredSettingTypes.Clear();
            filteredContextTypes.Clear();

            if (selectedProcessorIndex < 0 ||
                selectedProcessorIndex >= allProcessorBaseTypes.Count)
            {
                settingSelected = Array.Empty<bool>();
                contextSelected = Array.Empty<bool>();
                return;
            }

            var baseType = allProcessorBaseTypes[selectedProcessorIndex];

            ExtractProcessorConstraints(
                baseType,
                out constraintSettingInterface,
                out constraintContextInterface);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null || t.IsAbstract || t.IsInterface ||
                        t.ContainsGenericParameters || !t.IsClass)
                        continue;

                    if (constraintSettingInterface != null &&
                        constraintSettingInterface.IsAssignableFrom(t))
                        filteredSettingTypes.Add(t);

                    if (constraintContextInterface != null &&
                        constraintContextInterface.IsAssignableFrom(t))
                        filteredContextTypes.Add(t);
                }
            }

            filteredSettingTypes.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            filteredContextTypes.Sort((a, b) =>
                string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            settingSelected = new bool[filteredSettingTypes.Count];
            contextSelected = new bool[filteredContextTypes.Count];
        }

        private bool ImplementsProcessorInterface(Type t)
        {
            var type = t;
            while (type != null && type != typeof(object))
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;
                    if (iface.GetGenericTypeDefinition() == typeof(IProcessor<,>))
                        return true;
                }

                var baseType = type.BaseType;
                if (baseType != null && baseType.IsGenericType && !baseType.ContainsGenericParameters)
                {
                    foreach (var iface in baseType.GetGenericTypeDefinition().GetInterfaces())
                    {
                        if (!iface.IsGenericType) continue;
                        if (iface.GetGenericTypeDefinition() == typeof(IProcessor<,>))
                            return true;
                    }
                }

                type = type.BaseType;
            }

            return false;
        }

        private void ExtractProcessorConstraints(
            Type processorType,
            out Type settingInterface,
            out Type contextInterface)
        {
            settingInterface = null;
            contextInterface = null;

            var type = processorType;
            while (type != null && type != typeof(object))
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;
                    if (iface.GetGenericTypeDefinition() != typeof(IProcessor<,>)) continue;

                    var args = iface.GetGenericArguments();
                    settingInterface = args[0];
                    contextInterface = args[1];
                    return;
                }

                var baseType = type.BaseType;
                if (baseType != null && baseType.IsGenericType && !baseType.ContainsGenericParameters)
                {
                    var baseArgs = baseType.GetGenericArguments();
                    if (baseArgs.Length >= 2)
                    {
                        foreach (var iface in baseType.GetGenericTypeDefinition().GetInterfaces())
                        {
                            if (!iface.IsGenericType) continue;
                            if (iface.GetGenericTypeDefinition() != typeof(IProcessor<,>)) continue;

                            settingInterface = baseArgs[0];
                            contextInterface = baseArgs[1];
                            return;
                        }
                    }
                }

                type = type.BaseType;
            }
        }

        private List<Type> GetSelectedSettings()
        {
            var result = new List<Type>();
            for (int i = 0; i < settingSelected.Length; i++)
                if (settingSelected[i]) result.Add(filteredSettingTypes[i]);
            return result;
        }

        private List<Type> GetSelectedContexts()
        {
            var result = new List<Type>();
            for (int i = 0; i < contextSelected.Length; i++)
                if (contextSelected[i]) result.Add(filteredContextTypes[i]);
            return result;
        }

        private void OnGUI()
        {
            if (selectedProcessorIndex != prevProcessorIndex)
            {
                prevProcessorIndex = selectedProcessorIndex;
                RefreshSettingContextCandidates();
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawHeader();
            GUILayout.Space(8f);
            DrawClassNameSection();
            GUILayout.Space(8f);
            DrawProcessorBaseSection();
            GUILayout.Space(6f);
            DrawConstraintInfo();
            GUILayout.Space(6f);
            DrawToggleSection("Settings  (RequireSetting)", filteredSettingTypes, settingSelected);
            GUILayout.Space(8f);
            DrawToggleSection("Contexts  (RequireContext)", filteredContextTypes, contextSelected);
            GUILayout.Space(12f);
            DrawPreview();
            GUILayout.Space(12f);
            DrawGenerateButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Processor Creator", EditorStyles.boldLabel);
            if (GUILayout.Button("Research Types", GUILayout.Height(24f)))
                ScanTypes();
        }

        private void DrawClassNameSection()
        {
            EditorGUILayout.LabelField("Class Info", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Namespace", GUILayout.Width(100f));
                namespaceName = EditorGUILayout.TextField(namespaceName);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Class Name", GUILayout.Width(100f));
                className = EditorGUILayout.TextField(className);
            }
        }

        private void DrawProcessorBaseSection()
        {
            EditorGUILayout.LabelField("Select Base Processor", EditorStyles.boldLabel);

            if (allProcessorBaseTypes.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "ProcessorCache에서 IProcessor<,> 구현 타입을 찾지 못했습니다.",
                    MessageType.Warning);
                return;
            }

            selectedProcessorIndex = EditorGUILayout.Popup(
                "Base Processor",
                selectedProcessorIndex,
                processorBaseTypeNames);
        }

        private void DrawConstraintInfo()
        {
            if (constraintSettingInterface == null && constraintContextInterface == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Interfaces", EditorStyles.miniLabel);

                if (constraintSettingInterface != null)
                    EditorGUILayout.LabelField(
                        $"  Setting  :  {constraintSettingInterface.Name}",
                        EditorStyles.miniLabel);

                if (constraintContextInterface != null)
                    EditorGUILayout.LabelField(
                        $"  Context  :  {constraintContextInterface.Name}",
                        EditorStyles.miniLabel);
            }
        }

        private void DrawToggleSection(string header, List<Type> types, bool[] selected)
        {
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

            if (types.Count == 0)
            {
                EditorGUILayout.HelpBox("Couldn't find type.", MessageType.Info);
                return;
            }

            for (int i = 0; i < types.Count; i++)
            {
                selected[i] = EditorGUILayout.ToggleLeft(
                    $"{types[i].Name}  ({types[i].Namespace})",
                    selected[i]);
            }
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            var style = new GUIStyle(EditorStyles.textArea)
            {
                fontSize = 11,
                wordWrap = false,
                richText = false
            };

            EditorGUILayout.TextArea(
                GenerateCode(), style,
                GUILayout.ExpandHeight(true),
                GUILayout.MinHeight(200f));
        }

        private void DrawGenerateButton()
        {
            bool valid = !string.IsNullOrWhiteSpace(className) &&
                         selectedProcessorIndex >= 0;

            GUI.enabled = valid;
            if (GUILayout.Button("Add file", GUILayout.Height(36f)))
                GenerateFile();
            GUI.enabled = true;

            if (!valid)
                EditorGUILayout.HelpBox(
                    "Please select name and base Processor",
                    MessageType.Warning);
        }

        private string GenerateCode()
        {
            if (selectedProcessorIndex < 0 ||
                selectedProcessorIndex >= allProcessorBaseTypes.Count)
                return "// Please base Processor";

            var baseType = allProcessorBaseTypes[selectedProcessorIndex];
            var selectedSettings = GetSelectedSettings();
            var selectedContexts = GetSelectedContexts();

            var sb = new StringBuilder();
            bool hasNs = !string.IsNullOrWhiteSpace(namespaceName);
            string ind = hasNs ? "    " : "";
            string mem = ind + "    ";

            sb.AppendLine("using AM.Core;");
            sb.AppendLine("using AM.Core.Utilities;");
            sb.AppendLine("using System;");
            sb.AppendLine();

            if (hasNs)
            {
                sb.AppendLine($"namespace {namespaceName.Trim()}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"{ind}[Serializable]");

            if (selectedSettings.Count > 0)
            {
                var args = string.Join(", ", selectedSettings.Select(t => $"typeof({t.Name})"));
                sb.AppendLine($"{ind}[RequireSetting({args})]");
            }

            if (selectedContexts.Count > 0)
            {
                var args = string.Join(", ", selectedContexts.Select(t => $"typeof({t.Name})"));
                sb.AppendLine($"{ind}[RequireContext({args})]");
            }

            sb.AppendLine($"{ind}public class {className.Trim()} : {baseType.Name}");
            sb.AppendLine($"{ind}{{");

            foreach (var t in selectedSettings)
                sb.AppendLine($"{mem}private {t.Name} {ToCamelCase(t.Name)};");

            foreach (var t in selectedContexts)
                sb.AppendLine($"{mem}private {t.Name} {ToCamelCase(t.Name)};");

            if (selectedSettings.Count > 0 || selectedContexts.Count > 0)
                sb.AppendLine();

            var initMethod = ResolveInitializeMethod(baseType);
            if (initMethod != null)
            {
                string paramStr = BuildInitializeParamString(initMethod);
                string settingReg = FindRegistryParamName(initMethod, isContext: false);
                string contextReg = FindRegistryParamName(initMethod, isContext: true);

                sb.AppendLine($"{mem}public override void Initialize({paramStr})");
                sb.AppendLine($"{mem}{{");

                foreach (var t in selectedSettings)
                    sb.AppendLine($"{mem}    {ToCamelCase(t.Name)} = {settingReg}.Get<{t.Name}>();");

                foreach (var t in selectedContexts)
                    sb.AppendLine($"{mem}    {ToCamelCase(t.Name)} = {contextReg}.Get<{t.Name}>();");

                sb.AppendLine($"{mem}}}");
                sb.AppendLine();
            }

            sb.AppendLine($"{mem}public override void Process()");
            sb.AppendLine($"{mem}{{");
            sb.AppendLine($"{mem}}}");

            sb.AppendLine($"{ind}}}");

            if (hasNs)
                sb.AppendLine("}");

            return sb.ToString();
        }

        private MethodInfo ResolveInitializeMethod(Type baseType)
        {
            var type = baseType;
            while (type != null && type != typeof(object))
            {
                var method = type.GetMethod(
                    "Initialize",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                if (method != null) return method;
                type = type.BaseType;
            }

            return null;
        }

        private string BuildInitializeParamString(MethodInfo method)
        {
            return string.Join(", ", method.GetParameters()
                .Select(p => $"{GetFriendlyTypeName(p.ParameterType)} {p.Name}"));
        }

        private string FindRegistryParamName(MethodInfo method, bool isContext)
        {
            foreach (var p in method.GetParameters())
            {
                var lower = p.ParameterType.Name.ToLower();
                if (isContext && lower.Contains("context")) return p.Name;
                if (!isContext && lower.Contains("setting")) return p.Name;
            }

            var parameters = method.GetParameters();
            if (!isContext && parameters.Length > 0) return parameters[0].Name;
            if (isContext && parameters.Length > 1) return parameters[1].Name;

            return isContext ? "contextRegistry" : "settingRegistry";
        }

        private string GetFriendlyTypeName(Type t)
        {
            if (!t.IsGenericType) return t.Name;

            var name = t.GetGenericTypeDefinition().Name;
            int backtick = name.IndexOf('`');
            if (backtick > 0) name = name.Substring(0, backtick);

            var args = t.GetGenericArguments().Select(GetFriendlyTypeName);
            return $"{name}<{string.Join(", ", args)}>";
        }
        private string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLower(name[0]) + name.Substring(1);
        }

        private void GenerateFile()
        {
            var code = GenerateCode();
            var defaultName = $"{className.Trim()}.cs";

            var path = EditorUtility.SaveFilePanel(
                "Save Processor Script",
                Application.dataPath,
                defaultName,
                "cs");

            if (string.IsNullOrEmpty(path)) return;

            File.WriteAllText(path, code, Encoding.UTF8);
            AssetDatabase.Refresh();

            var relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath);
            if (asset != null)
            {
                ProjectWindowUtil.ShowCreatedAsset(asset);
                Selection.activeObject = asset;
            }
        }
    }
}