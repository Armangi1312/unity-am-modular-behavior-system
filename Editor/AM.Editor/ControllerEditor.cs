using AM.Core;
using AM.Core.Utilities;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AM.Editor
{
    [CustomEditor(typeof(Controller), true)]
    internal class ControllerEditor : UnityEditor.Editor
    {
        private ReorderableList processorList;

        private SerializedProperty processorsProperty;
        private SerializedProperty settingsRootProperty;
        private SerializedProperty contextsRootProperty;

        private Controller controller;
        private Type controllerSettingType;
        private Type controllerContextType;
        private Type controllerProcessorType;

        private void OnEnable()
        {
            controller = target as Controller;
            if (controller == null) return;

            ResolveControllerTypes();

            processorsProperty = serializedObject.FindProperty("processors");
            settingsRootProperty = serializedObject.FindProperty("settings");
            contextsRootProperty = serializedObject.FindProperty("contexts");

            if (processorsProperty != null)
                processorList = CreateProcessorList();
        }

        private void ResolveControllerTypes()
        {
            try
            {
                controllerSettingType = controller.SettingType();
                controllerContextType = controller.ContextType();
                controllerProcessorType = controller.ProcessorType();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ControllerEditor] Failed to resolve controller types: {e.Message}");
                controllerSettingType = null;
                controllerContextType = null;
                controllerProcessorType = null;
            }
        }

        #region ReorderableList

        private ReorderableList CreateProcessorList()
        {
            return new ReorderableList(serializedObject, processorsProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Processors"),
                drawElementCallback = DrawProcessorElement,
                elementHeightCallback = GetProcessorElementHeight,
                onAddDropdownCallback = (rect, _) => ShowProcessorMenu(),
                onRemoveCallback = OnRemoveProcessor,
                drawNoneElementCallback = rect =>
                    EditorGUI.LabelField(rect, "No processors. Add one with +", EditorStyles.centeredGreyMiniLabel)
            };
        }

        private void DrawProcessorElement(Rect rect, int index, bool active, bool focused)
        {
            var element = processorsProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            var obj = element.managedReferenceValue;
            string label = obj == null ? "Null" : obj.GetType().Name;

            EditorGUI.LabelField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                label, EditorStyles.boldLabel);

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        }

        private float GetProcessorElementHeight(int index)
        {
            var element = processorsProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 6f;
        }

        private void OnRemoveProcessor(ReorderableList list)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        #endregion

        #region Processor Menu

        private void ShowProcessorMenu()
        {
            var menu = new GenericMenu();
            bool any = false;

            foreach (var type in ProcessorCache.ProcessorTypes)
            {
                if (!IsValidProcessorCandidate(type)) continue;
                if (!IsCompatibleProcessor(type)) continue;
                if (ProcessorAlreadyAdded(type)) continue;

                any = true;
                var cached = type;
                menu.AddItem(new GUIContent(type.Name), false, () => AddProcessor(cached));
            }

            if (!any)
                menu.AddDisabledItem(new GUIContent("No compatible processors found"));

            menu.ShowAsContext();
        }

        private static bool IsValidProcessorCandidate(Type type)
        {
            return type != null && !type.IsAbstract && !type.IsInterface && !type.ContainsGenericParameters;
        }

        private bool IsCompatibleProcessor(Type candidate)
        {
            if (controllerSettingType == null || controllerContextType == null || controllerProcessorType == null)
                return false;

            if (!controllerProcessorType.IsAssignableFrom(candidate))
                return false;

            foreach (var iface in candidate.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                if (iface.GetGenericTypeDefinition() != typeof(IProcessor<,>)) continue;

                var args = iface.GetGenericArguments();
                if (controllerSettingType.IsAssignableFrom(args[0]) &&
                    controllerContextType.IsAssignableFrom(args[1]))
                    return true;
            }

            return false;
        }

        private bool ProcessorAlreadyAdded(Type type)
        {
            if (processorsProperty == null) return false;

            for (int i = 0; i < processorsProperty.arraySize; i++)
            {
                var existing = processorsProperty.GetArrayElementAtIndex(i).managedReferenceValue;
                if (existing != null && existing.GetType() == type) return true;
            }

            return false;
        }

        private void AddProcessor(Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!IsValidProcessorCandidate(type)) return;

            serializedObject.Update();
            Undo.RecordObject(controller, "Add Processor");

            int index = processorsProperty.arraySize;
            processorsProperty.InsertArrayElementAtIndex(index);
            processorsProperty.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        #endregion

        #region Inspector GUI

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

        #endregion
    }
}