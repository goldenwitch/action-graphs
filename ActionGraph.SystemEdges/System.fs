namespace ActionGraph.SystemEdges
open ActionGraph
open System

module SystemEdges =
    let Edges =
        Map.ofSeq(seq{
            yield ("navigate",
                ActionEdge(
                    function(valueNode, _, _, graph) -> 
                            GraphConversions.actOnGraphLikeAsString(valueNode.Value, 
                                fun a -> Console.WriteLine("It is now "+a)
                            )
                            
                )
            );
            yield ("event",
                FunctionEdge(
                    function(valueNode, _, _, _) -> 
                            valueNode
                        
                )
            );
        })
