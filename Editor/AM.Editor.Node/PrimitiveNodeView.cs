using AM.Node.Math.Primitives;
using GraphProcessor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AM.Editor.Node
{
    public abstract class PrimitiveNodeView<TNode, TField, TValue> : BaseNodeView
        where TNode : PrimitiveNode<TValue>
        where TField : BaseField<TValue>, new()
        where TValue : struct
    {
        public override void Enable()
        {
            var node = nodeTarget as TNode;
            var field = CreateField(node);

            node.onProcessed += () => field.value = node.Input;
            field.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo($"Updated {typeof(TNode).Name} input");
                node.Input = e.newValue;
            });

            controlsContainer.Add(field);
        }

        protected virtual TField CreateField(TNode node) => new() { value = node.Input };
    }

    [NodeCustomEditor(typeof(IntNode))]
    public class IntNodeView : PrimitiveNodeView<IntNode, IntegerField, int> { }

    [NodeCustomEditor(typeof(FloatNode))]
    public class FloatNodeView : PrimitiveNodeView<FloatNode, FloatField, float> { }

    [NodeCustomEditor(typeof(BoolNode))]
    public class BoolNodeView : PrimitiveNodeView<BoolNode, Toggle, bool> { }

    [NodeCustomEditor(typeof(Vector2Node))]
    public class Vector2NodeView : PrimitiveNodeView<Vector2Node, Vector2Field, Vector2> { }

    [NodeCustomEditor(typeof(Vector3Node))]
    public class Vector3NodeView : PrimitiveNodeView<Vector3Node, Vector3Field, Vector3> { }

    [NodeCustomEditor(typeof(Vector4Node))]
    public class Vector4NodeView : PrimitiveNodeView<Vector4Node, Vector4Field, Vector4> { }

    [NodeCustomEditor(typeof(ColorNode))]
    public class ColorNodeView : PrimitiveNodeView<ColorNode, ColorField, Color> { }


}