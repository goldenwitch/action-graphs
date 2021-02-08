namespace ActionGraph.Projections

open ActionGraph

module ProjectionTypes =
    type ProjectionOutput =
        | StringOut of string
    type ProjectionRule =
        | GraphRule of (Graph -> Option<ProjectionOutput>)
        | NodeRule of (Node -> Option<ProjectionOutput>)
        | EdgeRule of (Node * EdgeLike -> Option<ProjectionOutput>)
        // disabled on account of events being modeled outside of the graph. Need some magic rearrangement behind the scenes
        //| EventRule of (Node * GraphEvent -> ProjectionOutput) 