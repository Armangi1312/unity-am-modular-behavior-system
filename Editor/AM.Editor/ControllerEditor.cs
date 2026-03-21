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
            if (controller == null)
                return;

            ResolveControllerTypesFromControllerAPIs();

            processorsProperty = serializedObject.FindProperty("processors");
            settingsRootProperty = serializedObject.FindProperty("settings");
            contextsRootProperty = serializedObject.FindProperty("contexts");

            if (processorsProperty != null)
                processorList = CreateProcessorList();
        }

        private void ResolveControllerTypesFromControllerAPIs()
        {
            controllerSettingType = null;
            controllerContextType = null;
            controllerProcessorType = null;

            try
            {
                controllerSettingType = controller.SettingType();
                controllerContextType = controller.ContextType();
                controllerProcessorType = controller.ProcessorType();
            }
            catch
            {
                controllerSettingType = null;
                controllerContextType = null;
                controllerProcessorType = null;
            }
        }

        private ReorderableList CreateProcessorList()
        {
            var list = new ReorderableList(serializedObject, processorsProperty, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Processors");
                },

                drawElementCallback = (rect, index, active, focused) =>
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
                },

                elementHeightCallback = index =>
                {
                    var element = processorsProperty.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(element, true) + 6f;
                },

                onAddDropdownCallback = (rect, l) =>
                {
                    ShowProcessorMenu();
                },

                onRemoveCallback = l =>
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                        return;

                    ReorderableList.defaultBehaviours.DoRemoveButton(l);
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(controller);
                },

                drawNoneElementCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "No processors. Add one with +", EditorStyles.centeredGreyMiniLabel);
                }
            };

            return list;
        }

        private void ShowProcessorMenu()
        {
            var menu = new GenericMenu();
            bool any = false;

            foreach (var t in ProcessorCache.ProcessorTypes)
            {
                if (t == null || t.IsAbstract || t.IsInterface || t.ContainsGenericParameters)
                    continue;

                if (!IsCompatibleProcessorForController(t))
                    continue;

                if (ProcessorsAlreadyContainsType(t))
                    continue;

                any = true;
                var cached = t;
                menu.AddItem(new GUIContent(t.Name), false, () => AddManagedReference(processorsProperty, cached));
            }

            if (!any)
                menu.AddDisabledItem(new GUIContent("No compatible processors found"));

            menu.ShowAsContext();
        }

        private bool IsCompatibleProcessorForController(Type candidate)
        {
            if (candidate == null) return false;
            if (controllerSettingType == null || controllerContextType == null || controllerProcessorType == null)
                return false;

            if (!controllerProcessorType.IsAssignableFrom(candidate))
                return false;

            foreach (var iface in candidate.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                if (iface.GetGenericTypeDefinition() != typeof(IProcessor<,>)) continue;

                var args = iface.GetGenericArguments();
                bool settingMatch = controllerSettingType.IsAssignableFrom(args[0]);
                bool contextMatch = controllerContextType.IsAssignableFrom(args[1]);

                if (settingMatch && contextMatch) return true;
            }

            return false;
        }

        private bool ProcessorsAlreadyContainsType(Type type)
        {
            if (processorsProperty == null) return false;

            for (int i = 0; i < processorsProperty.arraySize; i++)
            {
                var existing = processorsProperty.GetArrayElementAtIndex(i).managedReferenceValue;
                if (existing != null && existing.GetType() == type) return true;
            }

            return false;
        }

        private void AddManagedReference(SerializedProperty arrayProperty, Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (type == null || type.IsAbstract || type.IsInterface || type.ContainsGenericParameters)
                return;

            Undo.RecordObject(controller, "Add Element");
            serializedObject.Update();

            int index = arrayProperty.arraySize;
            arrayProperty.InsertArrayElementAtIndex(index);
            arrayProperty.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        public override void OnInspectorGUI()
        {
            if (controller == null)
                return;

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script",
                MonoScript.FromMonoBehaviour((MonoBehaviour)target),
                typeof(MonoScript), false);
            GUI.enabled = true;

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
    }
}