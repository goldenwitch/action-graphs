namespace ActionGraph.EchoSample
open System.IO
open ActionGraph

module EchoGraph =
    let Definition =
        let edgeFunctions = Map.ofSeq(Prebuilts.ConsoleEdges)
        let graphDefinition = ActionGraph.Load(File.ReadAllText("echoGraph.json"), edgeFunctions)
        graphDefinition