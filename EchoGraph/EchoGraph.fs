namespace ActionGraph.EchoGraph
open System
open System.IO
open ActionGraph
open ActionGraph.Expressions

module EchoGraph =
    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("add",
                    function(fromNode : Node, _ : Node, velocity, source) -> 
                            let vel = GraphConversions.HandleVelocity(velocity, 1);
                            GraphConversions.actOnGraphLikeAsInt(fromNode.Value, fun a -> fromNode.Value <- GraphConversions.assignIntAsGraphLike(a+vel))
                            Console.WriteLine("Add walked with velocity "+vel.ToString()+", value at " + fromNode.Value.ToString())
                );
                yield ("text",
                    function(fromNode : Node, _ : Node, velocity, _) ->
                            GraphConversions.actOnNodeByName(fromNode.Value, StringValue ("text"), fun a -> GraphConversions.actOnGraphLikeAsString(a.Value, fun b -> Console.WriteLine(b+velocity.ToString())))
                );
                yield ("templateText",
                    function(fromNode : Node, _ : Node, velocity, source) ->
                            GraphConversions.actOnNodeByName(fromNode.Value, StringValue ("text"), 
                                fun a -> 
                                    GraphConversions.actOnGraphLikeAsString(a.Value, 
                                        fun b ->
                                            if b.Contains("{node:count}") then
                                                GraphConversions.actOnNodeByName(Graph(source), StringValue ("Count"), fun c ->
                                                    GraphConversions.actOnGraphLikeAsInt(c.Value, fun d ->
                                                            let output = b.Replace("{system:velocity}", velocity.ToString()).Replace("{node:count}",d.ToString())
                                                            Console.WriteLine(output)
                                                    )
                                                )
                                            else
                                                let output = b.Replace("{system:velocity}", velocity.ToString())
                                                Console.WriteLine(output)
                                    )
                            )
                );
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("echoGraph.json"), edgeFunctions)
        graphDefinition

    type EchoWalker =
        {
            BaseWalker : SuperWalker
        }

        member this.Walk(graph : Graph, expression : string) =
            this.BaseWalker.Walk(graph,expression)
            ()

    let LoadWalker(graph, currentNode) =
        {
            BaseWalker = {
                BaseWalker =
                    {
                        CurrentNode = currentNode
                    }
                SuperEdges = 
                    Map.ofSeq(
                        seq{
                            yield ("Count", 
                                function(input : Walker, _) ->
                                        //Save current node
                                        let currentNode = input.CurrentNode
                                        //teleport to count
                                        input.CurrentNode <- graph.Nodes.[StringValue "Count"]
                                        //walk count add=1
                                        input.Walk(graph, "add=1")
                                        //teleport back to current node
                                        input.CurrentNode <- currentNode
                                )
                        }
                    )
            }
        }