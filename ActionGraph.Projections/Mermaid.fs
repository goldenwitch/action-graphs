namespace ActionGraph.Projections.Prebuilts

open System.Linq

open ActionGraph
open ActionGraph.Projections.ProjectionTypes
open ActionGraph.Projections
open System.IO


module Mermaid =
    let ReprojectTo(graph: Graph, direction) =
        let globalNodeName(node:Node) =
            let rec appendParent(nodeCursor, parents) =
                match nodeCursor.Parent.Value with
                | Some n -> n.Id.ToString() :: parents
                | None -> parents
            String.concat "." (appendParent(node, [node.Id.ToString()]));
        Reproject.Transform(
            graph, 
            seq{
                yield GraphRule(
                    function(iterGraph) ->
                            //If we are the top level graph, output a graph tag
                            if iterGraph.Nodes.Any(fun(nodepair) -> 
                                    match nodepair.Value.Parent.Value with
                                    | Some _ -> true
                                    | None _ -> false
                            ) then 
                                None
                            else
                                Some(StringOut("graph "+direction+";"))
                                
                );
                yield NodeRule(
                    function(node) ->
                            //Need to output a node if we don't have any edges
                            if node.Edges.Count = 0 then
                                Some(StringOut(globalNodeName(node)+";"))
                            else
                                None
                );
                yield EdgeRule(
                    function(node, edge) ->
                            //handle each type of edge
                            let output(edgedestination, edgeid) =
                                Some(StringOut(globalNodeName(node)+"-->|"+edgeid+"|"+edgedestination+";"))
                            match edge with
                            | Edge e -> 
                                match e.To with
                                | IntValue i ->  output(globalNodeName(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget("[#"+i.ToString()+"]"))), e.Id.ToString())
                                | _ -> output(globalNodeName(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget(e.To.ToString()))), e.Id.ToString())
                            | ConditionalEdge e -> 
                                match e.To with
                                | IntValue i ->  output(globalNodeName(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget("[#"+i.ToString()+"]"))), e.Id.ToString())
                                | _ -> output(globalNodeName(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget(e.To.ToString()))), e.Id.ToString())
                            | ExpressionEdge e ->
                                let walkedNode = GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget(e.Action.ToString()))
                                //If we follow the expression to ourself, assume that we are a smart edge walking our subgraph and output the edge multiple times
                                match node.Value with
                                | Graph g -> 
                                    if node = walkedNode then
                                        let moreedges = seq {
                                            for subgraphNode in g.Nodes do
                                                let subgraphnodeName = globalNodeName(subgraphNode.Value);
                                                yield globalNodeName(node)+"-->|"+e.Id.ToString()+"|"+subgraphnodeName+"{"+subgraphnodeName+"};"
                                        }
                                        Some(StringOut(String.concat "" moreedges))
                                    else
                                        output(globalNodeName(walkedNode), e.Id.ToString())
                                | _ -> output(globalNodeName(walkedNode), e.Id.ToString())
                                        
                                
                );
            }
        )
    let ProjectionToFile(projection:seq<Option<ProjectionOutput>>, outputFilePath:string) =
        //loop through projection and append all strings
        let fileName = outputFilePath
        let fileType = outputFilePath.Split(".").LastOrDefault()
        File.Delete(fileName);
        File.AppendAllLines(fileName, seq {
            if fileType = "md" then
                yield "```mermaid";
            for item in projection do
                match item with
                | Some i ->
                    match i with
                    | StringOut s -> yield s;
                    | _ -> ();
                | None -> ()
            if fileType = "md" then
                yield "```"
        })