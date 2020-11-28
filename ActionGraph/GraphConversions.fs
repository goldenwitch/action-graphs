namespace ActionGraph

module GraphConversions =
//Graph has node
    let (|GraphHasNode|_|) (input:Graph, nodeId:GraphValue) = if input.Nodes.ContainsKey(nodeId) then Some(input.Nodes.[nodeId]) else None
//Act on node by name in graphlike
    let actOnNodeByName(input:GraphLike, targetNode:GraphValue, action) =
        match input with
        | Graph g -> 
            match (g,targetNode) with
                | GraphHasNode a -> action(a)
                | _ -> ()
        | _ -> ()
//Collapse graphlike to graph
    let actOnGraphLikeAsGraph(input:GraphLike, action) =
        match input with
        | Graph g -> action(g)
        | _ -> ()
//Collapse graphlike to graphvalue
    let actOnGraphLikeAsGraphValue(input:GraphLike, action) =
        match input with
        | GraphValue g -> action(g)
        | _ -> ()
//Collapse graphvalue to int
    let actOnIntValue(input:GraphValue, action) =
        match input with
        | IntValue i -> action(i)
        | _ -> ()
//Collapse graphvalue to string
    let actOnStringValue(input:GraphValue, action) =
        match input with
        | StringValue i -> action(i)
        | _ -> ()

//Collapse graphlike to string
    let actOnGraphLikeAsString(input:GraphLike, action) =
        actOnGraphLikeAsGraphValue(input, fun a -> actOnStringValue(a, action))
//Collapse graphlike to int
    let actOnGraphLikeAsInt(input:GraphLike, action) =
        actOnGraphLikeAsGraphValue(input, fun a -> actOnIntValue(a, action))

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