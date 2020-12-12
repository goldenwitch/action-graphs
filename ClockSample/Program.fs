// Learn more about F# at http://fsharp.org

open System
open ActionGraph
open ClockSample
open ActionGraph.Expressions

let consoleLoop (graph, walker:Walker, args:string) =
    walker.Walk(graph, args)
    //walker.Walk(graph, args)
    ()

let rec result(graph, walker) =
    let readValue = Console.ReadLine()
    //if not(String.IsNullOrWhiteSpace readValue)
    //then
    consoleLoop(graph, walker, readValue)
    result(graph, walker)
    //else
    //    consoleLoop(graph, walker, readValue)

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    let graph = GraphConversions.collapseGraphLikeToGraph(ClockGraph.Definition)
    match graph with
    | Some g -> 
        let playerWalker = 
            {
                CurrentNode = g.Nodes.[StringValue "Clock"]
            }
        result(g, playerWalker)
    | None -> ()

    0 // return an integer exit code
