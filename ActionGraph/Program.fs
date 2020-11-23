// Learn more about F# at http://fsharp.org

open System
open Graph
open Graph.Expressions
open Graph.MathGraph

let consoleLoop (args:string) =
    let walker =
        {
            CurrentNode = MathGraph.Definition.Nodes.[StringValue("Math")]
        }
    walker.Walk(MathGraph.Definition, "add=2.Exponent=1.add.add.multiply=5")
    ()

let rec result() =
    let readValue = Console.ReadLine()
    if not(String.IsNullOrWhiteSpace readValue)
    then
        consoleLoop readValue
        result()
    else
        consoleLoop readValue

[<EntryPoint>]
let main argv =
    let message = "Hello Autumn"
    Console.WriteLine message

    result()
    0 // return an integer exit code