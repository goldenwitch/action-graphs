// Learn more about F# at http://fsharp.org

open System
open System.Linq;
open ActionGraph
open ActionGraph.Expressions
open ActionGraph.MathGraph
open ActionGraph.EchoGraph
open System.Threading.Tasks


let consoleLoop (graph, walker:EchoGraph.EchoWalker, args:string) =
    walker.Walk(graph, "Count.text="+args)
    //walker.Walk(graph, args)
    ()

let rec result(graph, walker) =
    let readValue = Console.ReadLine()
    if not(String.IsNullOrWhiteSpace readValue)
    then
        consoleLoop(graph, walker, readValue)
        result(graph, walker)
    else
        consoleLoop(graph, walker, readValue)

let timeLoop(graph, walker:EchoGraph.EchoWalker) =
    async {
        while(true) do
            Task.Delay(1000)
            |> Async.AwaitTask
            |> Async.RunSynchronously

            walker.Walk(graph, "add=1")
    }

[<EntryPoint>]
let main argv =
    let message = "Hello World"
    Console.WriteLine message

    let graph = EchoGraph.Definition
    GraphConversions.actOnGraphLikeAsGraph(graph, 
        fun a -> 
            let walker = EchoGraph.LoadWalker(a, a.Nodes.[StringValue "Time"])
            timeLoop(a,walker)
            |> Async.Start
    )
    GraphConversions.actOnGraphLikeAsGraph(graph, 
        fun a -> 
            let walker = EchoGraph.LoadWalker(a, a.Nodes.[StringValue "Start"])
            result(a, walker)
    )
    
    0 // return an integer exit code