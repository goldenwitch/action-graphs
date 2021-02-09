namespace ActionGraph.Projections

open ActionGraph
open ProjectionTypes

module Reproject =
    let rec Transform(graph: Graph, rules: seq<ProjectionRule>) =
        //output a sequence of projectionoutput
        seq {
            //loop through every part of the graph and try to execute all the rules that apply
            //Start with the graph itself, try graph rule
            for item in rules do
                match item with
                | GraphRule g -> yield g(graph);
                | _ -> ()

            //Next the nodes
            //for each node
            for node in graph.Nodes do
            //  try node rule
                for item in rules do
                    match item with
                    | NodeRule n -> yield n(node.Value);
                    | _ -> ()
            //  for each edge
                for edge in node.Value.Edges do
            //      try edge rule
                    for item in rules do
                        match item with
                        | EdgeRule e -> yield e(node.Value, edge.Value);
                        | _ -> ()
        }