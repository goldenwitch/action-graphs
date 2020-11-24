namespace ActionGraph.MathGraph
open System
open ActionGraph
open System.IO
open Expressions

module MathGraph =
    let HandleVelocity(vel, def) =
         let (|Integer|_|) (str: string) =
            let mutable intvalue = 0
            if System.Int32.TryParse(str, &intvalue) then Some(intvalue)
            else None
         let (|IntValue|_|) (vel: GraphValue) =
             match vel with
             | IntValue i -> Some(i)
             | _ -> None
         let (|StringValue|_|) (vel: GraphValue) =
             match vel with
             | StringValue s ->
                 match s with
                 | Integer i -> Some(i)
                 | _ -> None
             | _ -> None
         match vel with
            | IntValue i -> i
            | StringValue s -> s
            | _ -> def

    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("add", 
                    function(fromNode : Node<int>, toNode: Node<int>, velocity) -> 
                            //If velocity is appropriate for math, use that, else use 1
                            let trueVel = HandleVelocity(velocity, 1);
                            fromNode.Value <- fromNode.Value+trueVel
                            Console.WriteLine("Add walked with velocity "+trueVel.ToString()+", value at " + fromNode.Value.ToString())
                );
                yield ("subtract", 
                    function(fromNode : Node<int>, toNode: Node<int>, velocity) -> 
                            let trueVel = HandleVelocity(velocity, 1);
                            fromNode.Value <- fromNode.Value-trueVel
                            Console.WriteLine("Subtract walked with velocity "+trueVel.ToString()+", value at " + fromNode.Value.ToString())
                );
                yield ("multiply", 
                    function(fromNode : Node<int>, toNode: Node<int>, velocity) -> 
                            let trueVel = HandleVelocity(velocity, 1);
                            fromNode.Value <- fromNode.Value*trueVel
                            Console.WriteLine("Multiply walked with velocity "+trueVel.ToString()+", value at " + fromNode.Value.ToString())
                );
                yield ("assign", 
                    function(fromNode : Node<int>, toNode: Node<int>, velocity) -> 
                            let trueVel = HandleVelocity(velocity, 0);
                            fromNode.Value <- trueVel
                            Console.WriteLine("Assigned walked with velocity "+trueVel.ToString()+", value at " + fromNode.Value.ToString())
                );
            })
        let graphDefinition = ActionGraph.Load<int>(File.ReadAllText("mathGraph.json"), edgeFunctions)
        graphDefinition

    type MathWalker =
        {
            CurrentNode : Node<int>
        }

        member this.Walk(graph : Graph<int>, expression : string) =
            let baseWalker : SuperWalker<int> = 
                {
                    CurrentNode = this.CurrentNode
                    SuperEdges = 
                        Map.ofSeq(
                            seq{
                                yield ("Exponent", 
                                    function(input : Walker<int>, velocity) ->
                                            let trueVel = HandleVelocity(velocity, 1);
                                            if trueVel = 0 then
                                                input.Walk(graph, "assign=1")
                                            else if trueVel = 1 then
                                                ()
                                            else
                                                let expExpression = [| for i in 0 .. Math.Abs(trueVel-2) -> "multiply="+input.CurrentNode.Value.ToString() |]
                                                input.Walk(graph, String.concat(".") expExpression)
                                            Console.WriteLine("Exponent walked with velocity "+trueVel.ToString()+", value at " + input.CurrentNode.Value.ToString())
                                );
                            }
                        )
                }
            baseWalker.Walk(graph,expression)
            ()