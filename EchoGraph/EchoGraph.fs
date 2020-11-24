namespace ActionGraph.EchoGraph
open System
open System.IO
open ActionGraph
open ActionGraph.Expressions

module EchoGraph =
    let Definition<'T> =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("text",
                    function(fromNode : Node<'T>, toNode : Node<'T>, velocity) -> 
                            Console.WriteLine(fromNode.Value.ToString()+velocity.ToString())
                );
                yield ("templateText",
                    function(fromNode : Node<'T>, toNode : Node<'T>, velocity) -> 
                            let outputString = fromNode.Value.ToString().Replace("{system:velocity}", velocity.ToString())
                            Console.WriteLine(outputString)
                );
            })
        let graphDefinition = ActionGraph.Load<'T>(File.ReadAllText("echoGraph.json"), edgeFunctions)
        graphDefinition