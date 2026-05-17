using GraphProcessor;

namespace AM.Node.Math
{
    public class NumericTypeAdapter : ITypeAdapter
    {
        // int - float
        public static float ConvertIntToFloat(int value) => (float)value;
        public static int ConvertFloatToInt(float value) => (int)value;

        // int - double
        public static double ConvertIntToDouble(int value) => (double)value;
        public static int ConvertDoubleToInt(double value) => (int)value;

        // float - double
        public static double ConvertFloatToDouble(float value) => (double)value;
        public static float ConvertDoubleToFloat(double value) => (float)value;

        public override System.Collections.Generic.IEnumerable<(System.Type, System.Type)> GetIncompatibleTypes()
            => System.Array.Empty<(System.Type, System.Type)>();
    }
}