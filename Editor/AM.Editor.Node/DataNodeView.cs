using System;
using System.Collections.Generic;
using System.Reflection;
using AM.Node.Data;
using GraphProcessor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AM.Editor.Node
{
    [NodeCustomEditor(typeof(ComponentDataNode))]
    public class ComponentDataNodeView : BaseNodeView
    {
        private ComponentDataNode node;

        private ObjectField objectField;
        private DropdownField componentDropdown;
        private Button memberButton;

        private const BindingFlags PublicFlags =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        public override void Enable()
        {
            node = nodeTarget as ComponentDataNode;

            BuildObjectField();
            BuildComponentDropdown();
            BuildMemberButton();

            schedule.Execute(() =>
            {
                node.Resolve();
                objectField.SetValueWithoutNotify(node.Target);

                if (node.Target != null)
                    OnTargetChanged(node.Target);
            });
        }

        private void BuildObjectField()
        {
            objectField = new ObjectField("Target")
            {
                objectType = typeof(UnityEngine.Object),
                allowSceneObjects = true
            };

            objectField.RegisterValueChangedCallback(v =>
            {
                owner.RegisterCompleteObjectUndo("Updated ContextNode Target");
                node.Target = v.newValue;
                OnTargetChanged(v.newValue);
            });

            controlsContainer.Add(objectField);
        }

        private void BuildComponentDropdown()
        {
            componentDropdown = new DropdownField("Component");
            componentDropdown.SetEnabled(false);

            componentDropdown.RegisterValueChangedCallback(v =>
            {
                owner.RegisterCompleteObjectUndo("Updated ContextNode Component");
                node.ComponentTypeName = v.newValue;
                UpdateMemberButton();
            });

            controlsContainer.Add(componentDropdown);
        }

        private void BuildMemberButton()
        {
            memberButton = new Button(OnMemberButtonClicked)
            {
                text = string.IsNullOrEmpty(node.Path) ? "No Function" : node.Path
            };

            memberButton.SetEnabled(false);
            controlsContainer.Add(memberButton);
        }

        private void OnMemberButtonClicked()
        {
            var target = ResolveSelectedComponent();
            if (target == null) return;

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("No Function"), string.IsNullOrEmpty(node.Path), () =>
            {
                owner.RegisterCompleteObjectUndo("Updated ContextNode Path");
                node.Path = null;
                memberButton.text = "No Function";
            });

            menu.AddSeparator("");

            CollectMenuItems(target.GetType(), "", menu);

            menu.ShowAsContext();
        }

        private void OnTargetChanged(UnityEngine.Object target)
        {
            componentDropdown.choices = new List<string>();
            componentDropdown.value = null;
            componentDropdown.SetEnabled(false);
            memberButton.SetEnabled(false);
            memberButton.text = "No Function";

            if (target == null) return;

            if (target is GameObject go)
            {
                var components = go.GetComponents<Component>();
                var choices = new List<string>();

                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    choices.Add(comp.GetType().Name);
                }

                componentDropdown.choices = choices;
                componentDropdown.SetEnabled(true);

                if (!string.IsNullOrEmpty(node.ComponentTypeName) && choices.Contains(node.ComponentTypeName))
                    componentDropdown.value = node.ComponentTypeName;
                else if (choices.Count > 0)
                    componentDropdown.index = 0;
            }
            else
            {
                componentDropdown.SetEnabled(false);
                UpdateMemberButton();
            }
        }

        private void UpdateMemberButton()
        {
            var target = ResolveSelectedComponent();
            memberButton.SetEnabled(target != null);
            memberButton.text = string.IsNullOrEmpty(node.Path) ? "No Function" : node.Path;
        }

        private UnityEngine.Object ResolveSelectedComponent()
        {
            if (node.Target is not GameObject go) return node.Target;
            if (string.IsNullOrEmpty(node.ComponentTypeName)) return null;

            foreach (var comp in go.GetComponents<Component>())
            {
                if (comp == null) continue;
                if (comp.GetType().Name == node.ComponentTypeName)
                    return comp;
            }

            return null;
        }

        private void CollectMenuItems(Type type, string prefix, GenericMenu menu)
        {
            var seen = new HashSet<string>();
            Type current = type;

            while (current != null && current != typeof(object))
            {
                foreach (var field in current.GetFields(PublicFlags))
                {
                    if (!seen.Add(field.Name)) continue;
                    var path = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}/{field.Name}";
                    AddMemberToMenu(path, menu);
                }

                foreach (var prop in current.GetProperties(PublicFlags))
                {
                    if (prop.GetIndexParameters().Length > 0) continue;
                    if (!prop.CanRead) continue;
                    if (!seen.Add(prop.Name)) continue;
                    var path = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}/{prop.Name}";
                    AddMemberToMenu(path, menu);
                }

                current = current.BaseType;
            }
        }

        private void AddMemberToMenu(string path, GenericMenu menu)
        {
            var captured = path;
            menu.AddItem(new GUIContent(path), node.Path == path, () =>
            {
                owner.RegisterCompleteObjectUndo("Updated ContextNode Path");
                node.Path = captured;
                memberButton.text = captured;
            });
        }
    }
}