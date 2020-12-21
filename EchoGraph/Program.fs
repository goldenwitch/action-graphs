// Learn more about F# at http://fsharp.org
open System
open ActionGraph
open ActionGraph.EchoSample
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
    printfn "Hello World from F#!"

    let graph = GraphConversions.collapseGraphLikeToGraph(EchoGraph.Definition)
    match graph with
    | Some g -> 
        let playerWalker = 
            {
                CurrentNode = g.Nodes.[StringValue "Start"]
            }
        result(g, playerWalker)
    | None -> ()

    0 // return an integer exit code
