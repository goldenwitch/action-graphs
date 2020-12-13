namespace ActionGraph
open System.Linq
//Forbidden chars to fix with better pattern recognition
// : inside of template strings
// { and } near template strings
//Cases
//node targeting
//  child>action
//  child>child
//  <parentsneighbor
//  <<parentsparent
//  <parent
//  <parentAction
//autofilling templates in edge values
//  "Your mother has a {node:nodeTarget}"
//  "The bread weighs {walk:velocity}"
type NavStep =
    | Up
    | Down of string
    | End
type TargetingExpression =
    {
        NavSteps : seq<NavStep>
        Target : string
    }
type Template =
    {
        TargetType : string
        TargetValue : string
    }
module GraphExpressions =
    let ParseTarget(input:string) = //Targeting is broken for ints... idk how to fix :)
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
            Target = array.Last() //Need to benchmark perf for this, could be pretty bad for large navs
        }
    let WalkTargetExpression(startNode, target) =
        let mutable targetNode = startNode
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
        targetNode
    let ParseTemplate(input:GraphValue) =
        let inputString = GraphConversions.collapseGraphValueToString(input)
        match inputString with
        | Some a ->
            match a.Substring(0,1) with
            | "{" ->
                let inputsplit = a.Remove(a.Length-1,1).Remove(0,1).Split(":")
                Some({
                    TargetType = inputsplit.[0]
                    TargetValue = inputsplit.[1]
                })
            | _ -> None
        | None -> None