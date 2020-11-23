namespace Graph
open System
open Graph
open System.IO
open Expressions

module TextAdventureGraph =
    let Definition<'T> =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("text",
                    function(input : Node<'T>, output : Node<'T>, velocity) -> 
                            Console.WriteLine(output.Value.ToString())
                );
            })
        let graphDefinition = ActionGraph.Load<'T>(File.ReadAllText("textAdventureGraph.json"), edgeFunctions)
        graphDefinition

    type TextAdventureWalker<'T> =
        {
            CurrentNode : Node<'T>
        }

        member this.Walk(graph : Graph<'T>, expression : string) =
            let baseWalker : SuperWalker<'T> = 
                {
                    CurrentNode = this.CurrentNode
                    SuperEdges = 
                        Map.ofSeq(
                            seq{
                                yield ("Exponent", 
                                    function(input : Walker<'T>, velocity) ->
                                            Console.WriteLine("Exponent walked with velocity ")
                                );
                            }
                        )
                }
            baseWalker.Walk(graph,expression)
            ()