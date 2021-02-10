namespace ActionGraph.Projections.Prebuilts

open System.Linq

open ActionGraph
open ActionGraph.Projections.ProjectionTypes
open ActionGraph.Projections
open System.IO

//Mermaid is slightly broken as of 8.8.4 and won't correctly render the edges. It gives up somewhere in there.
module Mermaid =
    let ReprojectTo(graph: Graph, direction) =
        let quotes(value) = "\""+value+"\""
        let globalNodeName(node:Node) =
            let rec appendParent(nodeCursor, parents) =
                match nodeCursor.Parent.Value with
                | Some n -> n.Id.ToString() :: parents
                | None -> parents
            String.concat "." (appendParent(node, [node.Id.ToString()]));
        let rec rules = seq{
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
                            Some(StringOut("flowchart "+direction+";"))
                            
            );
            yield NodeRule(
                function(node) ->
                        //Always output the node, we can make the edges later
                        match node.Value with
                        | Graph g ->
                        //Recurse if we have a subgraph
                            let name = globalNodeName(node);
                            Some(SeqOut(seq {
                                yield Some(StringOut("subgraph "+quotes(name)));
                                let subgraph = Reproject.Transform(g, rules);
                                yield! subgraph;
                                yield Some(StringOut("end"));
                            }))
                        | _ -> Some(StringOut(globalNodeName(node)+";"))
                        
            );
            yield EdgeRule(
                function(node, edge) ->
                        //handle each type of edge
                        let output(edgedestination, edgeid) =
                            Some(
                                EndStringOut(globalNodeName(node)+"-->|"+edgeid+"|"+globalNodeName(edgedestination)+";")
                            )
                        let toGraphOutput(edgedestination, graphid, edgeid) =
                             //If our edge goes to a graph node, we should instead write an edge to the cluster
                            Some(
                                EndStringOut(quotes(globalNodeName(node))+"->"+quotes(globalNodeName(edgedestination))+" [lhead="+quotes("cluster"+globalNodeName(graphid))+", taillabel=\""+edgeid+"\"];")
                            )
                        let handleGraphNode(toNode) =
                            match toNode.Value with
                            //| Graph g ->
                            //    toGraphOutput(g.Nodes.First().Value, toNode, edge.Id.ToString())
                            | _ -> output(toNode, edge.Id.ToString())
                        match edge with
                        | Edge e -> 
                            match e.To with
                            | IntValue i ->  handleGraphNode(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget("[#"+i.ToString()+"]")))
                            | _ -> handleGraphNode(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget(e.To.ToString())))
                        | ConditionalEdge e ->
                            match e.To with
                            | IntValue i ->  handleGraphNode(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget("[#"+i.ToString()+"]")))
                            | _ -> handleGraphNode(GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget(e.To.ToString())))
                        | ExpressionEdge e ->
                            let walkedNode = GraphExpressions.WalkTargetExpression(node, GraphExpressions.ParseTarget(e.Action.ToString()))
                            match walkedNode.Value with
                            | Graph g ->
                                if walkedNode = node then
                                    None
                                else
                                    handleGraphNode(walkedNode)
                            | _ -> handleGraphNode(walkedNode)
            );
        }
        Reproject.Transform(
            graph, 
            rules
        )
    let ProjectionToStrings(projection:seq<Option<ProjectionOutput>>) =
        let mutable wrapperList = []
        seq {
            let top = seq {
                let rec handleProjection(inputProj) =
                    seq {
                        for item in inputProj do
                            match item with
                            | Some i ->
                                match i with
                                | StringOut s -> yield s;
                                | SeqOut s ->
                                    yield! handleProjection(s)
                                | EndStringOut s -> wrapperList <- s :: wrapperList
                                | WrapperOut (h, t) ->
                                    match h with
                                    | Some outValue ->
                                        match outValue with
                                        | StringOut outString -> yield outString;
                                        | _ -> ()
                                    | None -> ()
                                    match t with
                                    | Some outValue ->
                                        match outValue with
                                        | StringOut outString -> wrapperList <- outString :: wrapperList
                                        | _ -> ()
                                    | None -> ()
                            | None -> ()
                    }
                yield! handleProjection(projection)
            }
            yield! top;
            let bottom = Seq.ofList(wrapperList);
            yield! bottom;
        }
    let ProjectionToFile(projection:seq<Option<ProjectionOutput>>, outputFilePath:string) =
        //loop through projection and append all strings
        let fileName = outputFilePath
        let fileType = outputFilePath.Split(".").LastOrDefault()
        File.Delete(fileName);
        let fileSeq = seq {
            if fileType = "md" then
                yield "```mermaid";
            yield! ProjectionToStrings(projection);
            if fileType = "md" then
                yield "```"
        }
        File.AppendAllLines(fileName, fileSeq)