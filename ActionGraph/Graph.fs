namespace ActionGraph
open System
open System.Text
open System.Linq

type NavStep =
    | Up
    | Down of string
    | End
type TargetingExpression =
    {
        NavSteps : seq<NavStep>
        Target : string
    }

module TargetParser =
    let ExtractTarget(input:string) =
        //Split on < and >
        let array = input.Split('<','>')
        //project array -> navsteps
        {
            NavSteps =
                seq{
                    for i in 0 .. array.Length-1 do
                        if i = array.Length-1 then
                            End
                        else if array.[i].Length > 0 then
                            Down(array.[i])
                        else
                            Up
                }
            Target = array.Last()
        }

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
type EdgeLike =
| Edge of Edge
| ExpressionEdge of ExpressionEdge
    
    member x.Walk(fromNode:Node, graph: Graph, velocity:GraphValue) =
        match x with
        | Edge a ->
            let doWalk(toNode, targetGraph) = 
                if(targetGraph.EdgeActions.ContainsKey(a.Action)) then
                    match targetGraph.EdgeActions.[a.Action] with 
                    | ActionEdge e ->
                        e(fromNode, targetGraph.Nodes.[toNode], velocity, targetGraph)
                        targetGraph.Nodes.[toNode]
                    | FunctionEdge e -> e(fromNode, targetGraph.Nodes.[toNode], velocity, targetGraph)
                else
                    fromNode
            match a.To with
            | StringValue s ->
                //Cases 
                //one step to child node child>child --pass but redundant
                //two steps to childs child child1>child2>child2 --pass but redundant
                //one step to parent <parent
                //one step to parents neighbor <neighbor
                //two steps to parents(p1) parent(p2) <<parent2
                //two steps to parents parent's neighbor(p2n) <<p2n
                let target = TargetParser.ExtractTarget(s)
                let mutable targetNode = fromNode
                for navStep in target.NavSteps do
                    match navStep with
                    | Up -> 
                        match targetNode.Parent.Value with
                        | Some a -> targetNode <- a
                        | None -> failwith "Attempted navigation to a parent from graph root."
                    | Down a -> 
                        match targetNode.Value with
                        | GraphValue g -> failwith "Attempted navigation to child node from a node with a non-graph value."
                        | Graph g -> targetNode <- g.Nodes.[StringValue(a)]
                    | End -> ()
                doWalk(StringValue(target.Target), targetNode.Graph.Force())
            | _ ->
                doWalk(a.To, graph)
        | ExpressionEdge a -> 
            let action = TargetParser.ExtractTarget(x.Action)
            if(graph.EdgeActions.ContainsKey(action.Target)) then
                //Navigate to target node by following sequence
                let mutable targetNode = fromNode
                for navStep in action.NavSteps do
                    match navStep with
                    | Up -> 
                        match targetNode.Parent.Value with
                        | Some a -> targetNode <- a
                        | None -> failwith "Attempted navigation to a parent from graph root."
                    | Down a -> 
                        match targetNode.Value with
                        | GraphValue g -> failwith "Attempted navigation to child node from a node with a non-graph value."
                        | Graph g -> targetNode <- g.Nodes.[StringValue(a)]
                    | End -> ()
                
                //call registered edge
                match graph.EdgeActions.[action.Target] with 
                | ActionEdge e -> 
                    e(targetNode,targetNode, velocity, graph)
                    targetNode
                | FunctionEdge e -> e(targetNode, targetNode, velocity, graph)
            else
                fromNode
    member x.Action =
        match x with
        | Edge a  -> a.Action
        | ExpressionEdge a -> a.Action
and Node =
    {
        Graph: Lazy<Graph>
        Parent: Lazy<Option<Node>>
        Id : GraphValue
        Edges : Map<GraphValue, EdgeLike>
        mutable Value : GraphLike
    }
and EdgeAction =
| ActionEdge of (Node * Node * GraphValue * Graph -> unit)
| FunctionEdge of (Node * Node * GraphValue * Graph -> Node)
and Graph =
    {
        Nodes : Map<GraphValue, Node>
        EdgeActions : Map<String, EdgeAction>
    }

    member this.WalkEdge(fromNode : GraphValue, edge : GraphValue) =
        //Handle case where edge is a function, needs to pass velocity to action
        let edgeId = edge.Id

        if this.Nodes.[fromNode].Edges.ContainsKey(edgeId) then
            let targetEdge = this.Nodes.[fromNode].Edges.[edgeId]
            targetEdge.Walk(this.Nodes.[fromNode],this, edge.Velocity)
        else
            this.Nodes.[fromNode]

    override this.ToString() =
        let stringBuilder = new StringBuilder()
        for node in this.Nodes do
            stringBuilder.Append(node.Key.ToString()+", ")
            |> ignore
        stringBuilder.ToString()

and GraphLike =
    | GraphValue of GraphValue
    | Graph of Graph
