namespace ActionGraph
module Edges =
    let templateFunctions =
        Map.ofSeq(seq {
        //need to add walk:velocity, this:value
            yield ("node",
                fun(targetValue:string, fromNode:Node, graph: Graph, velocity:GraphValue) ->
                    let targetingExpression = GraphExpressions.ParseTarget(targetValue)
                    let targetNode = GraphExpressions.WalkTargetExpression(fromNode, targetingExpression)
                    //Since the action we are doing in this case is resolving the graph node, we need to do some gymnastics to handle neighbors
                    let value = targetNode.Graph.Value.Nodes.[StringValue(targetingExpression.Target)].Value
                    GraphConversions.collapseGraphLikeToGraphValue(value)
            );
            yield ("walk",
                fun(targetValue, fromNode:Node, graph: Graph, velocity:GraphValue) ->
                    match targetValue with
                    | "velocity" -> Some(velocity)
                    | _ -> Some(StringValue(targetValue))
            );
            yield ("this",
                fun(targetValue, fromNode:Node, graph: Graph, velocity:GraphValue) ->
                    match targetValue with
                    | "Value" -> Some(StringValue(fromNode.Value.ToString()))
                    | "Id" -> Some(fromNode.Id)
                    | _ -> Some(StringValue(targetValue))
            );
        }
        )
    let rec Walk(x, fromNode:Node, graph: Graph, velocity:GraphValue) =
            match x with
            | ConditionalEdge a ->
                //Check whether condition is valid
                let evaluateTemplate(term, termOption:Option<Template>) =
                    match termOption with
                    | Some t -> templateFunctions.[t.TargetType](t.TargetValue, fromNode, graph, velocity)
                    | None -> Some(term)
                let termTemplate = GraphExpressions.ParseTemplate(a.Condition.Term)
                let followingTermTemplate = GraphExpressions.ParseTemplate(a.Condition.FollowingTerm)
                let termValue = evaluateTemplate(a.Condition.Term, termTemplate)
                let followingTermValue = evaluateTemplate(a.Condition.FollowingTerm, followingTermTemplate)
                if(termValue = followingTermValue) then
                    //if it is, downconvert to edge and walk
                    let downEdge =
                        {
                            Id = a.Id
                            Action = a.Action
                            To = a.To
                        }
                    Walk(Edge(downEdge), fromNode, graph, velocity)
                else
                    fromNode
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
                    let target = GraphExpressions.ParseTarget(s)
                    let targetNode = GraphExpressions.WalkTargetExpression(fromNode, target)
                    doWalk(targetNode.Id, targetNode.Graph.Force())
                | _ ->
                    doWalk(a.To, graph)
            | ExpressionEdge a -> 
                let target = GraphExpressions.ParseTarget(x.Action)
                if(graph.EdgeActions.ContainsKey(target.Target)) then
                    let targetNode = GraphExpressions.WalkTargetExpression(fromNode,target)
                    //call registered edge
                    match graph.EdgeActions.[target.Target] with 
                    | ActionEdge e -> 
                        e(targetNode,targetNode, velocity, graph)
                        targetNode
                    | FunctionEdge e -> e(targetNode, targetNode, velocity, graph)
                else
                    fromNode
    let WalkEdge(this: Graph, fromNode : GraphValue, edge : GraphValue) =
        //Handle case where edge is a function, needs to pass velocity to action
        let edgeId = edge.Id

        if this.Nodes.[fromNode].Edges.ContainsKey(edgeId) then
            let targetEdge = this.Nodes.[fromNode].Edges.[edgeId]
            Walk(targetEdge, this.Nodes.[fromNode],this, edge.Velocity)
        else
            this.Nodes.[fromNode]
    type EdgeLike with
        member x.Walk(fromNode:Node, graph: Graph, velocity:GraphValue) = Walk(x, fromNode, graph, velocity)
    type Graph with
        member x.WalkEdge(fromNode : GraphValue, edge : GraphValue) = WalkEdge(x, fromNode, edge)