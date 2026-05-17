using AM.Core.Pipelines.Node;
using GraphProcessor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AM.Editor.Node
{
    public class PipelineNodeGraphWindow : BaseGraphWindow
    {
        [OnOpenAsset(0)]
        public static bool OnGraphOpened(int instanceID, int line)
        {
            #pragma warning disable CS0618
            var asset = EditorUtility.InstanceIDToObject(instanceID) as PipelineNodeGraph;
            #pragma warning restore CS0618

            if (asset == null) return false;

            GetWindow<PipelineNodeGraphWindow>().InitializeGraph(asset);
            return true;
        }

        protected override void InitializeWindow(BaseGraph graph)
        {
            titleContent = new GUIContent("Pipeline Graph");

            graphView ??= new PipelineNodeGraphView(this);

            rootView.Add(graphView);
        }
    }
}