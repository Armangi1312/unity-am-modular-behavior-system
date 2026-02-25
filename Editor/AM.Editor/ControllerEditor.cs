using AM.Core;
using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AM.Editor
{
    [CustomEditor(typeof(Controller), true)]
    internal class ControllerEditor : UnityEditor.Editor
    {
        private ReorderableList processorList;
        private ReorderableList settingList;
        private ReorderableList contextList;

        private SerializedProperty processorsProperty;
        private SerializedProperty settingsSerializedObjectsProperty; 
        private SerializedProperty contextsSerializedObjectsProperty;
        
        private SerializedProperty settingsPropertyRoot;
        private SerializedProperty contextsPropertyRoot;

        private bool settingsFoldout = true;
        private bool contextsFoldout = true;

        private Controller controller;

        private void OnEnable()
        {
            controller = target as Controller;
            if (controller == null)
                return;

            // find processors list (field name must match exactly in the concrete controller)
            processorsProperty = serializedObject.FindProperty("processors");

            // settings.SerializedObjects and contexts.SerializedObjects
            settingsPropertyRoot = serializedObject.FindProperty("settings");
            if (settingsPropertyRoot != null)
            {
                settingsSerializedObjectsProperty = settingsPropertyRoot.FindPropertyRelative("SerializedObjects");
            }

            contextsPropertyRoot = serializedObject.FindProperty("contexts");
            if (contextsPropertyRoot != null)
            {
                contextsSerializedObjectsProperty = contextsPropertyRoot.FindPropertyRelative("SerializedObjects");
            }

            if (processorsProperty != null)
                InitializeProcessorList();

            if (settingsSerializedObjectsProperty != null)
                InitializeSettingList();

            if (contextsSerializedObjectsProperty != null)
                InitializeContextList();
        }

        // -------------------------
        // Processor list
        // -------------------------
        private void InitializeProcessorList()
        {
            processorList = new ReorderableList(serializedObject, processorsProperty, true, true, true, true);

            processorList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Processors");

            processorList.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = processorsProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                if (element != null && element.managedReferenceValue != null)
                {
                    var type = element.managedReferenceValue.GetType();
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        type.Name, EditorStyles.boldLabel);
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                }

                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };

            processorList.elementHeightCallback = index =>
            {
                var element = processorsProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6f;
            };

            processorList.onAddDropdownCallback = ShowProcessorMenu;

            processorList.onRemoveCallback = list =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Debug.LogError("[Controller] Cannot remove processors during Play Mode.", controller);
                    EditorUtility.DisplayDialog("Modification Blocked",
                        "Processors cannot be removed while in Play Mode.\nStop the game before editing.", "OK");
                    return;
                }

                // perform default behavior (removes the element)
                ReorderableList.defaultBehaviours.DoRemoveButton(list);

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            };
        }

        private void ShowProcessorMenu(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();

            var types = ProcessorCache.ProcessorTypes;
            if (types == null || types.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No processors found"));
            }
            else
            {
                foreach (var t in types)
                {
                    var cached = t;
                    menu.AddItem(new GUIContent(t.Name), false, () => AddProcessor(cached));
                }
            }

            menu.ShowAsContext();
        }

        private void AddProcessor(Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Controller] Cannot add processors during Play Mode.", controller);
                EditorUtility.DisplayDialog("Modification Blocked",
                    "Processors cannot be added while in Play Mode.\nStop the game before editing.", "OK");
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

        // -------------------------
        // Setting list (Registry.SerializedObjects)
        // -------------------------
        private void InitializeSettingList()
        {
            settingList = new ReorderableList(serializedObject, settingsSerializedObjectsProperty, true, true, true, true);

            settingList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Settings");

            settingList.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = settingsSerializedObjectsProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                if (element != null && element.managedReferenceValue != null)
                {
                    var type = element.managedReferenceValue.GetType();
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        type.Name, EditorStyles.boldLabel);
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                }

                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };

            settingList.elementHeightCallback = index =>
            {
                var element = settingsSerializedObjectsProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6f;
            };

            settingList.onAddDropdownCallback = ShowSettingMenu;

            settingList.onRemoveCallback = list =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Debug.LogError("[Controller] Cannot remove settings during Play Mode.", controller);
                    EditorUtility.DisplayDialog("Modification Blocked",
                        "Settings cannot be removed while in Play Mode.\nStop the game before editing.", "OK");
                    return;
                }

                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            };
        }

        private void ShowSettingMenu(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();

            // assume SettingCache.SettingTypes exists and contains Type[]
            var types = SettingCache.SettingTypes;

            if (types == null || types.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No settings found"));
            }
            else
            {
                bool any = false;
                foreach (var t in types)
                {
                    if (t.IsAbstract || t.IsInterface) continue;
                    any = true;
                    var cached = t;
                    menu.AddItem(new GUIContent(t.Name), false, () => AddSetting(cached));
                }

                if (!any) menu.AddDisabledItem(new GUIContent("No concrete settings found"));
            }

            menu.ShowAsContext();
        }

        private void AddSetting(Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Controller] Cannot add settings during Play Mode.", controller);
                EditorUtility.DisplayDialog("Modification Blocked",
                    "Settings cannot be added while in Play Mode.\nStop the game before editing.", "OK");
                return;
            }

            Undo.RecordObject(controller, "Add Setting");
            serializedObject.Update();

            int index = settingsSerializedObjectsProperty.arraySize;
            settingsSerializedObjectsProperty.InsertArrayElementAtIndex(index);

            var element = settingsSerializedObjectsProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        // -------------------------
        // Context list (Registry.SerializedObjects)
        // -------------------------
        private void InitializeContextList()
        {
            contextList = new ReorderableList(serializedObject, contextsSerializedObjectsProperty, true, true, true, true);

            contextList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Contexts");

            contextList.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = contextsSerializedObjectsProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                if (element != null && element.managedReferenceValue != null)
                {
                    var type = element.managedReferenceValue.GetType();
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        type.Name, EditorStyles.boldLabel);
                    rect.y += EditorGUIUtility.singleLineHeight + 2;
                }

                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };

            contextList.elementHeightCallback = index =>
            {
                var element = contextsSerializedObjectsProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6f;
            };

            contextList.onAddDropdownCallback = ShowContextMenu;

            contextList.onRemoveCallback = list =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Debug.LogError("[Controller] Cannot remove contexts during Play Mode.", controller);
                    EditorUtility.DisplayDialog("Modification Blocked",
                        "Contexts cannot be removed while in Play Mode.\nStop the game before editing.", "OK");
                    return;
                }

                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            };
        }

        private void ShowContextMenu(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();

            // assume ContextCache.ContextTypes exists
            var types = ContextCache.ContextTypes;

            if (types == null || types.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No contexts found"));
            }
            else
            {
                bool any = false;
                foreach (var t in types)
                {
                    if (t.IsAbstract || t.IsInterface) continue;
                    any = true;
                    var cached = t;
                    menu.AddItem(new GUIContent(t.Name), false, () => AddContext(cached));
                }

                if (!any) menu.AddDisabledItem(new GUIContent("No concrete contexts found"));
            }

            menu.ShowAsContext();
        }

        private void AddContext(Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[Controller] Cannot add contexts during Play Mode.", controller);
                EditorUtility.DisplayDialog("Modification Blocked",
                    "Contexts cannot be added while in Play Mode.\nStop the game before editing.", "OK");
                return;
            }

            Undo.RecordObject(controller, "Add Context");
            serializedObject.Update();

            int index = contextsSerializedObjectsProperty.arraySize;
            contextsSerializedObjectsProperty.InsertArrayElementAtIndex(index);

            var element = contextsSerializedObjectsProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        // -------------------------
        // Inspector GUI
        // -------------------------
        public override void OnInspectorGUI()
        {
            if (controller == null)
                return;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            GUILayout.Space(4);
            processorList?.DoLayoutList();

            GUILayout.Space(6);
            settingList?.DoLayoutList();

            GUILayout.Space(8);
            contextList?.DoLayoutList();

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

        // -------------------------
        // Registry drawer helper
        // -------------------------
        private void DrawRegistry(string title, ref bool foldout, IList list, bool editable)
        {
            if (list == null)
            {
                EditorGUILayout.HelpBox($"{title} not available", MessageType.Warning);
                return;
            }

            foldout = EditorGUILayout.Foldout(foldout, title, true);
            if (!foldout) return;

            EditorGUI.indentLevel++;
            if (list.Count == 0)
            {
                EditorGUILayout.LabelField("(empty)");
            }
            else
            {
                if (!editable) EditorGUI.BeginDisabledGroup(true);
                RegistryEditorHelper.DrawSerializedObjects(list);
                if (!editable) EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel--;
        }
    }
}