using AM.Core.Utilities;
using GraphProcessor;
using UnityEngine;

namespace AM.Core.Pipelines.Node
{
    [CreateAssetMenu(menuName = "Pipeline Graph")]
    public class PipelineNodeGraph : BaseGraph
    {
        [SerializeField] private SceneObjectReference<Controller> controllerReference = new();

        public Controller TargetController
        {
            get => controllerReference.Value;
            set => controllerReference.Value = value;
        }

#if UNITY_EDITOR
        public void Resolve() => controllerReference.Resolve();
#endif
    }
}
