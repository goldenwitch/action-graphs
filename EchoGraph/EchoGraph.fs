namespace ActionGraph.EchoGraph
open System
open System.IO
open ActionGraph
open ActionGraph.Expressions
open System.Linq

module EchoGraph =
    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("add",
                    function(fromNode : Node, _ : Node, velocity, source) -> 
                            let vel = GraphConversions.HandleVelocity(velocity, 1);
                            GraphConversions.actOnGraphLikeAsInt(fromNode.Value, fun a -> fromNode.Value <- GraphConversions.assignIntAsGraphLike(a+vel))
                );
                yield ("text",
                    function(fromNode : Node, _ : Node, velocity, _) ->
                            GraphConversions.actOnNodeByName(fromNode.Value, StringValue ("text"), fun a -> GraphConversions.actOnGraphLikeAsString(a.Value, fun b -> Console.WriteLine(b+velocity.ToString())))
                );
                yield ("templateText",
                    function(fromNode : Node, _ : Node, velocity, graph) ->
                            let value = GraphConversions.collapseGraphLikeToString(fromNode.Value)
                            match value with
                            | Some v ->
                                let mutable outputstring = v;
                                //extract all templates from v
                                let templates = v.Split("{", StringSplitOptions.RemoveEmptyEntries).Select(fun a ->
                                    if(a.Contains("}")) then
                                        a.Split("}", StringSplitOptions.RemoveEmptyEntries).First()
                                    else
                                        ""
                                    )
                                for template in templates do
                                    if template.Contains(":") then
                                        let splitplate = template.Split(":",StringSplitOptions.RemoveEmptyEntries)
                                        match (splitplate.[0], splitplate.[1]) with
                                        | ("node", _) & (var1, var2) -> 
                                            match GraphConversions.getNodeByName(Graph(graph), StringValue(var2)) with
                                                | Some a -> outputstring <- outputstring.Replace("{"+var1+":"+var2+"}",  GraphConversions.someOrDefault(GraphConversions.collapseGraphLikeToInt(a.Value), 0).ToString())
                                                | None -> ()
                                        | ("system", "velocity") & (var1, var2) -> outputstring <- outputstring.Replace("{"+var1+":"+var2+"}", GraphConversions.someOrDefault(GraphConversions.collapseGraphValueToString(velocity), "Type Cooercion Failed"))
                                        | (_,_) -> ()
                                            
                                Console.WriteLine(outputstring);
                                //
                                //loop through templates
                            | None -> ()
                );
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("echoGraph.json"), edgeFunctions)
        graphDefinition

    type EchoWalker =
        {
            BaseWalker : SuperWalker
        }

        member this.Walk(graph : Graph, expression : string) =
            this.BaseWalker.Walk(graph,expression)
            ()

    let LoadWalker(graph, currentNode) =
        {
            BaseWalker = {
                BaseWalker =
                    {
                        CurrentNode = currentNode
                    }
                SuperEdges = 
                    Map.ofSeq(
                        seq{
                            yield ("Count", 
                                function(input : Walker, _) ->
                                        //Save current node
                                        let currentNode = input.CurrentNode
                                        //teleport to count
                                        input.CurrentNode <- graph.Nodes.[StringValue "Count"]
                                        //walk count add=1
                                        input.Walk(graph, "add=1")
                                        //teleport back to current node
                                        input.CurrentNode <- currentNode
                                )
                        }
                    )
            }
        }