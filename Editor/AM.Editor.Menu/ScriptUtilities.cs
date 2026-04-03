using System.Text;

namespace AM.Editor.Menu
{
    public class ScriptUtilities
    {
        public static void GetGenerateSetting(string @string,
                                              out string extactedNameSpace,
                                              out string extactedClassName,
                                              out string extactedInheritName,
                                              out string[] extractedClassGenerics,
                                              out string[] extractedInheritGenerics)
        {
            string clear = @string.Trim();

            static (string body, string[] generics) ExtractGenerics(string input)
            {
                int start = input.IndexOf('<');
                int end = input.LastIndexOf('>');

                if (start == -1 || end == -1 || end <= start)
                    return (input.Trim(), null);

                string body = input.Substring(0, start).Trim();
                string content = input.Substring(start + 1, end - start - 1);
                string[] types = content.Split(',');

                for (int i = 0; i < types.Length; i++)
                    types[i] = types[i].Trim();

                return (body, types);
            }

            string classPart = clear;
            string basePart = null;

            int colonIndex = clear.IndexOf(':');
            if (colonIndex != -1)
            {
                classPart = clear.Substring(0, colonIndex).Trim();
                basePart = clear.Substring(colonIndex + 1).Trim();
            }

            var (classBody, classGenerics) = ExtractGenerics(classPart);

            int lastDot = classBody.LastIndexOf('.');
            if (lastDot != -1)
            {
                extactedNameSpace = classBody.Substring(0, lastDot);
                extactedClassName = classBody.Substring(lastDot + 1);
            }
            else
            {
                extactedNameSpace = null;
                extactedClassName = classBody;
            }

            extractedClassGenerics = classGenerics;

            if (basePart != null)
            {
                var (baseBody, baseGenerics) = ExtractGenerics(basePart);
                extactedInheritName = baseBody;
                extractedInheritGenerics = baseGenerics;
            }
            else
            {
                extactedInheritName = null;
                extractedInheritGenerics = null;
            }
        }

        private static string BuildClassDeclaration(
            string keyword,
            string name,
            string[] classGenerics,
            string inheritName,
            string[] inheritGenerics)
        {
            var sb = new StringBuilder();
            sb.Append($"public {keyword} {name}");

            if (classGenerics != null && classGenerics.Length > 0)
                sb.Append($"<{string.Join(", ", classGenerics)}>");

            if (!string.IsNullOrEmpty(inheritName))
            {
                sb.Append($" : {inheritName}");

                if (inheritGenerics != null && inheritGenerics.Length > 0)
                    sb.Append($"<{string.Join(", ", inheritGenerics)}>");
            }

            return sb.ToString();
        }

        private static string WrapNamespace(string nameSpace, string body)
        {
            if (string.IsNullOrEmpty(nameSpace))
                return body;

            var indented = new StringBuilder();
            foreach (var line in body.Split('\n'))
                indented.Append(string.IsNullOrWhiteSpace(line) ? "\n" : $"    {line}\n");

            return $"namespace {nameSpace}\n{{\n{indented}}}";
        }

        public static string GetMonoBehaviourTemplate(string @string)
        {
            GetGenerateSetting(@string,
                out string nameSpace,
                out string className,
                out string inheritName,
                out string[] classGenerics,
                out string[] inheritGenerics);

            return GetMonoBehaviourTemplate(className, nameSpace, classGenerics, inheritName, inheritGenerics);
        }

        public static string GetMonoBehaviourTemplate(
            string name,
            string nameSpace = null,
            string[] classGenerics = null,
            string inheritName = null,
            string[] inheritGenerics = null)
        {
            string baseClass = string.IsNullOrEmpty(inheritName) ? "MonoBehaviour" : inheritName;
            string[] baseGenerics = string.IsNullOrEmpty(inheritName) ? null : inheritGenerics;

            string declaration = BuildClassDeclaration("class", name, classGenerics, baseClass, baseGenerics);

            string body = $@"using UnityEngine;

{declaration}
{{
    private void Awake()
    {{
    }}

    private void Update()
    {{
    }}
}}";
            return WrapNamespace(nameSpace, body);
        }

        public static string GetScriptableObjectTemplate(string @string)
        {
            GetGenerateSetting(@string,
                out string nameSpace,
                out string className,
                out string inheritName,
                out string[] classGenerics,
                out string[] inheritGenerics);

            return GetScriptableObjectTemplate(className, nameSpace, classGenerics, inheritName, inheritGenerics);
        }

        public static string GetScriptableObjectTemplate(
            string name,
            string nameSpace = null,
            string[] classGenerics = null,
            string inheritName = null,
            string[] inheritGenerics = null)
        {
            string baseClass = string.IsNullOrEmpty(inheritName) ? "ScriptableObject" : inheritName;
            string[] baseGenerics = string.IsNullOrEmpty(inheritName) ? null : inheritGenerics;

            string declaration = BuildClassDeclaration("class", name, classGenerics, baseClass, baseGenerics);

            string body = $@"using UnityEngine;

[CreateAssetMenu(fileName = ""{name}"", menuName = ""Game/{name}"")]
{declaration}
{{
}}";
            return WrapNamespace(nameSpace, body);
        }

        public static string GetEditorScriptTemplate(string @string)
        {
            GetGenerateSetting(@string,
                out string nameSpace,
                out string className,
                out string inheritName,
                out string[] classGenerics,
                out string[] inheritGenerics);

            return GetEditorScriptTemplate(className, nameSpace, classGenerics, inheritName, inheritGenerics);
        }

        public static string GetEditorScriptTemplate(
            string name,
            string nameSpace = null,
            string[] classGenerics = null,
            string inheritName = null,
            string[] inheritGenerics = null)
        {
            string baseClass = string.IsNullOrEmpty(inheritName) ? "Editor" : inheritName;
            string[] baseGenerics = string.IsNullOrEmpty(inheritName) ? null : inheritGenerics;

            string declaration = BuildClassDeclaration("class", name, classGenerics, baseClass, baseGenerics);

            string body = $@"#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour))]
{declaration}
{{
    public override void OnInspectorGUI()
    {{
        base.OnInspectorGUI();
    }}
}}
#endif";
            return WrapNamespace(nameSpace, body);
        }

        public static string GetInterfaceTemplate(string @string)
        {
            GetGenerateSetting(@string,
                out string nameSpace,
                out string className,
                out string inheritName,
                out string[] classGenerics,
                out string[] inheritGenerics);

            return GetInterfaceTemplate(className, nameSpace, classGenerics, inheritName, inheritGenerics);
        }

        public static string GetInterfaceTemplate(
            string name,
            string nameSpace = null,
            string[] classGenerics = null,
            string inheritName = null,
            string[] inheritGenerics = null)
        {
            string declaration = BuildClassDeclaration("interface", name, classGenerics, inheritName, inheritGenerics);

            string body = $@"{declaration}
{{
}}";
            return WrapNamespace(nameSpace, body);
        }

        public static string GetAbstractClassTemplate(string @string)
        {
            GetGenerateSetting(@string,
                out string nameSpace,
                out string className,
                out string inheritName,
                out string[] classGenerics,
                out string[] inheritGenerics);

            return GetAbstractClassTemplate(className, nameSpace, classGenerics, inheritName, inheritGenerics);
        }

        public static string GetAbstractClassTemplate(
            string name,
            string nameSpace = null,
            string[] classGenerics = null,
            string inheritName = null,
            string[] inheritGenerics = null)
        {
            string declaration = BuildClassDeclaration("abstract class", name, classGenerics, inheritName, inheritGenerics);

            string body = $@"{declaration}
{{
}}";
            return WrapNamespace(nameSpace, body);
        }

        public static string GetGenericTemplate(string @string)
        {
            GetGenerateSetting(@string,
                out string nameSpace,
                out string className,
                out string inheritName,
                out string[] classGenerics,
                out string[] inheritGenerics);

            return GetGenericTemplate(className, nameSpace, classGenerics, inheritName, inheritGenerics);
        }

        public static string GetGenericTemplate(
            string name,
            string nameSpace = null,
            string[] classGenerics = null,
            string inheritName = null,
            string[] inheritGenerics = null)
        {
            string declaration = BuildClassDeclaration("class", name, classGenerics, inheritName, inheritGenerics);

            string body = $@"{declaration}
{{
}}";
            return WrapNamespace(nameSpace, body);
        }

        public static string GetStructTemplate(string @string)
        {
            GetGenerateSetting(@string,
                out string nameSpace,
                out string className,
                out string inheritName,
                out string[] classGenerics,
                out string[] inheritGenerics);

            return GetStructTemplate(className, nameSpace, classGenerics, inheritName, inheritGenerics);
        }

        public static string GetStructTemplate(
            string name,
            string nameSpace = null,
            string[] classGenerics = null,
            string inheritName = null,
            string[] inheritGenerics = null)
        {
            string declaration = BuildClassDeclaration("struct", name, classGenerics, inheritName, inheritGenerics);

            string body = $@"{declaration}
{{
}}";
            return WrapNamespace(nameSpace, body);
        }
    }
}