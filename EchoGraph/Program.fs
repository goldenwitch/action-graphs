// Learn more about F# at http://fsharp.org
open System
open ActionGraph
open ActionGraph.EchoSample
open ActionGraph.Expressions

//Our core loop that runs every time we send a console message.
let consoleLoop (graph, walker:Walker, userMessage:String) =
    walker.Walk(graph, "text="+userMessage) //Walk our graph, following the "text" edge each time
    ()

let rec result(graph, walker) =
    let userMessage = Console.ReadLine()
    consoleLoop(graph, walker, userMessage) //Run our console loop
    result(graph, walker)

[<EntryPoint>]
let main argv =
    Console.WriteLine("Press enter or say anything to navigate between the nodes and read their value out.")

    let graphDefinition = EchoGraph.Definition
    match graphDefinition with //Here we are handling the case in which we don't succeed at loading our graph
    | Some g ->
        let playerWalker = //Create a "walker" which points to a specific place in the graph and follows the edges between nodes.
            {
                CurrentNode = g.Nodes.[StringValue "Start"]
            }
        result(g, playerWalker) //Start the console loop, passing in the walker to go walk the graph
    | None -> ()

    0 // return an integer exit code