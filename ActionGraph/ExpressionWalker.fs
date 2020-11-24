namespace ActionGraph

module Expressions = 
    type Walker<'T> =
        {
            mutable CurrentNode : Node<'T>
        }

        member this.Walk(graph : Graph<'T>, expression : string) =
            //Accepts graph and expression
            //Expression in form of "Edge.Edge.Edge" OR "Edge=1.Edge.Edge=5"
            for label in expression.Split(".") do
                //Split out params
                let valueSplit = label.Split("=")
                if valueSplit.Length <= 1 then
                    this.CurrentNode <- graph.WalkEdge(this.CurrentNode.Id, StringValue(label))
                else
                    this.CurrentNode <- graph.WalkEdge(this.CurrentNode.Id, FuncValue((StringValue(valueSplit.[0]), StringValue(valueSplit.[1]))))
    
    type SuperWalker<'T> =
        {
            CurrentNode : Node<'T>
            SuperEdges : Map<string, (Walker<'T> * GraphValue -> Unit)>
        }

        member this.Walk(graph : Graph<'T>, expression : string) =
            let baseWalker : Walker<'T> = 
                {
                    CurrentNode = this.CurrentNode
                }
        
            for label in expression.Split(".") do
                let valueSplit = label.Split("=")
                if valueSplit.Length <= 1 then
                    if this.SuperEdges.ContainsKey(label) then //If the next item on the expression matches an item from the superedges, run the super edge
                        this.SuperEdges.[label](baseWalker, StringValue(label))
                    else //Else run the normal walker on the remaining components of the expression
                        baseWalker.Walk(graph, label)
                else
                    if this.SuperEdges.ContainsKey(valueSplit.[0]) then //If the next item on the expression matches an item from the superedges, run the super edge
                        this.SuperEdges.[valueSplit.[0]](baseWalker, StringValue(valueSplit.[1]))
                    else //Else run the normal walker on the remaining components of the expression
                        baseWalker.Walk(graph, label)
            ()