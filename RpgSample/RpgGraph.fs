namespace ActionGraph.TextAdventureSample

open ActionGraph.Expressions
open ActionGraph
open System.IO

module RpgGraph =
    let LoadWalker(graph, currentNode, travelInterval) =
        {
            CurrentNode = currentNode
        }

    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield! Prebuilts.ConsoleEdges;
                yield! Prebuilts.SubgraphEdges;
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("rpgGraph.json"), edgeFunctions)
        graphDefinition
