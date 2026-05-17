using GraphProcessor;
using System;

namespace AM.Node.Math
{
    [Serializable, NodeMenuItem("Math/Time")]
    public class TimeNode : MathNode
    {
        [Output("Time")] public float Time;
        [Output("DeltaTime")] public float DeltaTime;

        public override string name => "Time";
    }
}
