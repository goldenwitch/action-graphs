namespace ActionGraph.EchoSample
open System
open System.IO
open ActionGraph
open ActionGraph.Expressions
open System.Linq

module EchoGraph =
    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("add", //Only works on ints
                    ActionEdge(
                        function(fromNode : Node, _ , velocity, _) -> 
                                let vel = GraphConversions.HandleVelocity(velocity, 1);
                                GraphConversions.actOnGraphLikeAsInt(fromNode.Value, fun a -> fromNode.Value <- GraphConversions.assignIntAsGraphLike(a+vel))
                    )
                );
                yield ("text", //Only works on strings
                    ActionEdge(
                        function(fromNode : Node, _ , _, _) ->
                                GraphConversions.actOnGraphLikeAsString(fromNode.Value, fun b -> Console.WriteLine(b))
                    )
                );
                yield ("templateText", //Only works on strings
                    ActionEdge(
                        function(fromNode : Node, _, velocity, graph) ->
                                GraphConversions.actOnGraphLikeAsString(fromNode.Value, 
                                    fun a ->
                                        Console.WriteLine(GraphExpressions.ReplaceTemplates(a,fromNode,graph,velocity))
                                )
                                
                    )
                );
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("echoGraph.json"), edgeFunctions)
        graphDefinition