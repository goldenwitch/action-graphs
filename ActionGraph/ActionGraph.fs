namespace ActionGraph
open System
open Newtonsoft.Json.Linq

module ActionGraph = 
    let Load(jsonText, edgeFunctions : Map<string, (Node * Node * GraphValue * Graph -> unit)>) =
        // If input matches our dictionary of edge functions
        let (|Action|None|) (input) = if edgeFunctions.ContainsKey(input) then Action else None
        let extractAction(input) =
            match input with
                | Action -> edgeFunctions.[input]
                | None -> fun(input, output, velocity, graph) -> ()

        let json = JToken.Parse(jsonText)

        let (|GraphToken|StringToken|IntToken|) (input:JToken) =
            if input.Type = JTokenType.Object then GraphToken 
            else if input.Type = JTokenType.Integer then IntToken 
            else StringToken
        let (|StringToken|IntToken|) (input:JToken) =
                   if input.Type = JTokenType.Integer then IntToken 
                   else StringToken
        let extractGraphValue (input: JToken) =
            match input with
                | IntToken -> IntValue(input.Value<int>())
                | StringToken -> StringValue(input.ToString())
        let rec extractGraphLike (input:JToken) =
            match input with
                | GraphToken -> parseGraph(input)
                | IntToken -> GraphConversions.assignIntAsGraphLike(input.Value<int>())
                | StringToken -> GraphConversions.assignStringAsGraphLike(input.ToString())
        and parseGraph(input: JToken) =
            Graph ({
                Nodes = 
                    Map.ofSeq(
                        seq {
                            for item in input.["Nodes"] do
                                let newNode =
                                    {
                                        Id = extractGraphValue(item.["Id"])
                                        Edges =
                                            Map.ofArray(
                                                [| for edge in item.["Edges"] do
                                                    let newEdge =
                                                        {
                                                            Id = extractGraphValue(edge.["Id"])
                                                            Action = extractAction(edge.["Action"].Value<String>())
                                                            To = extractGraphValue(edge.["To"])
                                                        }
                                                    yield (newEdge.Id, newEdge)
                                                |]
                                            )
                                        Value = extractGraphLike(item.["Value"])
                                    }
                                yield (newNode.Id, newNode)
                        }
                    )
            })
        parseGraph(json)
