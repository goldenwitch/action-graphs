// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Text

open ActionGraph.Projections.Prebuilts
open ActionGraph


[<EntryPoint>]
let main argv =
    let noMatch() =
        Console.WriteLine("Not sure what you want. Use argument 'help' to find out what I can do.")
    let help() =
        Console.WriteLine("mermaid sourceFilePath [outputFilePath, documentOrder]")
        Console.WriteLine("    ex. mermaid dog.json dog.md BT")
        Console.WriteLine("    documentOrder options (see https://mermaid-js.github.io/mermaid/#/flowchart?id=flowchart-orientation)")
        Console.WriteLine("        TD,BT,LR,RL")
    let mermaid(filePath, outputFilePath, documentOrder) =
        //Get graph from file (no edges)
        let graph = ActionGraph.Load(File.ReadAllText(filePath), Map.empty)
        Mermaid.ProjectionToFile(Mermaid.ReprojectTo(graph.Value, documentOrder), outputFilePath)
        Console.WriteLine("Projecting mermaid from '"+filePath+"' with outputFilePath '"+outputFilePath+"' and documentOrder '"+documentOrder+"'")

    if(argv.Length = 0) then
        noMatch()
    else
        match argv.[0] with
        | "mermaid" -> 
            match argv.Length with
            | 2 -> mermaid(argv.[1], "", "TD")
            | 3 -> mermaid(argv.[1], argv.[2], "TD")
            | 4 -> mermaid(argv.[1], argv.[2], argv.[3])
            | _ -> noMatch()
        | "help" -> help()
        | _ -> noMatch()

    0 // return an integer exit code
