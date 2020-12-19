namespace ActionGraph
open System.Linq
open System.Text

//Forbidden chars to fix with better pattern recognition
// : inside of template strings
// { and } near template strings
//Cases
//node targeting
//[] = me = no steps
//[[child]] = down(child)
//.[] = my parent = up()
//[neighbor] = neighbor(neighbor)
//[neighbors[child]] = neighbor(neighbors)down(child)
//.[parentsneighbor] = up()neighbor(parentsneighbor)
//.[parentsneighbors[child]]
//..[grandparentsneighbors[child]]
//..[] = grandparent
//autofilling templates in edge values
//  "Your mother has a {node:nodeTarget}"
//  "The bread weighs {walk:velocity}"
type NavEnum =
    | Down
    | Neighbor
type NavStep =
    | Up
    | Down of string
    | Neighbor of string
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
    let rec ParseTail(input:list<char>, output:StringBuilder) =
        if not (input.Head = ']') then
            ParseTail(input.Tail, output.Append(input.Head))
        else
            output
    let rec ParseParentSteps(input:list<char>, output:list<NavStep>) =
        //if starting character is ., peek and pop characters until we peek a [
        //emit "Up" for each .
        if(input.Head = '.') then
            ParseParentSteps((input.Tail, Up :: output))
        else
            (input, output)
    let rec ParseSpecificNodeIdentifier(input:list<char>, output:list<NavStep>, (carryNav:option<NavEnum>, carryString:string)) =
        //output a sequence of navsteps
        //down [string
        //string neighbor
        //done ] or null
        //If we see ] we are done, and can exit
        match carryNav with
        | Some a -> //If we already have a nav we are carrying, [ indicates that we are done, and can add this step to our output, but also populate carrynav with down. ] indicates we have our final result
            if input.Head = ']' then
                match a with
                | NavEnum.Down -> 
                    Down(carryString) :: output
                | NavEnum.Neighbor ->
                    Neighbor(carryString) :: output
            else if input.Head = '[' then
                match a with
                | NavEnum.Down -> 
                    ParseSpecificNodeIdentifier(input.Tail, Down(carryString) :: output, (Some(NavEnum.Down), ""))
                | NavEnum.Neighbor ->
                    ParseSpecificNodeIdentifier(input.Tail, Neighbor(carryString) :: output, (Some(NavEnum.Down), ""))
            else
                ParseSpecificNodeIdentifier(input.Tail, output, (carryNav, carryString+input.Head.ToString()))
        | None -> //if we don't yet have a carry, look for ] to indicate we have our final result
            if input.Head = ']' then
                output
            else if input.Head = '[' then
                ParseSpecificNodeIdentifier(input.Tail, output, (Some(NavEnum.Down), ""))
            else
                ParseSpecificNodeIdentifier(input.Tail, output, (Some(NavEnum.Neighbor), input.Head.ToString()))

    let ParseTarget(input:string) =
        //relative target expression has 3 components 
        //parent climb (optional)
        //specific node identifier
        //and some identifying tail expression (optional)
        let conslist = List.ofSeq(input) //O(n)
        let remaininglist, navsteps = ParseParentSteps(conslist, [])
        //after we have removed the front, extract the back character by character until we hit ]
        let snoclist = List.rev(conslist)
        let tailBuilder = new StringBuilder()
        let tail = System.String(ParseTail(snoclist, tailBuilder).ToString().Reverse().ToArray())
        {
            NavSteps = Seq.ofList(List.rev(ParseSpecificNodeIdentifier(remaininglist.Tail, navsteps, (None, ""))))
            Target = tail
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
            | Neighbor a -> 
                targetNode <- targetNode.Graph.Value.Nodes.[StringValue(a)]
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