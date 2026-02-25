using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using AM.Core;

namespace AM.Editor
{
    [CustomEditor(typeof(Controller), true)]
    public class ControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty processorsProperty;
        private ReorderableList processorList;

        private bool contextsFoldout = true;
        private bool settingsFoldout = true;

        private Controller controller;

        private void OnEnable()
        {
            controller = target as Controller;

            if (controller == null)
                return;

            processorsProperty = serializedObject.FindProperty("processors");

            InitializeProcessorList();

            processorList.onRemoveCallback = list =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Debug.LogError(
                        "[Controller] Processors cannot be removed during Play Mode.",
                        controller);

                    return;
                }

                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };
        }

        private void InitializeProcessorList()
        {
            processorList = new ReorderableList(
                serializedObject,
                processorsProperty,
                true, true, true, true);

            processorList.drawHeaderCallback = DrawProcessorHeader;
            processorList.drawElementCallback = DrawProcessorElement;
            processorList.elementHeightCallback = GetProcessorElementHeight;
            processorList.onAddDropdownCallback = ShowProcessorMenu;
        }

        public override void OnInspectorGUI()
        {
            if (controller == null)
                return;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(4);
            processorList.DoLayoutList();

            GUILayout.Space(6);
            DrawRegistry(
                "Settings",
                ref settingsFoldout,
                controller.Settings?.SerializedObjects,
                editable: true);

            GUILayout.Space(8);
            DrawRegistry(
                "Contexts",
                ref contextsFoldout,
                controller.Contexts?.SerializedObjects,
                editable: false);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawProcessorHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Processors");
        }

        private float GetProcessorElementHeight(int index)
        {
            var element = processorsProperty.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 6f;
        }

        private void DrawProcessorElement(Rect rect, int index, bool active, bool focused)
        {
            var element = processorsProperty.GetArrayElementAtIndex(index);

            rect.y += 2;

            if (element.managedReferenceValue != null)
            {
                string typeName = element.managedReferenceValue.GetType().Name;

                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    typeName,
                    EditorStyles.boldLabel);

                rect.y += EditorGUIUtility.singleLineHeight + 2;
            }

            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        }

        private void ShowProcessorMenu(Rect rect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            if (ProcessorCache.ProcessorTypes == null)
            {
                menu.AddDisabledItem(new GUIContent("No processors found"));
            }
            else
            {
                foreach (var type in ProcessorCache.ProcessorTypes)
                {
                    var cachedType = type;
                    menu.AddItem(
                        new GUIContent(type.Name),
                        false,
                        () => AddProcessor(cachedType));
                }
            }

            menu.ShowAsContext();
        }

        private void AddProcessor(Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(
                    "[Controller] Processors cannot be modified during Play Mode.",
                    controller);

                EditorUtility.DisplayDialog(
                    "Modification Blocked",
                    "Processors cannot be added while in Play Mode.\nStop the game before editing.",
                    "OK");

                return;
            }

            Undo.RecordObject(controller, "Add Processor");

            serializedObject.Update();

            int index = processorsProperty.arraySize;
            processorsProperty.InsertArrayElementAtIndex(index);

            var element = processorsProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        private void DrawRegistry(
            string title,
            ref bool foldout,
            IList list,
            bool editable)
        {
            if (list == null)
            {
                EditorGUILayout.HelpBox($"{title} not found", MessageType.Warning);
                return;
            }

            foldout = EditorGUILayout.Foldout(foldout, title, true);
            if (!foldout)
                return;

            EditorGUI.indentLevel++;

            if (list.Count == 0)
            {
                EditorGUILayout.LabelField("(empty)");
            }
            else
            {
                if (!editable)
                    EditorGUI.BeginDisabledGroup(true);

                RegistryEditorHelper.DrawSerializedObjects(list);

                if (!editable)
                    EditorGUI.EndDisabledGroup();
            }

            EditorGUI.indentLevel--;
        }
    }
}