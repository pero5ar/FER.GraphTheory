using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FirstHomework
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Console.ReadLine().Trim();
            var inputStream = new StreamReader(path);

            var N = int.Parse(inputStream.ReadLine());
            inputStream.ReadLine();
            var adjacencyMatrix = new List<List<int>>(N);
            for (var i = 0; i < N; i++)
            {
                adjacencyMatrix.Add(inputStream.ReadLine().Trim().Split(' ').Select(int.Parse).ToList());
            }
            var initialGraph = new Graph(adjacencyMatrix);


            ulong spanningTreeNumber = 0;
            Graph spanningTree = null;
            
            var isSpanningTree = initialGraph.E == N - 1;
            var isComplete = initialGraph.E == (N * (N - 1) / 2);

            if (!initialGraph.IsConnected())
            {
                spanningTree = new Graph(); // empty graph
            }
            else if (isSpanningTree)
            {
                spanningTreeNumber = 1;
                spanningTree = initialGraph;
            }
            else if (isComplete)
            {
                spanningTreeNumber = Convert.ToUInt64(Math.Pow(N, N - 2));
                spanningTree = initialGraph.GetSpanningTree();
            }
            else
            {
                spanningTreeNumber = DeletionContractionRecurrence(initialGraph);
                spanningTree = initialGraph.GetSpanningTree();
            }

            Console.WriteLine(spanningTreeNumber);
            spanningTree.Print();
        }

        static ulong DeletionContractionRecurrence(Graph graph)
        {
            if (graph.V == 1)
            {
                // any leftover edges are loops that would just get deleted
                return 1;
            }

            Graph deletionGraph = null;
            Graph contractionGraph = null;
            Tuple<int, int> chosenEdge = null;
            var nonLoopEdges = graph.Edges.Where(pair => pair.Item1 != pair.Item2).ToList();

            if (nonLoopEdges.Count == 0)
            {
                // all edges are loops, all further operations would just delete the loops until no edges
                return 1;
            }

            foreach (var edge in nonLoopEdges)
            {
                deletionGraph = Graph.ConstructByDeletion(graph, edge);
                if (deletionGraph.IsConnected())
                {
                    // edge is not a cut-edge
                    chosenEdge = edge;
                    break;
                }
            }

            if (chosenEdge == null)
            {
                // all edges are cut-edges or loops, so just contract one cut-edge
                contractionGraph = Graph.ConstructByDeletion(graph, nonLoopEdges[0]);
                return DeletionContractionRecurrence(contractionGraph);
            }

            // edge is not a loop or cut-edge
            contractionGraph = Graph.ConstructByContraction(graph, chosenEdge);
            return DeletionContractionRecurrence(deletionGraph) + DeletionContractionRecurrence(contractionGraph);
        }
    }

    class Graph
    {
        public readonly int V;
        public readonly int E;

        public List<List<int>> AdjacencyMatrix { get; private set; }
        public List<Tuple<int, int>> Edges { get; private set; }

        public Graph()
        {
            AdjacencyMatrix = new List<List<int>>();
            V = 0;
            Edges = new List<Tuple<int, int>>();
            E = 0;
        }

        public Graph(List<List<int>> adjacencyMatrix)
        {
            AdjacencyMatrix = adjacencyMatrix;
            V = adjacencyMatrix.Count;
            E = CountEdges();
        }

        public Graph(List<List<int>> adjacencyMatrix, List<Tuple<int, int>> edges)
        {
            AdjacencyMatrix = adjacencyMatrix;
            V = adjacencyMatrix.Count;
            Edges = edges;
            E = edges.Count;
        }

        public Graph(List<Tuple<int, int>> edges)
        {
            Edges = edges;
            E = edges.Count;
            var vertices = edges.SelectMany(pair => new List<int>() { pair.Item1, pair.Item2 }).Distinct().ToList();
            V = vertices.Count;
            AdjacencyMatrix = Enumerable.Range(0, V).Select(_ => Enumerable.Repeat(0, V).ToList()).ToList();
            foreach (var edge in edges)
            {
                AdjacencyMatrix[edge.Item1][edge.Item2] = 1;
                AdjacencyMatrix[edge.Item2][edge.Item1] = 1;
            }
        }

        private int CountEdges()
        {
            var connected = 0;
            Edges = new List<Tuple<int, int>>();
            for (var i = 0; i < V; i++)
            {
                for (var j = i; j < V; j++)
                {
                    connected += AdjacencyMatrix[i][j];
                    for (var k = 0; k < AdjacencyMatrix[i][j]; k++)
                    {
                        Edges.Add(new Tuple<int, int>(i, j));  // NOTE: i <= j (Item2 >= Item1)
                    }
                }
            }
            return connected;
        }

        public bool IsConnected()
        {
            if (V - 1 > E)
            {
                return false;
            }
            var visited = new HashSet<int> { Edges[0].Item1, Edges[0].Item2 };
            while (visited.Count < V)
            {
                var visitedNew = false;
                foreach (var edge in Edges)
                {
                    var contains = new Tuple<bool, bool>(visited.Contains(edge.Item1), visited.Contains(edge.Item2));
                    if (contains.Item1 == contains.Item2) continue; // contains both or neither
                    if (contains.Item1) visited.Add(edge.Item2);
                    if (contains.Item2) visited.Add(edge.Item1);
                    visitedNew = true;
                }
                if (!visitedNew)
                {
                    return false;
                }
            }
            return true;
        }

        public void Print()
        {
            foreach (var row in AdjacencyMatrix)
            {
                foreach (var element in row) Console.Write(element + " ");
                Console.WriteLine();
            }
        }

        public static Graph ConstructByDeletion(Graph graph, Tuple<int, int> edge)
        {
            var adjacencyMatrix = graph.AdjacencyMatrix.Select(row => new List<int>(row)).ToList();
            var edges = graph.Edges.Select(pair => new Tuple<int, int>(pair.Item1, pair.Item2)).ToList();
            
            adjacencyMatrix[edge.Item1][edge.Item2]--;
            if (edge.Item1 != edge.Item2)
            {
                // edge is not a loop
                adjacencyMatrix[edge.Item2][edge.Item1]--;
            }
            edges.Remove(edge);

            return new Graph(adjacencyMatrix, edges);
        }

        public static Graph ConstructByContraction(Graph graph, Tuple<int, int> edge)
        {
            if (edge.Item1 == edge.Item2)
            {
                throw new ArgumentException("Cannot contract a loop");
            }
            var adjacencyMatrix = graph.AdjacencyMatrix.Select(row => new List<int>(row)).ToList();

            // Construct contracted row:
            var contracted = Enumerable.Zip(adjacencyMatrix[edge.Item1], adjacencyMatrix[edge.Item2], (u, v) => u + v).ToList();
            contracted.RemoveAt(edge.Item2);
            contracted.RemoveAt(edge.Item1);
            contracted.Add(adjacencyMatrix[edge.Item1][edge.Item2] - 1);

            // Remove Rows:
            adjacencyMatrix.RemoveAt(edge.Item2);
            adjacencyMatrix.RemoveAt(edge.Item1);

            // Remove Columns:
            foreach (var row in adjacencyMatrix)
            {
                row.RemoveAt(edge.Item2);
                row.RemoveAt(edge.Item1);
            }

            // Add contracted column (without last element)
            for (var i = 0; i < adjacencyMatrix.Count; i++)
            {
                adjacencyMatrix[i].Add(contracted[i]);
            }
            adjacencyMatrix.Add(contracted);

            return new Graph(adjacencyMatrix);
        }

        public Graph GetSpanningTree()
        {
            if (V - 1 > E)
            {
                return new Graph();
            }
            var visited = new HashSet<int> { Edges[0].Item1, Edges[0].Item2 };
            var edges = new List<Tuple<int, int>>(V - 1) { Edges[0] };
            while (visited.Count < V)
            {
                var visitedNew = false;
                foreach (var edge in Edges)
                {
                    var contains = new Tuple<bool, bool>(visited.Contains(edge.Item1), visited.Contains(edge.Item2));
                    if (contains.Item1 == contains.Item2) continue; // contains both or neither
                    if (contains.Item1) visited.Add(edge.Item2);
                    if (contains.Item2) visited.Add(edge.Item1);
                    edges.Add(edge);
                    visitedNew = true;
                }
                if (!visitedNew)
                {
                    return new Graph();
                }
            }
            return new Graph(edges);
        }
    }
}
