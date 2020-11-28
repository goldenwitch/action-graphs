namespace ActionGraph.MathGraph
open System
open ActionGraph
open System.IO
open Expressions

module MathGraph =
    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("add", 
                    function(fromNode : Node, toNode: Node, velocity, _) -> 
                            //If velocity is appropriate for math, use that, else use 1
                            let vel = GraphConversions.HandleVelocity(velocity, 1);
                            GraphConversions.actOnGraphLikeAsInt(fromNode.Value, fun a -> fromNode.Value <- GraphConversions.assignIntAsGraphLike(a+vel))
                            Console.WriteLine("Add walked with velocity "+vel.ToString()+", value at " + fromNode.Value.ToString())
                );
                yield ("subtract", 
                    function(fromNode : Node, toNode: Node, velocity, _) -> 
                            let vel = GraphConversions.HandleVelocity(velocity, 1);
                            GraphConversions.actOnGraphLikeAsInt(fromNode.Value, fun a -> fromNode.Value <- GraphConversions.assignIntAsGraphLike(a-vel))
                            Console.WriteLine("Subtract walked with velocity "+vel.ToString()+", value at " + fromNode.Value.ToString())
                );
                yield ("multiply", 
                    function(fromNode : Node, toNode: Node, velocity, _) -> 
                            let vel = GraphConversions.HandleVelocity(velocity, 1);
                            GraphConversions.actOnGraphLikeAsInt(fromNode.Value, fun a -> fromNode.Value <- GraphConversions.assignIntAsGraphLike(a*vel))
                            Console.WriteLine("Multiply walked with velocity "+vel.ToString()+", value at " + fromNode.Value.ToString())
                );
                yield ("assign", 
                    function(fromNode : Node, toNode: Node, velocity, _) -> 
                            let vel = GraphConversions.HandleVelocity(velocity, 1);
                            GraphConversions.actOnGraphLikeAsInt(fromNode.Value, fun a -> fromNode.Value <- GraphConversions.assignIntAsGraphLike(vel))
                            Console.WriteLine("Assigned walked with velocity "+vel.ToString()+", value at " + fromNode.Value.ToString())
                );
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("mathGraph.json"), edgeFunctions)
        graphDefinition

    type MathWalker =
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
                                yield ("Exponent", 
                                    function(input : Walker, velocity) ->
                                            let trueVel = GraphConversions.HandleVelocity(velocity, 1);
                                            if trueVel = 0 then
                                                input.Walk(graph, "assign=1")
                                            else if trueVel = 1 then
                                                ()
                                            else
                                                GraphConversions.actOnGraphLikeAsInt(input.CurrentNode.Value, 
                                                    fun a ->
                                                        let expExpression = [| for i in 0 .. Math.Abs(trueVel-2) -> "multiply="+a.ToString() |]
                                                        input.Walk(graph, String.concat(".") expExpression)
                                                        Console.WriteLine("Exponent walked with velocity "+trueVel.ToString()+", value at " + input.CurrentNode.Value.ToString())
                                                )
                                );
                            }
                        )
                }
            }


