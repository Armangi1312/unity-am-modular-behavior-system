using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using AM.Core;

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

        // Controller가 알려주는 타입들 (Controller 구현체에서 제공)
        private Type controllerSettingType;
        private Type controllerContextType;
        private Type controllerProcessorType;

        private void OnEnable()
        {
            controller = target as Controller;
            if (controller == null)
                return;

            ResolveControllerTypesFromControllerAPIs();

            // SerializedProperty 참조 획득
            processorsProperty = serializedObject.FindProperty("processors");

            settingsRootProperty = serializedObject.FindProperty("settings");
            contextsRootProperty = serializedObject.FindProperty("contexts");

            if (settingsRootProperty != null)
                settingsSerializedObjectsProperty = settingsRootProperty.FindPropertyRelative("serializedObjects");

            if (contextsRootProperty != null)
                contextsSerializedObjectsProperty = contextsRootProperty.FindPropertyRelative("serializedObjects");

            // ReorderableList 생성 (있을 때만)
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
        // Controller의 Setting/Context/Processor 타입을 Controller API에서 조회
        // ============================================================
        private void ResolveControllerTypesFromControllerAPIs()
        {
            controllerSettingType = null;
            controllerContextType = null;
            controllerProcessorType = null;

            try
            {
                // Controller 인터페이스에서 제공하는 메서드 사용
                controllerSettingType = controller.SettingType();
                controllerContextType = controller.ContextType();
                controllerProcessorType = controller.ProcessorType();
            }
            catch
            {
                // 예외 발생 시 null 허용(호환 검사에서 걸러짐)
                controllerSettingType = null;
                controllerContextType = null;
                controllerProcessorType = null;
            }
        }

        // ============================================================
        // Processor 리스트 (ReorderableList)
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

                // 타입 라벨 표시
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

            // ProcessorCache에서 후보를 가져옴 (프로젝트 단위 캐시 가정)
            foreach (var t in ProcessorCache.ProcessorTypes)
            {
                if (t == null || t.IsAbstract || t.IsInterface || t.ContainsGenericParameters)
                    continue;

                if (!IsCompatibleProcessorForController(t))
                    continue;

                // 중복 방지: 이미 같은 타입이 등록되어 있으면 메뉴에 표시하지 않음
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

        // Controller의 제약(ProcessorType, SettingType, ContextType)에 대해 후보 타입이 허용되는지 검사
        private bool IsCompatibleProcessorForController(Type candidate)
        {
            // 기본적 안전 검사
            if (candidate == null) return false;
            if (controllerSettingType == null || controllerContextType == null || controllerProcessorType == null)
                return false;

            // 1) Controller가 허용한 Processor 타입 계열인지 확인
            if (!controllerProcessorType.IsAssignableFrom(candidate))
                return false;

            // 2) IProcessor<TSetting, TContext> 인터페이스 구현 여부 확인
            foreach (var iface in candidate.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;
                var def = iface.GetGenericTypeDefinition();
                if (def != typeof(IProcessor<,>)) continue;

                var args = iface.GetGenericArguments();
                var settingArg = args[0];
                var contextArg = args[1];

                bool settingMatch = controllerSettingType.IsAssignableFrom(settingArg);
                bool contextMatch = controllerContextType.IsAssignableFrom(contextArg);

                if (settingMatch && contextMatch) return true;
            }

            return false;
        }

        // processors 리스트에 이미 같은 타입이 있는지 검사
        private bool ProcessorsAlreadyContainsType(Type type)
        {
            if (processorsProperty == null) return false;

            for (int i = 0; i < processorsProperty.arraySize; i++)
            {
                var element = processorsProperty.GetArrayElementAtIndex(i);
                var existing = element.managedReferenceValue;
                if (existing == null) continue;
                if (existing.GetType() == type) return true;
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

                // 실제 타입 이름을 큰 글씨로 보여줍니다.
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

                    // 동일 타입 중복 추가 방지
                    if (ManagedReferenceArrayContainsType(arrayProperty, t))
                        continue;

                    any = true;
                    var cached = t;
                    menu.AddItem(new GUIContent(t.Name), false, () => AddManagedReference(arrayProperty, cached));
                }

                if (!any)
                    menu.AddDisabledItem(new GUIContent("No compatible types found"));

                menu.ShowAsContext();
            };

            list.onRemoveCallback = l =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;

                ReorderableList.defaultBehaviours.DoRemoveButton(l);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
            };

            return list;
        }

        // Settings/Contexts 후보 타입이 expectedType 기준에서 허용되는지 검사
        private bool IsCandidateAssignableToExpected(Type candidate, Type expectedType)
        {
            if (expectedType == null)
                return false;

            if (expectedType.IsAssignableFrom(candidate))
                return true;

            // generic interface 등 특수 케이스에 대해 검사
            if (expectedType.IsGenericTypeDefinition)
            {
                foreach (var iface in candidate.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;
                    if (iface.GetGenericTypeDefinition() == expectedType) return true;
                }
            }

            return false;
        }

        // arrayProperty(serializedObjects)에 이미 같은 타입이 들어있는지 확인
        private bool ManagedReferenceArrayContainsType(SerializedProperty arrayProperty, Type type)
        {
            if (arrayProperty == null) return false;

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                var el = arrayProperty.GetArrayElementAtIndex(i);
                var existing = el.managedReferenceValue;
                if (existing == null) continue;
                if (existing.GetType() == type) return true;
            }

            return false;
        }

        // ============================================================
        // element 추가(SerializeReference)
        // ============================================================
        private void AddManagedReference(SerializedProperty arrayProperty, Type type)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (type == null) return;
            if (type.IsAbstract || type.IsInterface || type.ContainsGenericParameters) return;

            Undo.RecordObject(controller, "Add Element");
            serializedObject.Update();

            int index = arrayProperty.arraySize;
            arrayProperty.InsertArrayElementAtIndex(index);

            var element = arrayProperty.GetArrayElementAtIndex(index);

            // 인스턴스 생성 후 managed reference에 할당합니다.
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

            // Script 필드만 표시 (Unity 기본)
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script",
                MonoScript.FromMonoBehaviour((MonoBehaviour)target),
                typeof(MonoScript), false);
            GUI.enabled = true;

            GUILayout.Space(8);

            processorList?.DoLayoutList();
            GUILayout.Space(6);

            settingList?.DoLayoutList();
            GUILayout.Space(6);

            contextList?.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}