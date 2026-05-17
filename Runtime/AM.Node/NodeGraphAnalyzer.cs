#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GraphProcessor;
using AM.Node.Data;
using AM.Node.Math;
using AM.Node.Math.Operators;
using AM.Node.Math.Primitives;
using AM.Node.Pipelines;
using UnityEditor;
using UnityEngine;

namespace AM.Node.Editor
{
    public class NodeGraphAnalyzer : EditorWindow
    {
        private BaseGraph targetGraph;
        private Vector2 scrollPosition;
        private string reportText = string.Empty;
        private AnalysisReport lastReport;
        private bool showNodeDetails = true;
        private bool showPortDetails = true;
        private bool showConnectionDetails = true;
        private bool showWarnings = true;

        [MenuItem("AM/Node Graph Analyzer")]
        public static void Open()
        {
            var window = GetWindow<NodeGraphAnalyzer>("Node Graph Analyzer");
            window.minSize = new Vector2(600f, 500f);
            window.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawGraphSelector();

            if (targetGraph == null)
            {
                EditorGUILayout.HelpBox("Assign a BaseGraph to analyze.", MessageType.Info);
                return;
            }

            DrawOptions();
            DrawAnalyzeButton();

            if (!string.IsNullOrEmpty(reportText))
                DrawReport();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("AM Node Graph Analyzer", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Copy Report", EditorStyles.toolbarButton, GUILayout.Width(90f)))
            {
                if (!string.IsNullOrEmpty(reportText))
                    EditorGUIUtility.systemCopyBuffer = reportText;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGraphSelector()
        {
            EditorGUI.BeginChangeCheck();
            targetGraph = (BaseGraph)EditorGUILayout.ObjectField(
                "Target Graph", targetGraph, typeof(BaseGraph), false);

            if (EditorGUI.EndChangeCheck())
                reportText = string.Empty;
        }

        private void DrawOptions()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Analysis Options", EditorStyles.boldLabel);

            showNodeDetails = EditorGUILayout.Toggle("Node Details", showNodeDetails);
            showPortDetails = EditorGUILayout.Toggle("Port Details", showPortDetails);
            showConnectionDetails = EditorGUILayout.Toggle("Connection Details", showConnectionDetails);
            showWarnings = EditorGUILayout.Toggle("Warnings", showWarnings);
        }

        private void DrawAnalyzeButton()
        {
            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Analyze", GUILayout.Height(32f)))
            {
                lastReport = Analyze(targetGraph);
                reportText = BuildReportText(lastReport);
            }
        }

        private void DrawReport()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Report", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(reportText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        public static AnalysisReport Analyze(BaseGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var report = new AnalysisReport
            {
                GraphName = graph.name
            };

            var nodes = graph.nodes;

            foreach (var node in nodes)
            {
                if (node == null) continue;

                var nodeInfo = BuildNodeInfo(node);
                report.Nodes.Add(nodeInfo);

                CategorizeNode(node, report);
            }

            report.TotalNodeCount = report.Nodes.Count;
            report.TotalEdgeCount = graph.edges.Count;

            AnalyzeConnections(graph, report);
            DetectWarnings(graph, report);

            return report;
        }

        private static NodeInfo BuildNodeInfo(BaseNode node)
        {
            var type = node.GetType();
            var info = new NodeInfo
            {
                NodeName = node.name,
                TypeName = type.Name,
                FullTypeName = type.FullName,
                Namespace = type.Namespace ?? string.Empty,
                Category = ResolveCategory(node),
                BaseChain = BuildBaseChain(type)
            };

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                var inputAttr = field.GetCustomAttribute<InputAttribute>();
                if (inputAttr != null)
                {
                    info.InputPorts.Add(new PortInfo
                    {
                        PortName = inputAttr.name ?? field.Name,
                        FieldName = field.Name,
                        TypeName = field.FieldType.Name
                    });
                }

                var outputAttr = field.GetCustomAttribute<OutputAttribute>();
                if (outputAttr != null)
                {
                    info.OutputPorts.Add(new PortInfo
                    {
                        PortName = outputAttr.name ?? field.Name,
                        FieldName = field.Name,
                        TypeName = field.FieldType.Name
                    });
                }
            }

            if (type.IsGenericType)
            {
                info.GenericArguments = type.GetGenericArguments()
                    .Select(a => a.Name)
                    .ToList();
            }
            else if (type.BaseType != null && type.BaseType.IsGenericType)
            {
                info.GenericArguments = type.BaseType.GetGenericArguments()
                    .Select(a => a.Name)
                    .ToList();
            }

            return info;
        }

        private static void CategorizeNode(BaseNode node, AnalysisReport report)
        {
            switch (node)
            {
                case DataNode:
                    report.DataNodeCount++;
                    break;
                case OperatorNode<float> or OperatorNode<bool>:
                    report.OperatorNodeCount++;
                    break;
                case PrimitiveNode<float> or PrimitiveNode<int> or PrimitiveNode<bool>
                    or PrimitiveNode<UnityEngine.Vector2> or PrimitiveNode<UnityEngine.Vector3>
                    or PrimitiveNode<UnityEngine.Vector4> or PrimitiveNode<UnityEngine.Color>:
                    report.PrimitiveNodeCount++;
                    break;
                case MathNode:
                    report.MathNodeCount++;
                    break;
                case PipelineNode:
                    report.PipelineNodeCount++;
                    break;
            }
        }

        private static void AnalyzeConnections(BaseGraph graph, AnalysisReport report)
        {
            var connectionsByNode = new Dictionary<string, int>();

            foreach (var edge in graph.edges)
            {
                if (edge == null) continue;

                var conn = new ConnectionInfo
                {
                    OutputNodeName = edge.outputNode?.name ?? "Unknown",
                    OutputPortName = edge.outputFieldName,
                    InputNodeName = edge.inputNode?.name ?? "Unknown",
                    InputPortName = edge.inputFieldName,
                    IsTypeAdapted = edge.passThroughBuffer != null
                };
                report.Connections.Add(conn);

                CountConnection(connectionsByNode, conn.OutputNodeName);
                CountConnection(connectionsByNode, conn.InputNodeName);
            }

            if (connectionsByNode.Count > 0)
            {
                report.MostConnectedNode = connectionsByNode.OrderByDescending(kv => kv.Value).First().Key;
                report.MaxConnectionCount = connectionsByNode.Values.Max();
            }
        }

        private static void CountConnection(Dictionary<string, int> dict, string key)
        {
            if (!dict.TryGetValue(key, out _))
                dict[key] = 0;
            dict[key]++;
        }

        private static void DetectWarnings(BaseGraph graph, AnalysisReport report)
        {
            var connectedNodeGuids = new HashSet<string>();

            foreach (var edge in graph.edges)
            {
                if (edge == null) continue;
                if (edge.outputNode != null) connectedNodeGuids.Add(edge.outputNode.GUID);
                if (edge.inputNode != null) connectedNodeGuids.Add(edge.inputNode.GUID);
            }

            foreach (var node in graph.nodes)
            {
                if (node == null) continue;

                if (!connectedNodeGuids.Contains(node.GUID) && !(node is EntryNode))
                    report.Warnings.Add($"Isolated node detected: '{node.name}' ({node.GetType().Name})");

                if (node is PipelineNode && !(node is EntryNode))
                {
                    var fields = node.GetType().GetFields(
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                    bool hasInput = fields.Any(f => f.GetCustomAttribute<InputAttribute>() != null
                        && f.FieldType == typeof(PipelineFlow));

                    if (hasInput && !connectedNodeGuids.Contains(node.GUID))
                        report.Warnings.Add($"PipelineNode '{node.name}' has no incoming PipelineFlow connection.");
                }
            }

            var entryNodes = graph.nodes.OfType<EntryNode>().ToList();
            if (entryNodes.Count == 0)
                report.Warnings.Add("No EntryNode found in graph.");
            else if (entryNodes.Count > 1)
                report.Warnings.Add($"Multiple EntryNodes found ({entryNodes.Count}). Consider consolidating.");
        }

        private static string ResolveCategory(BaseNode node)
        {
            return node switch
            {
                DataNode => "Data",
                OperatorNode<float> => "Math/Operator",
                OperatorNode<bool> => "Math/Operator",
                PrimitiveNode<float> or PrimitiveNode<int> or PrimitiveNode<bool>
                    or PrimitiveNode<UnityEngine.Vector2> or PrimitiveNode<UnityEngine.Vector3>
                    or PrimitiveNode<UnityEngine.Vector4> or PrimitiveNode<UnityEngine.Color> => "Math/Primitive",
                MathNode => "Math",
                PipelineNode => "Pipeline",
                _ => "Unknown"
            };
        }

        private static List<string> BuildBaseChain(Type type)
        {
            var chain = new List<string>();
            var current = type.BaseType;

            while (current != null && current != typeof(object))
            {
                chain.Add(current.IsGenericType
                    ? $"{current.Name.Split('`')[0]}<{string.Join(", ", current.GetGenericArguments().Select(a => a.Name))}>"
                    : current.Name);
                current = current.BaseType;
            }

            return chain;
        }

        private string BuildReportText(AnalysisReport report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("╔══════════════════════════════════════════════════╗");
            sb.AppendLine($"  AM Node Graph Analyzer - {report.GraphName}");
            sb.AppendLine("╚══════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("─── Summary ───────────────────────────────────────");
            sb.AppendLine($"Total Nodes     : {report.TotalNodeCount}");
            sb.AppendLine($"Total Edges     : {report.TotalEdgeCount}");
            sb.AppendLine($"  Data Nodes    : {report.DataNodeCount}");
            sb.AppendLine($"  Math Nodes    : {report.MathNodeCount}");
            sb.AppendLine($"  Operator Nodes: {report.OperatorNodeCount}");
            sb.AppendLine($"  Primitive Nodes: {report.PrimitiveNodeCount}");
            sb.AppendLine($"  Pipeline Nodes: {report.PipelineNodeCount}");

            if (!string.IsNullOrEmpty(report.MostConnectedNode))
                sb.AppendLine($"Most Connected  : '{report.MostConnectedNode}' ({report.MaxConnectionCount} connections)");

            sb.AppendLine();

            if (showWarnings && report.Warnings.Count > 0)
            {
                sb.AppendLine("─── Warnings ──────────────────────────────────────");
                foreach (var w in report.Warnings)
                    sb.AppendLine($"  ⚠ {w}");
                sb.AppendLine();
            }

            if (showNodeDetails)
            {
                sb.AppendLine("─── Nodes ─────────────────────────────────────────");
                foreach (var node in report.Nodes)
                {
                    sb.AppendLine($"  [{node.Category}] {node.NodeName} ({node.TypeName})");

                    if (node.GenericArguments.Count > 0)
                        sb.AppendLine($"    Generic Args  : <{string.Join(", ", node.GenericArguments)}>");

                    if (node.BaseChain.Count > 0)
                        sb.AppendLine($"    Base Chain    : {string.Join(" → ", node.BaseChain)}");

                    if (showPortDetails)
                    {
                        if (node.InputPorts.Count > 0)
                        {
                            sb.AppendLine($"    Input Ports ({node.InputPorts.Count}):");
                            foreach (var p in node.InputPorts)
                                sb.AppendLine($"      ← [{p.TypeName}] {p.PortName}");
                        }

                        if (node.OutputPorts.Count > 0)
                        {
                            sb.AppendLine($"    Output Ports ({node.OutputPorts.Count}):");
                            foreach (var p in node.OutputPorts)
                                sb.AppendLine($"      → [{p.TypeName}] {p.PortName}");
                        }
                    }
                }

                sb.AppendLine();
            }

            if (showConnectionDetails && report.Connections.Count > 0)
            {
                sb.AppendLine("─── Connections ───────────────────────────────────");
                foreach (var c in report.Connections)
                {
                    var adapted = c.IsTypeAdapted ? " [adapted]" : string.Empty;
                    sb.AppendLine($"  {c.OutputNodeName}.{c.OutputPortName} → {c.InputNodeName}.{c.InputPortName}{adapted}");
                }

                sb.AppendLine();
            }

            sb.AppendLine("───────────────────────────────────────────────────");
            sb.AppendLine("End of Report");

            return sb.ToString();
        }
    }

    public sealed class AnalysisReport
    {
        public string GraphName = string.Empty;
        public int TotalNodeCount;
        public int TotalEdgeCount;
        public int DataNodeCount;
        public int MathNodeCount;
        public int OperatorNodeCount;
        public int PrimitiveNodeCount;
        public int PipelineNodeCount;
        public string MostConnectedNode = string.Empty;
        public int MaxConnectionCount;

        public List<NodeInfo> Nodes = new();
        public List<ConnectionInfo> Connections = new();
        public List<string> Warnings = new();
    }

    public sealed class NodeInfo
    {
        public string NodeName = string.Empty;
        public string TypeName = string.Empty;
        public string FullTypeName = string.Empty;
        public string Namespace = string.Empty;
        public string Category = string.Empty;
        public List<string> BaseChain = new();
        public List<string> GenericArguments = new();
        public List<PortInfo> InputPorts = new();
        public List<PortInfo> OutputPorts = new();
    }

    public sealed class PortInfo
    {
        public string PortName = string.Empty;
        public string FieldName = string.Empty;
        public string TypeName = string.Empty;
    }

    public sealed class ConnectionInfo
    {
        public string OutputNodeName = string.Empty;
        public string OutputPortName = string.Empty;
        public string InputNodeName = string.Empty;
        public string InputPortName = string.Empty;
        public bool IsTypeAdapted;
    }
}
#endif
