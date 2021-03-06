﻿namespace ActionGraph
open System.Linq
open System.Text

//Consider generalizable expression library
//"Count of symbol" -> ... = 3 ...
//"Cons list" -> [a, [b, [c]]]
//"tail" -> _)tail where ) can be replaced by any termination symbol

//Forbidden chars to fix with better pattern recognition
// : inside of template strings
// { and } near template strings
// # forbidden inside of ids
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
    | Down of GraphValue
    | Neighbor of GraphValue
    | End
type TargetingExpression =
    {
        NavSteps : seq<NavStep>
        Target : GraphValue
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
                    if(carryString.First() = '#') then
                        Down(IntValue(carryString.Substring(1,carryString.Length-1) |> int)) :: output
                    else
                        Down(StringValue(carryString)) :: output
                | NavEnum.Neighbor ->
                    if(carryString.First() = '#') then
                        Neighbor(IntValue(carryString.Substring(1,carryString.Length-1) |> int)) :: output
                    else
                        Neighbor(StringValue(carryString)) :: output
            else if input.Head = '[' then
                match a with
                | NavEnum.Down ->
                    if(carryString.First() = '#') then
                        ParseSpecificNodeIdentifier(input.Tail, Down(IntValue(carryString.Substring(1,carryString.Length-1) |> int)) :: output, (Some(NavEnum.Down), ""))
                    else
                        ParseSpecificNodeIdentifier(input.Tail, Down(StringValue(carryString)) :: output, (Some(NavEnum.Down), ""))
                | NavEnum.Neighbor ->
                    if(carryString.First() = '#') then
                        ParseSpecificNodeIdentifier(input.Tail, Neighbor(IntValue(carryString.Substring(1,carryString.Length-1) |> int)) :: output, (Some(NavEnum.Down), ""))
                    else
                        ParseSpecificNodeIdentifier(input.Tail, Neighbor(StringValue(carryString)) :: output, (Some(NavEnum.Down), ""))
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
        let tailValue = 
            match tail with
            | "" -> StringValue(tail)
            | _ ->
                match tail.First() with
                | '#' -> IntValue(tail.Substring(1, tail.Length-1) |> int)
                | _ -> StringValue(tail)
        {
            NavSteps = Seq.ofList(List.rev(ParseSpecificNodeIdentifier(remaininglist.Tail, navsteps, (None, ""))))
            Target = tailValue
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
                | Graph g -> targetNode <- g.Nodes.[a]
            | Neighbor a -> 
                targetNode <- targetNode.Graph.Value.Nodes.[a]
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
    
    let DefaultTemplateFunctions =
        Map.ofSeq(seq {
        //need to add walk:velocity, this:value
            yield ("node",
                fun(targetValue:string, fromNode:Node, graph: Graph, velocity:GraphValue) ->
                    let targetingExpression = ParseTarget(targetValue)
                    let targetNode = WalkTargetExpression(fromNode, targetingExpression)
                    GraphConversions.collapseGraphLikeToGraphValue(targetNode.Value)
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
    //Check whether condition is valid
    let EvaluateTemplate(term, termOption:Option<Template>, fromNode:Node, graph: Graph, velocity:GraphValue) =
        match termOption with
        | Some t -> DefaultTemplateFunctions.[t.TargetType](t.TargetValue, fromNode, graph, velocity)
        | None -> Some(term)

    let ReplaceTemplates(input:string, fromNode:Node, graph: Graph, velocity:GraphValue) =
        //Find and replace all templates with their values
        //generate tuples of template+value
        //split on {
        let templates = input.Split('{')
        //Every odd index will contain a template
        let tuples = seq {
            for candidate in 1 .. templates.Length-1 do
                //split these odd indexes on }
                    let template = templates.[candidate].Split('}')
                    //The 0th index will contain the template
                    //wrap it in {}
                    let key = "{"+template.[0]+"}"
                    //pass it to the parser
                    //pass it to the evaluator
                    let value = EvaluateTemplate(StringValue(key), ParseTemplate(StringValue(key)), fromNode, graph, velocity)
                    match value with
                    | Some s -> yield (key,s) //add a key,value to the sequence
                    | None -> ()
        }
        let mutable outputString = input;
        //loop through sequence and find and replace the templates
        for (a,b) in tuples do
            outputString <- outputString.Replace(a.ToString(),b.ToString())
        outputString
