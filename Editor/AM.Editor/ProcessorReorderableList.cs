using AM.Core.Utilities;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AM.Editor
{
    internal sealed class ProcessorReorderableList
    {
        private readonly ReorderableList list;
        private readonly SerializedProperty property;
        private readonly SerializedObject serializedObject;
        private readonly ProcessorCompatibilityChecker checker;
        private readonly HashSet<Type> addedTypes = new();

        public ProcessorReorderableList(
            SerializedObject serializedObject,
            SerializedProperty property,
            ProcessorCompatibilityChecker checker)
        {
            this.serializedObject = serializedObject;
            this.property = property;
            this.checker = checker;
            this.list = Build();
        }

        public void DoLayoutList() => list.DoLayoutList();

        private ReorderableList Build()
        {
            return new ReorderableList(serializedObject, property, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = GetElementHeight,
                onAddDropdownCallback = (_, _) => ShowMenu(),
                onRemoveCallback = OnRemove,
                drawNoneElementCallback = DrawNoneElement
            };
        }

        private static void DrawHeader(Rect rect) => EditorGUI.LabelField(rect, "Processors");

        private static void DrawNoneElement(Rect rect) => EditorGUI.LabelField(rect, "No processors. Add one with +", EditorStyles.centeredGreyMiniLabel);

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var element = property.GetArrayElementAtIndex(index);
            rect.y += 2;

            var obj = element.managedReferenceValue;
            string label = obj == null ? "Null" : obj.GetType().Name;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), label, EditorStyles.boldLabel);

            rect.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(rect, element, GUIContent.none, true);
        }

        private float GetElementHeight(int index)
        {
            var element = property.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(element, true) + 6f;
        }

        private void OnRemove(ReorderableList reorderableList)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            ReorderableList.defaultBehaviours.DoRemoveButton(reorderableList);
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowMenu()
        {
            RefreshAddedTypes();

            var menu = new GenericMenu();
            bool any = false;

            foreach (var type in ProcessorCache.ProcessorTypes)
            {
                if (!checker.IsCompatible(type)) continue;
                if (addedTypes.Contains(type)) continue;

                any = true;
                var cached = type;
                menu.AddItem(new GUIContent(type.Name), false, () => AddProcessor(cached));
            }

            if (!any)
                menu.AddDisabledItem(new GUIContent("No compatible processors found"));

            menu.ShowAsContext();
        }

        private void RefreshAddedTypes()
        {
            addedTypes.Clear();

            for (int i = 0; i < property.arraySize; i++)
            {
                var existing = property.GetArrayElementAtIndex(i).managedReferenceValue;

                if (existing != null)
                    addedTypes.Add(existing.GetType());
            }
        }

        private void AddProcessor(Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!checker.IsCandidate(type)) return;

            serializedObject.Update();

            int index = property.arraySize;
            property.InsertArrayElementAtIndex(index);
            property.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
