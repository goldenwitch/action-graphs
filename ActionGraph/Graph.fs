namespace ActionGraph
open System
open Newtonsoft.Json.Linq

type GraphValue =
    | StringValue of string
    | IntValue of int
    | LongValue of int64
    | FuncValue of GraphValue * GraphValue

    override this.ToString() =
        match this with
            | StringValue s -> s
            | IntValue i -> i.ToString()
            | LongValue i -> i.ToString()
            | FuncValue (f1, f2) -> f1.ToString()+ f2.ToString()

    member this.Id =
        match this with
            | StringValue s -> StringValue(s)
            | IntValue i -> IntValue(i)
            | LongValue i -> LongValue(i)
            | FuncValue (f1, f2) -> f1.Id

    member this.Velocity =
        match this with
            | StringValue s -> StringValue(s)
            | IntValue i -> IntValue(i)
            | LongValue i -> LongValue(i)
            | FuncValue (f1, f2) -> f2.Velocity

type Edge =
    {
        Id : GraphValue
        Action : (Node * Node * GraphValue * Graph -> unit)
        To : GraphValue
    }

and Node =
    {
        Id : GraphValue
        Edges : Map<GraphValue, Edge>
        mutable Value : GraphLike
    }
and Graph =
    {
        Nodes : Map<GraphValue, Node>
    }

    member this.WalkEdge(fromNode : GraphValue, edge : GraphValue) =
        //Handle case where edge is a function, needs to pass velocity to action
        let edgeId = edge.Id

        if this.Nodes.[fromNode].Edges.ContainsKey(edgeId) then
            let targetEdge = this.Nodes.[fromNode].Edges.[edgeId]
            targetEdge.Action(this.Nodes.[fromNode], this.Nodes.[targetEdge.To], edge.Velocity, this)
            this.Nodes.[targetEdge.To]
        else
            this.Nodes.[fromNode]
and GraphLike =
    | GraphValue of GraphValue
    | Graph of Graph
