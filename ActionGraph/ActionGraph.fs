namespace ActionGraph
open System
open ActionGraph.GraphEvents
open Newtonsoft.Json.Linq

module ActionGraph = 
    let Load(jsonText, edgeFunctions : Map<string, EdgeAction>) =
        let json = JToken.Parse(jsonText)
        let eventLog = GraphEvents.DefaultEventLog
        let (|GraphToken|StringToken|IntToken|) (input:JToken) =
            if input.Type = JTokenType.Object then GraphToken 
            else if input.Type = JTokenType.Integer then IntToken 
            else StringToken
        let (|StringToken|IntToken|) (input:JToken) =
                   if input.Type = JTokenType.Integer then IntToken 
                   else StringToken
        let extractGraphValue(input: JToken) =
            match input with
                | IntToken -> IntValue(input.Value<int>())
                | StringToken -> StringValue(input.ToString())
        let rec extractGraphLike (input:JToken, parent) =
            match input with
                | GraphToken -> parseGraph(input, parent)
                | IntToken -> GraphConversions.assignIntAsGraphLike(input.Value<int>())
                | StringToken -> GraphConversions.assignStringAsGraphLike(input.ToString())
        and parseGraph(input: JToken, parent) =
            let rec recursivelyGraph = {
                Nodes = 
                    Map.ofSeq(
                        seq {
                            for item in input.["Nodes"] do
                                let rec newNode =
                                    {
                                        Graph = lazy recursivelyGraph
                                        Parent = parent
                                        Id = extractGraphValue(item.["Id"])
                                        Edges =
                                            Map.ofArray(
                                                [| for edge in item.["Edges"] do
                                                    match edge.["To"] with
                                                    | null -> 
                                                        let newEdge =
                                                            {
                                                                Id = extractGraphValue(edge.["Id"])
                                                                Action = edge.["Action"].ToString()
                                                            }
                                                        yield (newEdge.Id, ExpressionEdge(newEdge))
                                                    | input ->
                                                        match edge.["Condition"] with
                                                        | null ->
                                                            let newEdge =
                                                                {
                                                                    Id = extractGraphValue(edge.["Id"])
                                                                    Action = edge.["Action"].ToString()
                                                                    To = extractGraphValue(input)
                                                                }
                                                            yield (newEdge.Id, Edge(newEdge))
                                                        | condition ->
                                                            let newEdge =
                                                                {
                                                                    Id = extractGraphValue(edge.["Id"])
                                                                    Condition =
                                                                        {
                                                                            Term = extractGraphValue(condition.["Term"])
                                                                            Operator = Equals
                                                                            FollowingTerm = extractGraphValue(condition.["FollowingTerm"])

                                                                        }
                                                                    Action = edge.["Action"].ToString()
                                                                    To = extractGraphValue(input)
                                                                }
                                                            yield (newEdge.Id, ConditionalEdge(newEdge))
                                                |]
                                            )
                                        Value = extractGraphLike(item.["Value"], lazy Some(newNode))
                                    }
                                match item.["Events"] with
                                | null -> ()
                                | events ->
                                    for event in events do
                                        eventLog.AddEvent(
                                            {
                                                ObserverNode = newNode
                                                EventTarget = event.["EventTarget"].ToString()
                                                Action = event.["Action"].ToString()
                                            }
                                        )
                                yield (newNode.Id, newNode)
                        }
                    )
                EdgeActions =
                    edgeFunctions
                EventLog = eventLog
            }
            Graph(recursivelyGraph)
        parseGraph(json, lazy None)
