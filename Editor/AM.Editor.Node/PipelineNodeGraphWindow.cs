using AM.Core.Pipelines.Node;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AM.Editor.Node
{
    public class PipelineNodeGraphWindow : BaseGraphWindow
    {
#if UNITY_6000_3_OR_NEWER
        [OnOpenAsset(0)]
        public static bool OnGraphOpened(EntityId entityId, int line)
        {
            var asset = EditorUtility.EntityIdToObject(entityId) as PipelineNodeGraph;

            if (asset == null) return false;

            GetWindow<PipelineNodeGraphWindow>().InitializeGraph(asset);
            return true;
        }
#else
        [OnOpenAsset(0)]
        public static bool OnGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as PipelineNodeGraph;

            if (asset == null) return false;

            GetWindow<PipelineNodeGraphWindow>().InitializeGraph(asset);
            return true;
        }
#endif

        protected override void InitializeWindow(BaseGraph graph)
        {
            titleContent = new GUIContent("Pipeline Graph");

            graphView ??= new PipelineNodeGraphView(this);

            rootView.Add(graphView);
        }
    }
}