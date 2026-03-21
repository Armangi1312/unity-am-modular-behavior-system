using AM.Core;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AM.Editor
{
    [CustomPropertyDrawer(typeof(Registry<>))]
    internal class RegistryDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> listCache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var list = GetOrCreateList(property);
            if (list == null)
            {
                EditorGUI.LabelField(position, label.text, "serializedObjects not found");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            list.DoList(position);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var list = GetOrCreateList(property);
            return list?.GetHeight() ?? EditorGUIUtility.singleLineHeight;
        }

        private ReorderableList GetOrCreateList(SerializedProperty property)
        {
            string key = property.propertyPath;

            if (listCache.TryGetValue(key, out var cached))
                return cached;

            var arrayProp = property.FindPropertyRelative("serializedObjects");
            if (arrayProp == null)
                return null;

            Type elementType = ResolveElementType();

            var list = CreateList(property.serializedObject, arrayProp, property.displayName, elementType);
            listCache[key] = list;
            return list;
        }

        private Type ResolveElementType()
        {
            var fieldType = fieldInfo.FieldType;

            if (fieldType.IsArray)
                fieldType = fieldType.GetElementType();

            if (fieldType != null && fieldType.IsGenericType &&
                fieldType.GetGenericTypeDefinition() == typeof(List<>))
                fieldType = fieldType.GetGenericArguments()[0];

            if (fieldType != null &&
                fieldType.IsGenericType &&
                fieldType.GetGenericTypeDefinition() == typeof(Registry<>))
            {
                return fieldType.GetGenericArguments()[0];
            }

            return null;
        }

        private List<Type> CollectCandidateTypes(Type elementType)
        {
            var result = new List<Type>();
            if (elementType == null)
                return result;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null || t.IsAbstract || t.IsInterface || t.ContainsGenericParameters)
                        continue;

                    if (elementType.IsAssignableFrom(t))
                        result.Add(t);
                }
            }

            return result;
        }

        private ReorderableList CreateList(
            SerializedObject serializedObj,
            SerializedProperty arrayProperty,
            string header,
            Type elementType)
        {
            var list = new ReorderableList(serializedObj, arrayProperty, true, true, true, true)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, header);
                },

                drawElementCallback = (rect, index, active, focused) =>
                    {
                        var element = arrayProperty.GetArrayElementAtIndex(index);
                        rect.y += 2;

                        var obj = element.managedReferenceValue;
                        string label = obj == null ? "Null" : obj.GetType().Name;

                        EditorGUI.LabelField(
                            new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                            label,
                            EditorStyles.boldLabel);

                        rect.y += EditorGUIUtility.singleLineHeight + 2;
                        EditorGUI.PropertyField(rect, element, GUIContent.none, true);
                    },

                elementHeightCallback = index =>
                    {
                        var element = arrayProperty.GetArrayElementAtIndex(index);
                        return EditorGUI.GetPropertyHeight(element, true) + 22f;
                    },

                drawNoneElementCallback = rect =>
                    {
                        EditorGUI.LabelField(
                            rect,
                            $"No {header.ToLower()}. Add one with +",
                            EditorStyles.centeredGreyMiniLabel);
                    },

                onAddDropdownCallback = (rect, l) =>
                    {
                        ShowAddMenu(serializedObj, arrayProperty, elementType);
                    },

                onRemoveCallback = l =>
                    {
                        if (EditorApplication.isPlayingOrWillChangePlaymode)
                            return;

                        ReorderableList.defaultBehaviours.DoRemoveButton(l);
                        serializedObj.ApplyModifiedProperties();
                    }
            };

            return list;
        }

        private void ShowAddMenu(
            SerializedObject serializedObj,
            SerializedProperty arrayProperty,
            Type elementType)
        {
            var menu = new GenericMenu();
            var candidates = CollectCandidateTypes(elementType);
            bool any = false;

            foreach (var t in candidates)
            {
                if (ArrayContainsType(arrayProperty, t))
                    continue;

                any = true;
                var cached = t;
                menu.AddItem(new GUIContent(t.Name), false, () =>
                {
                    AddElement(serializedObj, arrayProperty, cached);
                });
            }

            if (!any)
                menu.AddDisabledItem(new GUIContent("No compatible types found"));

            menu.ShowAsContext();
        }

        private bool ArrayContainsType(SerializedProperty arrayProperty, Type type)
        {
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                var el = arrayProperty.GetArrayElementAtIndex(i);
                var existing = el.managedReferenceValue;
                if (existing != null && existing.GetType() == type)
                    return true;
            }

            return false;
        }

        private void AddElement(SerializedObject serializedObj, SerializedProperty arrayProperty, Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (type == null || type.IsAbstract || type.IsInterface || type.ContainsGenericParameters)
                return;

            serializedObj.Update();

            int index = arrayProperty.arraySize;
            arrayProperty.InsertArrayElementAtIndex(index);

            var element = arrayProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = Activator.CreateInstance(type);

            serializedObj.ApplyModifiedProperties();

            Undo.RecordObject(serializedObj.targetObject, $"Add {type.Name}");
            EditorUtility.SetDirty(serializedObj.targetObject);
        }
    }
}