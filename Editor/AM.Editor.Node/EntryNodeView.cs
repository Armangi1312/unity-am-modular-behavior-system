using AM.Core;
using AM.Core.Pipelines.Node;
using AM.Node.Pipelines;
using GraphProcessor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AM.Editor.Node
{
    [NodeCustomEditor(typeof(EntryNode))]
    public class EntryNodeView : BaseNodeView
    {
        public override void Enable()
        {
            var node = nodeTarget as EntryNode;

            ObjectField objectField = new()
            {
                objectType = typeof(Controller),
                allowSceneObjects = true
            };

            objectField.RegisterValueChangedCallback(evt =>
            {
                owner.RegisterCompleteObjectUndo("Updated EntryNode");
                var graph = owner.graph as PipelineNodeGraph;

                graph.TargetController = evt.newValue as Controller;
            });

            node.onProcessed += () =>
            {
                var graph = owner.graph as PipelineNodeGraph;
                objectField.SetValueWithoutNotify(graph != null ? graph.TargetController : null);
            };

            schedule.Execute(() =>
            {
                var graph = owner.graph as PipelineNodeGraph;
                graph.Resolve();
                objectField.SetValueWithoutNotify(graph != null ? graph.TargetController : null);
            });

            controlsContainer.Add(objectField);
        }
    }
}