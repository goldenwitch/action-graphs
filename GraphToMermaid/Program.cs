using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GraphToMermaid
{
    class Program
    {
        static void Main(string[] filePaths)
        {
            foreach (var inputPath in filePaths)
            {
                var outputPath = GetOutputPath(inputPath);
                
                var graph = JObject.Parse(File.ReadAllText(inputPath));

                using (var writer = File.CreateText(outputPath))
                {
                    writer.WriteLine("graph TD");
                    TransformJsonGraph(graph, writer);
                }
            }
        }

        /// <summary>
        /// Transform a given json graph in Autumn format, and writes the mermaid equivalent.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="parentNodeName">The parent all nodes should be tied to.</param>
        private static void TransformJsonGraph(JToken graph, StreamWriter writer, string parentNodeName = null)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            // ---

            foreach (var node in graph["Nodes"].AsJEnumerable())
            {
                var nodeId = parentNodeName == null
                    ? node["Id"].ToString()
                    : parentNodeName + node["Id"].ToString();

                if (parentNodeName != null)
                {
                    writer.Write("    ");
                    writer.Write(parentNodeName);
                    writer.Write(" --> ");
                    writer.WriteLine(nodeId);
                    writer.WriteLine($"    style {nodeId} fill:#f9f,stroke:#333,stroke-width:4px;");
                }

                foreach (var edge in node["Edges"].AsJEnumerable())
                {
                    var to = edge["To"].ToString();

                    writer.Write("    ");
                    writer.Write(nodeId);
                    writer.Write(" -->|");

                    writer.Write("id:");
                    writer.Write(edge["Id"].ToString());
                    writer.Write(", action:");
                    writer.Write(edge["Action"].ToString());

                    writer.Write("| ");
                    writer.WriteLine(parentNodeName + edge["To"].ToString());
                }

                if (node["Value"].Type == JTokenType.Object)
                {
                    string valueGraphName = nodeId + "-VG";
                    writer.Write("    ");
                    writer.Write(nodeId);
                    writer.Write(" --> ");
                    writer.WriteLine(valueGraphName);
                    writer.WriteLine($"    style {valueGraphName} fill:#f9f,stroke:#333,stroke-width:4px;");
                    TransformJsonGraph(node["Value"], writer, valueGraphName);
                }
            }
        }

        /// <summary>
        /// Given "samplefile.xyz" returns "samplefile.mermaid". Given "samplefile" returns "samplefile.mermaid".
        /// </summary>
        /// <param name="inputPath">Any non-null string.</param>
        /// <returns>The correlated string.</returns>
        private static string GetOutputPath(string inputPath)
        {
            if (inputPath == null)
                throw new ArgumentNullException(nameof(inputPath));
            // ---

            var indexOfLastDot = inputPath.LastIndexOf('.');

            if (indexOfLastDot < 0)
            {
                return inputPath + ".mermaid";
            }
            else
            {
                var firstPart = inputPath.Substring(0, indexOfLastDot);
                return firstPart + ".mermaid";
            }
        }
    }
}
