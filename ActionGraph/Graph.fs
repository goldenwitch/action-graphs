namespace Graph
open System
open Newtonsoft.Json.Linq

type GraphValue =
    | StringValue of string
    | IntValue of int
    | FuncValue of GraphValue * GraphValue

    override this.ToString() =
        match this with
            | StringValue s -> s
            | IntValue i -> i.ToString()
            | FuncValue (f1, f2) -> f1.ToString()+ f2.ToString()

    member this.Id =
        match this with
            | StringValue s -> StringValue(s)
            | IntValue i -> IntValue(i)
            | FuncValue (f1, f2) -> f1.Id

    member this.Velocity =
        match this with
            | StringValue s -> StringValue(s)
            | IntValue i -> IntValue(i)
            | FuncValue (f1, f2) -> f2.Velocity

type Edge<'T> =
    {
        Id : GraphValue
        Action : (Node<'T> * Node<'T> * GraphValue -> unit)
        To : GraphValue
    }

and Node<'T> =
    {   
        Id : GraphValue
        Edges : Map<GraphValue, Edge<'T>>
        mutable Value : 'T 
    }

type Graph<'T> =
    {
        Nodes : Map<GraphValue, Node<'T>>
    }

    member this.WalkEdge(fromNode : GraphValue, edge : GraphValue) =
        //Handle case where edge is a function, needs to pass velocity to action
        let edgeId = edge.Id

        if this.Nodes.[fromNode].Edges.ContainsKey(edgeId) then
            let targetEdge = this.Nodes.[fromNode].Edges.[edgeId]
            targetEdge.Action(this.Nodes.[fromNode], this.Nodes.[targetEdge.To], edge.Velocity)
            this.Nodes.[targetEdge.To]
        else
            this.Nodes.[fromNode]

module ActionGraph = 
    let Load<'T>(jsonText, edgeFunctions : Map<string, (Node<'T> * Node<'T> * GraphValue -> unit)>) =
        let (|Int|String|) (input) = if input = JTokenType.Integer then Int else String
        let extractDU (input, result : JToken) =
            match input with
                | Int -> IntValue (result.Value<int>())
                | String -> StringValue (result.ToString())

        // If input matches our dictionary of edge functions
        let (|Action|None|) (input) = if edgeFunctions.ContainsKey(input) then Action else None
        let extractAction(input) =
            match input with
                | Action -> edgeFunctions.[input]
                | None -> fun(input, output, velocity) -> ()

        let json = JObject.Parse(jsonText)
        let graph =
            {
                Nodes = 
                    Map.ofSeq(
                        seq {
                            for item in json.["Nodes"] do
                                let newNode =
                                    {
                                        Id = extractDU(item.["Id"].Type, item.["Id"])
                                        Edges =
                                            Map.ofArray(
                                                [| for edge in item.["Edges"] do
                                                    let newEdge =
                                                        {
                                                            Id = extractDU(edge.["Id"].Type, edge.["Id"])
                                                            Action = extractAction(edge.["Action"].Value<String>())
                                                            To = extractDU(edge.["To"].Type, edge.["To"])
                                                        }
                                                    yield (newEdge.Id, newEdge)
                                                |]
                                            )
                                        Value = item.["Value"].Value<'T>()
                                    }
                                yield (newNode.Id, newNode)
                        }
                    )
            }
        graph
