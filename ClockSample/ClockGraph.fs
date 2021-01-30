namespace ActionGraph.ClockSample
open ActionGraph.Expressions
open ActionGraph
open System.Threading.Tasks
open System.IO

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
                                        input.Walk(graph, "tick.navigate")
                            )
                        }
                    )
            }
        }

    let Definition =
        let edgeFunctions =
            Map.ofSeq(seq{
                yield ("Clock.start", //Only works on fsm graph nodes
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
                yield ("Clock.stop", //Only works on fsm graph nodes
                    ActionEdge(
                        function(fromNode : Node, _, velocity, _) ->
                                match GraphConversions.collapseGraphLikeToGraph(fromNode.Value) with
                                    | Some a -> 
                                        a.Nodes.[StringValue("Value")].Value <- GraphConversions.assignStringAsGraphLike("Stopped")
                                    | None -> ()
                    )
                );
                yield! Prebuilts.SystemEdges
            })
        let graphDefinition = ActionGraph.Load(File.ReadAllText("clockGraph.json"), edgeFunctions)
        graphDefinition
