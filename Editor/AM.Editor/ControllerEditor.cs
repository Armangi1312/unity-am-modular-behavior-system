using AM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private SerializedProperty settingsRootProperty;
        private SerializedProperty contextsRootProperty;
        private SerializedProperty settingsSerializedObjectsProperty;
        private SerializedProperty contextsSerializedObjectsProperty;

        private Controller controller;

        // 컨트롤러의 제네릭 인자 (예: ITestSetting, ITestContext)
        private Type controllerSettingType;
        private Type controllerContextType;

        private void OnEnable()
        {
            controller = target as Controller;
            if (controller == null)
                return;

            ResolveControllerGenericTypes();

            processorsProperty = serializedObject.FindProperty("processors");

            // Registry 필드 찾기 (Controller 내부에 있는 private fields 이름과 일치해야 함)
            settingsRootProperty = serializedObject.FindProperty("settings");
            contextsRootProperty = serializedObject.FindProperty("contexts");

            if (settingsRootProperty != null)
                settingsSerializedObjectsProperty = settingsRootProperty.FindPropertyRelative("serializedObjects");

            if (contextsRootProperty != null)
                contextsSerializedObjectsProperty = contextsRootProperty.FindPropertyRelative("serializedObjects");

            if (processorsProperty != null)
                processorList = CreateProcessorList();

            if (settingsSerializedObjectsProperty != null)
                settingList = CreateManagedReferenceList(
                    settingsSerializedObjectsProperty,
                    "Settings",
                    SettingCache.SettingTypes,
                    controllerSettingType);

            if (contextsSerializedObjectsProperty != null)
                contextList = CreateManagedReferenceList(
                    contextsSerializedObjectsProperty,
                    "Contexts",
                    ContextCache.ContextTypes,
                    controllerContextType);
        }

        // ============================================================
        // Controller<TSetting, TContext>에서 제네릭 인자 찾기
        // ============================================================
        private void ResolveControllerGenericTypes()
        {
            controllerSettingType = null;
            controllerContextType = null;

            Type t = controller.GetType();

            while (t != null)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Controller<,>))
                {
                    var args = t.GetGenericArguments();
                    controllerSettingType = args[0];
                    controllerContextType = args[1];
                    return;
                }
                t = t.BaseType;
            }
        }

        // ============================================================
        // Processor 리스트 (기존 로직 유지하되 보기 좋게)
        // ============================================================
        private ReorderableList CreateProcessorList()
        {
            var list = new ReorderableList(serializedObject, processorsProperty, true, true, true, true);

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Processors");
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = processorsProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                // 타입 라벨
                var obj = element.managedReferenceValue;
                string label = obj == null ? "Null" : obj.GetType().Name;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    label, EditorStyles.boldLabel);
                rect.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };

            list.elementHeightCallback = index =>
            {
                var element = processorsProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 6f;
            };

            list.onAddDropdownCallback = (rect, l) =>
            {
                ShowProcessorMenu();
            };

            list.onRemoveCallback = l =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                ReorderableList.defaultBehaviours.DoRemoveButton(l);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            };

            list.drawNoneElementCallback = rect =>
            {
                EditorGUI.LabelField(rect, "No processors. Add one with +", EditorStyles.centeredGreyMiniLabel);
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

                any = true;
                var cached = t;
                menu.AddItem(new GUIContent(t.Name), false, () => AddManagedReference(processorsProperty, cached));
            }

            if (!any)
                menu.AddDisabledItem(new GUIContent("No compatible processors found"));

            menu.ShowAsContext();
        }

        // Controller의 제네릭 인자와 비교해서 Processor가 호환되는지 검사
        private bool IsCompatibleProcessorForController(Type candidate)
        {
            foreach (var iface in candidate.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                if (iface.GetGenericTypeDefinition() != typeof(IProcessor<,>)) continue;

                var args = iface.GetGenericArguments();
                var settingArg = args[0];
                var contextArg = args[1];

                // controllerSettingType/controllerContextType이 null이면 허용하지 않음
                if (controllerSettingType == null || controllerContextType == null)
                    return false;

                // Controller가 요구하는 타입이 상위 타입인지(= candidate가 그것을 구현/상속)
                bool settingMatch = controllerSettingType.IsAssignableFrom(settingArg);
                bool contextMatch = controllerContextType.IsAssignableFrom(contextArg);

                if (settingMatch && contextMatch) return true;
            }

            return false;
        }

        // ============================================================
        // Generic ManagedReference 리스트 생성 (Settings / Contexts)
        // ============================================================
        private ReorderableList CreateManagedReferenceList(
            SerializedProperty arrayProperty,
            string header,
            IEnumerable<Type> candidateTypes,
            Type expectedType)
        {
            var list = new ReorderableList(serializedObject, arrayProperty, true, true, true, true);

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, header);
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = arrayProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                var obj = element.managedReferenceValue;
                string label = obj == null ? "Null" : obj.GetType().Name;

                // 실제 타입 이름을 큰 글씨로
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    label, EditorStyles.boldLabel);
                rect.y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };

            list.elementHeightCallback = index =>
            {
                var element = arrayProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 22f;
            };

            list.drawNoneElementCallback = rect =>
            {
                EditorGUI.LabelField(rect, $"No {header.ToLower()}. Add one with +", EditorStyles.centeredGreyMiniLabel);
            };

            list.onAddDropdownCallback = (rect, l) =>
            {
                var menu = new GenericMenu();
                bool any = false;

                foreach (var t in candidateTypes)
                {
                    if (t == null || t.IsAbstract || t.IsInterface || t.ContainsGenericParameters)
                        continue;

                    if (!IsCandidateAssignableToExpected(t, expectedType))
                        continue;

                    any = true;
                    var cached = t;
                    menu.AddItem(new GUIContent(t.Name), false, () => AddManagedReference(arrayProperty, cached));
                }

                if (!any)
                    menu.AddDisabledItem(new GUIContent("No compatible types found"));

                menu.ShowAsContext();
            };

            return list;
        }

        // Settings/Contexts 후보 타입이 expectedType 기준에서 허용되는지 검사
        private bool IsCandidateAssignableToExpected(Type candidate, Type expectedType)
        {
            if (expectedType == null)
                return false;

            // 기본적 케이스: expectedType이 상위 타입(인터페이스 또는 base class)인 경우
            if (expectedType.IsAssignableFrom(candidate))
                return true;

            // 보수적 보완: candidate가 인터페이스를 통해 expectedType을 구현하는지 확인 (대부분 IsAssignableFrom으로 커버되지만 안전하게 한 번 더 검사)
            var ifaces = candidate.GetInterfaces();
            if (ifaces.Any(i => i == expectedType))
                return true;

            // generic interface 등 특수 케이스에 대해 추가 검사 (예: 열린/닫힌 generic 정의 매칭)
            if (expectedType.IsGenericTypeDefinition)
            {
                foreach (var iface in candidate.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;
                    if (iface.GetGenericTypeDefinition() == expectedType) return true;
                }
            }

            // 추가적으로, expectedType이 concrete 타입이고 candidate가 expectedType의 base 타입이라서 양방향 허용을 원하면 아래를 활성화
            // if (candidate.IsAssignableFrom(expectedType)) return true;

            return false;
        }

        // ============================================================
        // element 추가(SerializeReference)
        // ============================================================
        private void AddManagedReference(SerializedProperty arrayProperty, Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            Undo.RecordObject(controller, "Add Element");
            serializedObject.Update();

            int index = arrayProperty.arraySize;
            arrayProperty.InsertArrayElementAtIndex(index);

            var element = arrayProperty.GetArrayElementAtIndex(index);

            // CreateInstance로 인스턴스 생성해서 managedReferenceValue에 할당
            element.managedReferenceValue = Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }

        // ============================================================
        // Inspector GUI
        // ============================================================
        public override void OnInspectorGUI()
        {
            if (controller == null)
                return;

            serializedObject.Update();

            processorList?.DoLayoutList();
            GUILayout.Space(6);

            settingList?.DoLayoutList();
            GUILayout.Space(6);

            contextList?.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}