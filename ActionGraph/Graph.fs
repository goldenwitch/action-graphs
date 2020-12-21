namespace ActionGraph
open System
open System.Text
open System.Linq

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
        Action : String
        To : GraphValue
    }
type ExpressionEdge =
    {
        Id : GraphValue
        Action : String //Replace with 'Action' Type later, that validates the incoming string and offers debug messages
    }
type Operator =
| Equals
type Condition =
    {
        Term : GraphValue
        Operator : Operator
        FollowingTerm : GraphValue
    }
type ConditionalEdge =
    {
        Id : GraphValue
        Condition : Condition
        Action : String
        To : GraphValue
    }
type EdgeLike =
| Edge of Edge
| ExpressionEdge of ExpressionEdge
| ConditionalEdge of ConditionalEdge
    member x.Action =
        match x with
        | Edge a  -> a.Action
        | ExpressionEdge a -> a.Action
        | ConditionalEdge a -> a.Action
and GraphEvent =
    {
        ObserverNode: Node
        EventTarget: string
        Action: string
    }

and Node =
    {
        Graph: Lazy<Graph>
        Parent: Lazy<Option<Node>>
        Id : GraphValue
        Edges : Map<GraphValue, EdgeLike>
        mutable Value : GraphLike
    }

    override this.ToString() =
        this.Id.ToString()

and 
    [<CustomEquality>]
    [<NoComparison>]
    EdgeAction =
        | ActionEdge of (Node * Node * GraphValue * Graph -> unit)
        | FunctionEdge of (Node * Node * GraphValue * Graph -> Node)

         override x.Equals(b) =
            true
and 
    [<CustomEquality>]
    [<NoComparison>]
    EventAction =
        | EventAction of Map<string, (Node * Node * Node * GraphValue * Graph -> unit)>

        override x.Equals(b) =
            true 
and EventLog =
    {
        mutable Events: list<GraphEvent>
        EventActions: EventAction
    }
and 
    Graph =
    {
        Nodes : Map<GraphValue, Node>
        EdgeActions : Map<String, EdgeAction>
        EventLog: EventLog
    }

    override this.ToString() =
        let stringBuilder = new StringBuilder()
        for node in this.Nodes do
            stringBuilder.Append(node.Key.ToString()+", ")
            |> ignore
        stringBuilder.ToString()

and GraphLike =
    | GraphValue of GraphValue
    | Graph of Graph

    override this.ToString() =
        match this with
        | GraphValue g -> g.ToString()
        | Graph g -> g.ToString()