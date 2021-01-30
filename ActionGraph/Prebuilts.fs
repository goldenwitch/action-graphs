namespace ActionGraph

    open System
    open ActionGraph.Edges

    module Prebuilts =
        let private random = new System.Random()
        let private getRandomInt (min, max) = random.Next(min, max)

        let SystemEdges =
            seq{
                yield ("system.navigate",
                    ActionEdge(
                        function(_, _, _, _) -> ()
                    )
                );
                yield ("system.stay",
                    FunctionEdge(
                        function(valueNode, _, _, _) -> 
                                valueNode
                    )
                );
            }
        let ConsoleEdges =
            seq{
                yield ("console.text", //Only works on strings
                    ActionEdge(
                        function(fromNode : Node, _ , _, _) ->
                                GraphConversions.actOnGraphLikeAsString(fromNode.Value, fun b -> Console.WriteLine(b))
                    )
                );
                yield ("console.templateText", //Only works on strings
                    ActionEdge(
                        function(fromNode : Node, _, velocity, graph) ->
                                GraphConversions.actOnGraphLikeAsString(fromNode.Value, 
                                    fun a ->
                                        Console.WriteLine(GraphExpressions.ReplaceTemplates(a,fromNode,graph,velocity))
                                )
                            
                    )
                );
            }
        let SubgraphEdges =
            seq{
                yield ("subgraph.any",
                    FunctionEdge(
                        function(fromNode : Node, _, _, _) ->
                                match GraphConversions.collapseGraphLikeToGraph(fromNode.Value) with
                                    | Some a ->
                                        let randomNode = getRandomInt(0, a.Nodes.Count)
                                        a.WalkEdge(IntValue(randomNode), StringValue("any"))
                                    | None -> fromNode
                    ) 
                                
                );
                yield ("subgraph.query", //Only works on graphs of strings, uses velocity as id for nodes to walk
                    FunctionEdge(
                        function(fromNode : Node, _, velocity, _) ->
                                match GraphConversions.collapseGraphLikeToGraph(fromNode.Value) with
                                    | Some a -> 
                                        if a.Nodes.ContainsKey(velocity) then
                                            a.WalkEdge(velocity, StringValue("queryText"))
                                        else
                                            fromNode
                                    | None -> fromNode
                    )
                );
            }
