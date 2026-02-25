using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AM.Editor
{
    public static class RegistryEditorHelper
    {
        private static readonly Dictionary<string, bool> foldoutStates = new();

        public static void DrawSerializedObjects(IEnumerable objects)
        {
            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                DrawSerializedObject(obj);
                EditorGUILayout.Space(4);
            }
        }

        private static void DrawSerializedObject(object obj)
        {
            if (obj == null) return;

            Type type = obj.GetType();
            EditorGUILayout.LabelField(type.Name, EditorStyles.boldLabel);

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null)
                    continue;

                DrawDecorators(field);

                object value = field.GetValue(obj);

                GUIContent label = new GUIContent(
                    GetDisplayName(field),
                    field.GetCustomAttribute<TooltipAttribute>()?.tooltip
                );

                // 복합 객체 → foldout
                if (!IsSimple(field.FieldType))
                {
                    string key = obj.GetHashCode() + field.Name;

                    if (DrawFoldout(key, label.text))
                    {
                        EditorGUI.indentLevel++;
                        DrawSerializedObject(value);
                        EditorGUI.indentLevel--;
                    }

                    continue;
                }

                value = DrawValue(field.FieldType, value, label, field);
                field.SetValue(obj, value);
            }
        }

        private static void DrawDecorators(MemberInfo member)
        {
            var header = member.GetCustomAttribute<HeaderAttribute>();
            if (header != null)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField(header.header, EditorStyles.boldLabel);
            }

            var space = member.GetCustomAttribute<SpaceAttribute>();
            if (space != null)
            {
                EditorGUILayout.Space(space.height);
            }
        }

        private static bool DrawFoldout(string key, string label)
        {
            if (!foldoutStates.ContainsKey(key))
                foldoutStates[key] = true;

            foldoutStates[key] = EditorGUILayout.Foldout(
                foldoutStates[key],
                label,
                true
            );

            return foldoutStates[key];
        }

        private static string GetDisplayName(FieldInfo field)
        {
            string name = field.Name;

            // <Name>k__BackingField → Name
            if (name.StartsWith("<") && name.Contains(">"))
            {
                int end = name.IndexOf('>');
                name = name.Substring(1, end - 1);
            }

            return ObjectNames.NicifyVariableName(name);
        }

        private static bool IsSimple(Type type)
        {
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type.IsEnum ||
                   type == typeof(Vector2) ||
                   type == typeof(Vector3) ||
                   type == typeof(Vector4) ||
                   type == typeof(Color) ||
                   type == typeof(Rect) ||
                   type == typeof(Bounds) ||
                   type == typeof(AnimationCurve) ||
                   type == typeof(Gradient) ||
                   typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        private static object DrawValue(Type type, object value, GUIContent label, MemberInfo member)
        {
            var range = member.GetCustomAttribute<RangeAttribute>();

            if (type == typeof(LayerMask))
            {
                LayerMask mask = value != null ? (LayerMask)value : default;
                mask.value = EditorGUILayout.MaskField(
                    label,
                    mask.value,
                    UnityEditorInternal.InternalEditorUtility.layers
                );
                return mask;
            }

            if (type == typeof(int))
            {
                int v = value != null ? (int)value : 0;
                if (range != null)
                    return EditorGUILayout.IntSlider(label, v, (int)range.min, (int)range.max);
                return EditorGUILayout.IntField(label, v);
            }

            if (type == typeof(float))
            {
                float v = value != null ? (float)value : 0f;
                if (range != null)
                    return EditorGUILayout.Slider(label, v, range.min, range.max);
                return EditorGUILayout.FloatField(label, v);
            }

            if (type == typeof(bool))
                return EditorGUILayout.Toggle(label, value != null && (bool)value);

            if (type == typeof(string))
                return EditorGUILayout.TextField(label, value as string ?? "");

            if (type.IsEnum)
            {
                value ??= Activator.CreateInstance(type);
                return EditorGUILayout.EnumPopup(label, (Enum)value);
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, type, true);

            if (type == typeof(Vector2))
                return EditorGUILayout.Vector2Field(label.text, value != null ? (Vector2)value : Vector2.zero);

            if (type == typeof(Vector3))
                return EditorGUILayout.Vector3Field(label.text, value != null ? (Vector3)value : Vector3.zero);

            if (type == typeof(Color))
                return EditorGUILayout.ColorField(label, value != null ? (Color)value : Color.white);

            if (type == typeof(AnimationCurve))
                return EditorGUILayout.CurveField(label, value as AnimationCurve ?? new AnimationCurve());

            if (type == typeof(Gradient))
            {
                if (value == null) value = new Gradient();
                EditorGUILayout.GradientField(label, (Gradient)value);
                return value;
            }

            // 리스트 / 배열 → ReorderableList
            if (typeof(IList).IsAssignableFrom(type))
            {
                IList list = value as IList;

                if (list == null)
                {
                    if (type.IsArray)
                        list = Array.CreateInstance(type.GetElementType(), 0);
                    else
                        list = Activator.CreateInstance(type) as IList;
                }

                Type elementType = type.IsArray
                    ? type.GetElementType()
                    : type.GetGenericArguments()[0];

                var reorderable = new ReorderableList(list, elementType, true, true, true, true);

                reorderable.drawHeaderCallback = rect =>
                    EditorGUI.LabelField(rect, label);

                reorderable.drawElementCallback = (rect, index, active, focused) =>
                {
                    if (index >= list.Count) return;

                    rect.y += 2;
                    list[index] = DrawValue(
                        elementType,
                        list[index],
                        GUIContent.none,
                        member
                    );
                };

                reorderable.onAddCallback = r =>
                {
                    list.Add(Activator.CreateInstance(elementType));
                };

                reorderable.DoLayoutList();
                return list;
            }

            // 복합 객체 재귀
            if (value == null)
                value = Activator.CreateInstance(type);

            DrawSerializedObject(value);
            return value;
        }
    }
}