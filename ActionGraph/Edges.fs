namespace ActionGraph

open ActionGraph.GraphEvents

module Edges =
    let rec Walk(x, fromNode:Node, graph: Graph, velocity:GraphValue) =
            match x with
            | ConditionalEdge a ->
                let termTemplate = GraphExpressions.ParseTemplate(a.Condition.Term)
                let followingTermTemplate = GraphExpressions.ParseTemplate(a.Condition.FollowingTerm)
                let termValue = GraphExpressions.EvaluateTemplate(a.Condition.Term, termTemplate, fromNode, graph, velocity)
                let followingTermValue = GraphExpressions.EvaluateTemplate(a.Condition.FollowingTerm, followingTermTemplate, fromNode, graph, velocity)
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
                            graph.EventLog.ProcessEvent(fromNode, targetGraph.Nodes.[toNode], velocity, a.Id, graph)
                            targetGraph.Nodes.[toNode]
                        | FunctionEdge e -> 
                            let returnnode = e(fromNode, targetGraph.Nodes.[toNode], velocity, targetGraph)
                            graph.EventLog.ProcessEvent(fromNode, targetGraph.Nodes.[toNode], velocity, a.Id, graph)
                            returnnode
                    else
                        fromNode
                match a.To with
                | StringValue s ->
                    let target = GraphExpressions.ParseTarget(s)
                    let targetNode = GraphExpressions.WalkTargetExpression(fromNode, target)
                    doWalk(targetNode.Id, targetNode.Graph.Force())
                | _ ->
                    doWalk(a.To, graph)
            | ExpressionEdge a -> 
                let target = GraphExpressions.ParseTarget(x.Action)
                if(graph.EdgeActions.ContainsKey(target.Target.ToString())) then
                    let targetNode = GraphExpressions.WalkTargetExpression(fromNode,target)
                    //call registered edge
                    match graph.EdgeActions.[target.Target.ToString()] with 
                    | ActionEdge e -> 
                        e(fromNode,targetNode, velocity, graph)
                        graph.EventLog.ProcessEvent(fromNode, targetNode, velocity, a.Id, graph)
                        targetNode
                    | FunctionEdge e ->
                        let returnnode = e(fromNode, targetNode, velocity, graph)
                        graph.EventLog.ProcessEvent(targetNode, returnnode, velocity, a.Id, graph)
                        returnnode
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