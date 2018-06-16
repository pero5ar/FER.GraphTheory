using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecondHomework
{
    class Program
    {
        const int YES = 1;
        const int NO = 0;

        static Graph graph = null;
        static Dictionary<int, List<Tuple<int, int>>> coloredEdgesDict = null;

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
            
            graph = new Graph(adjacencyMatrix);
            
            if (graph.IsRegular())
            {
                var hasEvenN = N % 2 == 0;
                var isComplete = graph.E == (N * (N - 1) / 2);

                if (!hasEvenN)
                {
                    // not 1-factorable, see: https://en.wikipedia.org/wiki/Graph_factorization#1-factorization
                    // also: https://math.stackexchange.com/questions/212073/graph-theory-prove-k-regular-graph-v-odd-chig-k
                    Console.WriteLine(NO);
                    return;
                }
                if (isComplete)
                {
                    // http://www.fer.unizg.hr/_download/repository/3_Bojanja.pdf str 15, TM 2.2
                    // also: https://en.wikipedia.org/wiki/Graph_factorization#1-factorization
                    var resultForK = hasEvenN ? YES : NO;
                    Console.WriteLine(resultForK);
                    return;
                }
            }

            coloredEdgesDict = new Dictionary<int, List<Tuple<int, int>>>();
            for (var color = 0; color < graph.Delta; color++) coloredEdgesDict.Add(color, new List<Tuple<int, int>>());

            var result = IsDeltaEdgeColored(0) ? YES : NO;
            Console.WriteLine(result);
        }

        static bool IsDeltaEdgeColored(int edgeIndex)
        {
            if (edgeIndex == graph.E)
            {
                return true;
            }
            var edge = graph.Edges[edgeIndex];
            var incidentEdges = graph.GetIncidentEdges(edge);
            for (var color = 0; color < graph.Delta; color++)
            {
                if (!coloredEdgesDict[color].Any(_e => incidentEdges.Contains(_e)))
                {
                    coloredEdgesDict[color].Add(edge);
                    if (IsDeltaEdgeColored(edgeIndex + 1)) return true;
                    else coloredEdgesDict[color].Remove(edge);
                }
            }
            return false;
        }
    }

    class Graph
    {
        public readonly int V;
        public readonly int E;
        public readonly int Delta;

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
            Delta = FindMaximumDegree();
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
                        Edges.Add(new Tuple<int, int>(i, j));  // NOTE: i <= j (Item1 <= Item2)
                    }
                }
            }
            return connected;
        }

        public List<Tuple<int, int>> GetIncidentEdges(Tuple<int, int> edge)
            => Edges.Where(_e => _e != edge && (_e.Item1 == edge.Item1
                                                || _e.Item2 == edge.Item1
                                                || _e.Item1 == edge.Item2
                                                || _e.Item2 == edge.Item2
                )).ToList();

        private int FindMaximumDegree() => AdjacencyMatrix.Select(row => row.Sum()).Max();

        public bool IsRegular() => AdjacencyMatrix.Select(row => row.Sum()).All(deg => deg == Delta);
        
        // NOT NEEDED:

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
    }
}
