namespace ClockSample
open ActionGraph.Expressions
open ActionGraph
open System.Threading.Tasks
open System.IO
open System

module ClockGraph =
    type FSMWalker =
        {
            BaseWalker : SuperWalker
            TravelInterval : int
        }

        member this.DelayedWalk(graph: Graph) =
            async {
                Task.Delay(this.TravelInterval)
                |> Async.AwaitTask
                |> Async.RunSynchronously

                GraphConversions.actOnGraphLikeAsGraphValue(graph.Nodes.[StringValue("Value")].Value,
                    fun a -> 
                        match a with
                        | IntValue i -> this.Walk(graph)
                        | StringValue s -> if(s <> "Stopped") then this.Walk(graph)
                        | _ -> ()
                )
                ()
            }

        member this.Walk(graph : Graph) =
            //Start task to call myself in the future
            this.DelayedWalk(graph)
            |> Async.Start
            this.BaseWalker.Walk(graph, "Navigate")
            ()

    let LoadWalker(graph, currentNode, travelInterval) =
        {
            TravelInterval = travelInterval
            BaseWalker = {
                BaseWalker =
                    {
                        CurrentNode = currentNode
                    }
                SuperEdges = 
                    Map.ofSeq(
                        seq{
                            yield ("Navigate", 
                                function(input : Walker, _) ->
                                        match GraphConversions.collapseGraphLikeToInt(graph.Nodes.[StringValue("Ticks")].Value) with
                                        | Some g -> 
                                            GraphConversions.actOnGraphLikeAsInt(graph.Nodes.[StringValue("TicksAtLastNav")].Value,
                                                fun(j) ->
                                                    graph.Nodes.[StringValue("TicksSinceLastNav")].Value <- GraphConversions.assignIntAsGraphLike(g-j)
                                                )
                                        | None -> ()
                                        ()
                                        input.Walk(graph, "tick.navigate")
                            )
                        }
                    )
            }
        }

    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("start", //Only works on fsm graph nodes
                    ActionEdge(
                        function(fromNode : Node, _, velocity, _) ->
                                match GraphConversions.collapseGraphLikeToGraph(fromNode.Value) with
                                    | Some a -> 
                                        //instantiate FSM walker with stopped as current node
                                        let walker = LoadWalker(a, a.Nodes.[IntValue 12], 100)
                                        //walk navigate
                                        walker.Walk(a)
                                    | None -> ()
                    )
                );
                yield ("stop", //Only works on fsm graph nodes
                    ActionEdge(
                        function(fromNode : Node, _, velocity, _) ->
                                match GraphConversions.collapseGraphLikeToGraph(fromNode.Value) with
                                    | Some a -> 
                                        a.Nodes.[StringValue("Value")].Value <- GraphConversions.assignStringAsGraphLike("Stopped")
                                    | None -> ()
                    )
                );
                yield ("navigate",
                    ActionEdge(
                        function(valueNode, _, _, graph) -> 
                                match valueNode.Parent.Value with
                                | Some n -> 
                                    GraphConversions.actOnGraphLikeAsInt(graph.Nodes.[StringValue("Ticks")].Value, 
                                        fun(i) ->
                                            graph.Nodes.[StringValue("TicksAtLastNav")].Value <- GraphConversions.assignIntAsGraphLike(i)

                                    )
                                | None -> ()
                                GraphConversions.actOnGraphLikeAsString(valueNode.Value, 
                                    fun a -> Console.WriteLine("It is now "+a)
                                )
                                
                    )
                );
                yield ("event",
                    FunctionEdge(
                        function(valueNode, _, _, _) -> 
                                valueNode
                            
                    )
                );
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("clockGraph.json"), edgeFunctions)
        graphDefinition
