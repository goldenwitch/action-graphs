namespace ActionGraph

module GraphConversions =
//Some or default
    let someOrDefault<'T>(some:Option<'T>, def) =
        match some with
        | Some a -> a
        | _ -> def
//Collapse graphlike to graph
    let collapseGraphLikeToGraph(input:GraphLike) =
        match input with
            | Graph g -> Some(g)
            | _ -> None
    let actOnGraphLikeAsGraph(input:GraphLike, action) =
        match input with
        | Graph g -> action(g)
        | _ -> ()
//Collapse graphlike to graphvalue
    let collapseGraphLikeToGraphValue(input:GraphLike) =
        match input with
            | GraphValue g -> Some(g)
            | _ -> None
    let actOnGraphLikeAsGraphValue(input:GraphLike, action) =
        match input with
        | GraphValue g -> action(g)
        | _ -> ()
//Collapse graphvalue to int
    let collapseGraphValueToInt(input:GraphValue) =
        match input with
        | IntValue s -> Some(s)
        | _ -> None
    let actOnIntValue(input:GraphValue, action) =
        match input with
        | IntValue i -> action(i)
        | _ -> ()
//Collapse graphvalue to string
    let collapseGraphValueToString(input:GraphValue) =
        match input with
        | StringValue s -> Some(s)
        | _ -> None
    let actOnStringValue(input:GraphValue, action) =
        match input with
        | StringValue i -> action(i)
        | _ -> ()
//Collapse graphlike to string
    let collapseGraphLikeToString(input:GraphLike) =
        let output = collapseGraphLikeToGraphValue(input)
        match output with
        | Some g -> collapseGraphValueToString(g)
        | _ -> None
    let actOnGraphLikeAsString(input:GraphLike, action) =
        actOnGraphLikeAsGraphValue(input, fun a -> actOnStringValue(a, action))
//Collapse graphlike to int
    let collapseGraphLikeToInt(input:GraphLike) =
        let output = collapseGraphLikeToGraphValue(input)
        match output with
        | Some g -> collapseGraphValueToInt(g)
        | _ -> None
    let actOnGraphLikeAsInt(input:GraphLike, action) =
        actOnGraphLikeAsGraphValue(input, fun a -> actOnIntValue(a, action))
//Graph has node
    let (|GraphHasNode|_|) (input:Graph, nodeId:GraphValue) = if input.Nodes.ContainsKey(nodeId) then Some(input.Nodes.[nodeId]) else None
//Get node by name in graphlike
    let actOnNodeByName(input:GraphLike, targetNode:GraphValue, action) =
        match input with
        | Graph g -> 
            match (g,targetNode) with
                | GraphHasNode a -> action(a)
                | _ -> ()
        | _ -> ()

    let getNodeByName(input:GraphLike, targetNode:GraphValue) =
        let output = collapseGraphLikeToGraph(input)
        match output with
        | Some g -> 
            match (g,targetNode) with
                | GraphHasNode a -> Some(a)
                | _ -> None
        | _ -> None

//Assign int as GraphLike
    let assignIntAsGraphLike(input:int) =
        GraphValue(IntValue(input))
//Assign string as GraphLike
    let assignStringAsGraphLike(input:string) =
        GraphValue(StringValue(input))

    let HandleVelocity(vel, def) =
        let (|Integer|_|) (str: string) =
           let mutable intvalue = 0
           if System.Int32.TryParse(str, &intvalue) then Some(intvalue)
           else None
        let (|IntValue|_|) (vel: GraphValue) =
            match vel with
            | IntValue i -> Some(i)
            | _ -> None
        let (|StringValue|_|) (vel: GraphValue) =
            match vel with
            | StringValue s ->
                match s with
                | Integer i -> Some(i)
                | _ -> None
            | _ -> None
        match vel with
           | IntValue i -> i
           | StringValue s -> s
           | _ -> def