// Learn more about F# at http://fsharp.org

open System
open ActionGraph
open ActionGraph.TextAdventureSample
open ActionGraph.Expressions

let consoleLoop (graph, walker:Walker, args:string) =
    walker.Walk(graph, "text="+args)
    ()

let rec result(graph, walker) =
    let readValue = Console.ReadLine()
    consoleLoop(graph, walker, readValue)
    result(graph, walker)

[<EntryPoint>]
let main argv =
    let graph = RpgGraph.Definition
    match graph with
    | Some g -> 
        let playerWalker = 
            {
                CurrentNode = g.Nodes.[StringValue "Start"]
            }
        playerWalker.Walk(g, "text")
        result(g, playerWalker)
    | None -> ()

    0 // return an integer exit code
