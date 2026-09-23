using AM.Core;
using UnityEditor;
using UnityEngine;

namespace AM.Editor
{
    [CustomEditor(typeof(Controller), true)]
    internal class ControllerEditor : UnityEditor.Editor
    {
        private Controller controller;
        private ProcessorReorderableList processorList;
        private SerializedProperty settingsRootProperty;
        private SerializedProperty contextsRootProperty;

        private void OnEnable()
        {
            controller = target as Controller;
            if (controller == null) return;

            if (!ControllerTypeResolver.TryResolve(controller,
                out var settingType, out var contextType, out var processorType))
                return;

            var checker = new ProcessorCompatibilityChecker(settingType, contextType, processorType);
            var processorsProperty = serializedObject.FindProperty("processors");

            if (processorsProperty != null)
                processorList = new ProcessorReorderableList(serializedObject, processorsProperty, checker);

            settingsRootProperty = serializedObject.FindProperty("settings");
            contextsRootProperty = serializedObject.FindProperty("contexts");
        }

        public override void OnInspectorGUI()
        {
            if (controller == null) return;

            serializedObject.Update();

            DrawScriptField();

            GUILayout.Space(8);
            processorList?.DoLayoutList();

            GUILayout.Space(6);
            if (settingsRootProperty != null)
                EditorGUILayout.PropertyField(settingsRootProperty, true);

            GUILayout.Space(6);
            if (contextsRootProperty != null)
                EditorGUILayout.PropertyField(contextsRootProperty, true);

            GUILayout.Space(6);
            if (GUILayout.Button("Edit Pipeline"))
            {
                // Handle pipeline editing logic here
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScriptField()
        {
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script",
                MonoScript.FromMonoBehaviour((MonoBehaviour)target),
                typeof(MonoScript), false);
            GUI.enabled = true;
        }
    }
}