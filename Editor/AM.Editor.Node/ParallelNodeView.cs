using AM.Core;
using AM.Core.Pipelines.Node;
using AM.Node.Pipelines;
using GraphProcessor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AM.Editor.Node
{
    [NodeCustomEditor(typeof(ParallelNode))]
    public class ParallelNodeView : BaseNodeView
    {
        private ParallelNode node;
        private ListView listView;

        public override void Enable()
        {
            node = nodeTarget as ParallelNode;
            node.Processors ??= new List<IProcessor>();

            BuildListView();
            BuildAddButton();
        }

        private void BuildListView()
        {
            listView = new ListView(
                itemsSource: node.Processors,
                itemHeight: 22,
                makeItem: () =>
                {
                    VisualElement row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.alignItems = Align.Center;

                    Label label = new Label();
                    label.style.flexGrow = 1;
                    label.name = "label";

                    Button removeButton = new Button
                    {
                        text = "−",
                        name = "remove"
                    };

                    row.Add(label);
                    row.Add(removeButton);
                    return row;
                },
                bindItem: (element, i) =>
                {
                    Label label = element.Q<Label>("label");
                    Button removeButton = element.Q<Button>("remove");

                    label.text = node.Processors[i]?.GetType().Name ?? "Null";

                    removeButton.clicked -= null;
                    removeButton.clicked += () =>
                    {
                        owner.RegisterCompleteObjectUndo("Removed Processor from ParallelNode");
                        node.Processors.RemoveAt(i);
                        listView.Rebuild();
                    };
                }
            )
            {
                showBorder = true,
                showFoldoutHeader = true,
                headerTitle = "Processors",
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showAddRemoveFooter = false
            };
            listView.style.minHeight = 80;

            controlsContainer.Add(listView);
        }

        private void BuildAddButton()
        {
            Button addButton = new Button(ShowProcessorMenu) { text = "+ Add Processor" };
            controlsContainer.Add(addButton);
        }

        private void ShowProcessorMenu()
        {
            PipelineNodeGraph graph = owner.graph as PipelineNodeGraph;
            Controller controller = graph.TargetController;

            if (!controller)
            {
                EditorUtility.DisplayDialog("Null Error", "Controller is null", "OK");
                return;
            }

            GenericMenu menu = new GenericMenu();
            bool any = false;

            if (controller.GetProcessor() is not IList rawList) return;

            foreach (var item in rawList)
            {
                if (item is not IProcessor processor) continue;
                if (processor == null) continue;

                if (AlreadyAdded(processor.GetType())) continue;

                any = true;

                string path = $"{processor.GetType().Name}";
                var captured = processor;

                menu.AddItem(
                    new GUIContent(path),
                    false,
                    () => AddProcessor(captured)
                );
            }
            

            if (!any)
                menu.AddDisabledItem(new GUIContent("No available processors"));

            menu.ShowAsContext();
        }

        private bool AlreadyAdded(Type type)
        {
            foreach (IProcessor p in node.Processors)
                if (p != null && p.GetType() == type) return true;

            return false;
        }

        private void AddProcessor(IProcessor processor)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            owner.RegisterCompleteObjectUndo("Added Processor to ParallelNode");
            node.Processors.Add(processor);
            listView.Rebuild();
        }
    }
}