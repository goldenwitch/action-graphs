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
        /// <param name="writer"></param>
        private static void TransformJsonGraph(JToken graph, StreamWriter writer)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            // ---

            foreach (var node in graph["Nodes"].AsJEnumerable())
            {
                var nodeId = node["Id"].ToString();

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
                    writer.WriteLine(edge["To"].ToString());
                }

                if (node["Value"].Type == JTokenType.Object)
                {
                    TransformJsonGraph(node["Value"], writer);
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
