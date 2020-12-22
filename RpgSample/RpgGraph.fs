namespace RpgSample

open ActionGraph.Expressions
open ActionGraph
open System.Threading.Tasks
open System.IO
open System
open ActionGraph.Edges

module RpgGraph =
    let random = new System.Random()
    let getRandomInt (min, max) = random.Next(min, max)
    let LoadWalker(graph, currentNode, travelInterval) =
        {
            CurrentNode = currentNode
        }

    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("query", //Only works on graphs of strings, uses velocity as id for nodes to walk
                    FunctionEdge(
                        function(fromNode : Node, _, velocity, _) ->
                                match GraphConversions.collapseGraphLikeToGraph(fromNode.Value) with
                                    | Some a -> 
                                        a.WalkEdge(velocity, StringValue("queryText"))
                                    | None -> fromNode
                    )
                );
                yield ("text", //Only works on strings
                    ActionEdge(
                        function(fromNode : Node, _ , _, _) ->
                                GraphConversions.actOnGraphLikeAsString(fromNode.Value, fun b -> Console.WriteLine(b))
                    )
                );
                yield ("any",
                    FunctionEdge(
                        function(fromNode : Node, _, velocity, _) ->
                                match GraphConversions.collapseGraphLikeToGraph(fromNode.Value) with
                                    | Some a ->
                                        let randomNode = getRandomInt(0, a.Nodes.Count)
                                        a.WalkEdge(IntValue(randomNode), StringValue("any"))
                                    | None -> fromNode
                            )   
                                
                );
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("rpgGraph.json"), edgeFunctions)
        graphDefinition
