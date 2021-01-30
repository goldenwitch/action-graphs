# Understanding Action-Graphs
<details open="open">
  <summary><h2 style="display: inline-block">Table of Contents</h2></summary>
  <ol>
    <li>
      <a href="#anatomy-of-an-action-graph">Anatomy of an Action-Graph</a>
        <ul>
            <li>
                <a href="#prerequisites">Graphs</a>
                <ul>
                    <li>
                        <a href="#installation">Nodes</a>
                        <ul>
                            <li>
                                <a href="#installation">Values</a>
                            </li>
                        </ul>
                    </li>
                    <li>
                        <a href="#installation">Contextual Node Targeting</a>
                    </li>
                    <li>
                        <a href="#installation">Edges</a>
                        <ul>
                            <li>
                                <a href="#installation">Actions</a>
                            </li>
                            <li>
                                <a href="#installation">Conditions</a>
                            </li>
                        </ul>
                    </li>
                    <li>
                        <a href="#installation">Events</a>
                        <ul>
                            <li>
                                <a href="#installation">Event Actions</a>
                            </li>
                        </ul>
                    </li>
                </ul>
            </li>
            <li>
                <a href="#prerequisites">Walkers</a>
                <ul>
                    <li>
                        <a href="#prerequisites"> Expressions</a>
                    </li>
                </ul>
            </li>
        </ul>
    </li>
    <a href="#common-patterns">Common Patterns</a>
  </ol>
</details>

## Anatomy of an Action-Graph
A graph is a structure amounting to a set of objects in which some pairs of the objects are in some sense "related". - Wikipedia 2021

An action-graph is a structure amounting to a set of objects in which some pairs of the objects are related by a  specified function. We have built a schema for specifying these graphs in json, as well as a set of utilities around describing this data structure in a usable fashion. By creating action-graphs, and then walking them, arbitrary programs can be run.

### Quick references
- <a href="https://github.com/goldenwitch/action-graphs/blob/main/schema.json">Schema</a>

### Graphs
The "Graph" type is a collection of nodes. Top level nodes in the nodes property MUST have unique ids within the collection they are specified in.

#### Nodes
Nodes represent the vertices in the graph. They have a unique id for addressing them from their parent graph, a collection of edges connecting them to other nodes, and a "value", some piece of data associated with this vertex.

Node ids MUST be unique in the graph containing the node, and it MUST be either a json string or a json integer.

##### Values
Node values MUST be either a json string, a json integer, or a valid declaration of an action-graph.
This means that graphs can contain nodes which can contain graphs. Extending graphs into graph hierarchies enables encapsulation(still relevant in graph world) and better semantic modeling.

If nodes contain a graph as a value, this "child" graph may duplicate node ids found outside of the child graph, including both the parent nodes id as well as inside neighboring nodes "child" graphs.

#### Hierarchical Graph Terminology
Since it will come up a lot from here on out, let's clarify some terminology.

- Top Level Graph
    - When graph nodes can contain graphs, the resulting structure looks something like a tree (A hierarchical graph). The graph which contains all of the other graphs we'll refer to as the "top level graph"
- Parent and Child Graphs
    - If a graph has a node with a graph as a value, it can be said to be the "parent" graph to it's node's graph. Similarly, the node's value that is a graph can be said to be the "child" graph.
- Graph Levels
    - If you are in a graph, and it has a parent that is not the top level graph, then it can be said that you are on a lower graph level than the top level graph. If you count the number of times that you can move to your parent before you are in the top level graph, it can be said that you are that many levels down the graph.
    - Going the other direction, if you start in the top level graph, and navigate to a child graph, you have gone down one level. If you navigate again, you could say you are two levels down the graph.

#### Contextual Node Targeting
This needs diagrams.

Creating edges across graph levels is challenging. To resolve this, action-graphs provides for contextual node targeting expressions. These expressions when evaluated point to a single, specific node, starting from a single, specific node.

These expressions in can be broken down into 3 parts {climb levels}[{descend levels}]{metadata}
For example, consider the valid targeting expression "..[node[childofnode]]console.text"
- To climb levels, use the character '.' Each one of these '.' represents climbing to the parent of the graph that is in scope.
    - In our example above, from the starting node, we go up two graph levels.
- After we have climbed levels, we then descend down to the specific node we want by targeting child nodes by id. We nest each target in brackets [].
- The metadata on the end of the targeting expression is used contextually based on what is being targeted. In the example above it refers to an action to run on the resulting targeted node.

Node ids that are integers should be preceded by a # in the targeting expression

#### Edges
Edges describe the connections between different nodes. Edges can connect nodes even if the nodes are inside of a graph that is the value of another node.

Edges MUST have a unique id within the node they are defined in.

##### Actions
The "Action" property of edges represents the action to run when the edge is walked. It MUST be either a valid targeting expression with the metadata component of that expression being the action to run, or just the action to run.

If the "Action" property contains a valid targeting expression, the "To" property must be omitted. If it is just the action being run the "To" property must included and must be a valid targeting expression. The action specified must map to an action defined in the "edge actions" passed in to the initiate creation of the action-graph.

##### Conditions
A condition compares two terms when an edge is walked. If the condition is not met, the edge does not get walked.

#### Events
An action-graph event consists of a graph targeting expression where the edge is the metadata, as well as an action to run when this edge is walked.

### Walkers
A graph walker represents an entity traveling the graph nodes with a cursor representing the curret node.

#### Expressions
The base walker accepts expressions as input representing which edges to travel as well as the "velocity" (a parameter for the edge) with which to walk the edge.

The expressions follow the format "edge=velocity;nextedge=nextvelocity;..."

## Common Patterns
    "But where do I start!?"
With these common patterns we can resolve some standard programming problems.
### Text input pattern
    "So I have all these graphs, what can I do with them? How do I let the user walk them?"
The most common way to interact with these graphs, is to feed your walker user input to let the user manually walk the graph. This is easy to do with a console application as the samples demonstrate, but it could also come from some text canvas like discord or messenger.
Our official recommendation is to sanitize these inputs and map them to the edges available at the current context.
### State node pattern
To represent complex state that has no interactions, you can generate subgraphs or nodes that have no edges. These can be targeted with targeting expressions as a part of various edges or events to update/output their value.
### Sub-graph pattern
By creating a node with a graph as a value, and exposing key edges at the top level node that themselves generate their own walker, you can create an encapsulated piece of functionality.

A simple example is the query edge. It assumes that you have a subgraph with various different possible inputs that the query would like to hit. By taking the input provided to the top level edge and then walking the query node
For example, let's say I have a graph that does multiplication in it's nodes, but I only want to expose exponentiation to outside callers. I can create an exponents edge on the node that contains this graph, that when walked, creates a walker that then walks the subgraph to generate the desired result.
#### Async sub-graph pattern
Following the same pattern described above, I can make a walker that fires off asynchronously, based on a timer or some other asynchronous trigger, which then walks the subgraph. This lets us resolve many of the common problems with asynchronous and parallel programming natively in idiomatic graphs.
### Event ticker pattern
Events can be used with the async sub-graph pattern to pump arbitrary data or action into the graph. For example, by configuring an event that checks when a clock node ticks, we can update various nodes based on how they progressed through time.