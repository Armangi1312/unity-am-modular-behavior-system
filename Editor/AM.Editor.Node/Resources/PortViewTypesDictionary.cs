using AM.Node.Pipelines;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AM.Editor.Node.Resources
{
    public static class PortViewTypesDictionary
    {
        private static readonly Dictionary<Type, Color> colorMap = new()
        {
            { typeof(int),          Hex("#84E4E7") },
            { typeof(float),        Hex("#84E4E7") },
            { typeof(bool),         Hex("#8F7DDD") },
            { typeof(Vector2),      Hex("#96E98E") },
            { typeof(Vector3),      Hex("#ECF494") },
            { typeof(Vector4),      Hex("#F4C6ED") },
            { typeof(object),       Hex("#E08282") },
            { typeof(Color),        Hex("#F4C6ED") },
            { typeof(PipelineFlow), Hex("#8AD8A2") },
        };

        public static Color Get(Type type) =>
            colorMap.TryGetValue(type, out var c) ? c : Color.white;

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }
    }
}
