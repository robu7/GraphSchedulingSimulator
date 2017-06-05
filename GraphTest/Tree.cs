using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest
{
    class Tree
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }

        public Tree()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }


        public void AddNodes(List<Node> newNodes)
        {
            Nodes.AddRange(newNodes);
        }

        public void CreateEdge(Node parent, Node child)
        {
            Edges.Add(new Edge(parent, child));
            parent.AddChild(child);
            child.AddParent(parent);
        }

        public List<Node> TopologicalSort()
        {
            return Nodes;
        }

        public void ComputeTLevel()
        {
            foreach (var node in Nodes)
            {
                node.ComputeTLevel();
            }
        }

        public void ComputeSLevel()
        {
            var RevTopList = Nodes;
            RevTopList.Reverse();

            foreach (var node in RevTopList)
            {
                node.ComputeSLevel();
            }
        }

        public List<Node> SortBySLevel()
        {
            return Nodes.OrderByDescending(x => x.slLevel).ToList();
        }

        public List<Node> SortByTLevel()
        {
            return Nodes.OrderBy(x => x.tLevel).ToList();
        }

        public List<Node> SortByID()
        {
            return Nodes.OrderBy(x => x.ID).ToList();
        }

        /// <summary>
        /// A function to randomly generate a weighted DAG tree.
        /// Ranks represent different levels in the graph,
        /// edges can only be drawn from lower levels to higher.
        /// This will prevent cycles.
        /// 
        /// </summary>
        /// <returns></returns>
        public static Tree GenerateRandomWeightedDAG()
        {
            Tree DAGgraph = new Tree();
            Random randomGenerator = new Random();
            List<Node> newNodes = new List<Node>();
            List<Node> oldNodes = new List<Node>();

            int numberOfRanks = randomGenerator.Next(Settings.minRanks, Settings.maxRanks);
            //int numberOfRanks = maxRanks;
            int numberOfNewNodes;

            for (int i = 0; i < numberOfRanks; i++)
            {
                newNodes.Clear();

                /*
                 * Generate new nodes
                 */
                if (i == 0)
                    numberOfNewNodes = 1;
                else
                    numberOfNewNodes = randomGenerator.Next(Settings.minWidth, Settings.maxWidth);
                for (int n = 0; n < numberOfNewNodes; n++)
                {
                    newNodes.Add(new Node(DAGgraph.Nodes.Count + 1 + n, randomGenerator.Next(Settings.minSimTime, Settings.maxSimTime)));
                }

                /*
                 * Generate edges from old nodes to new
                 */
                foreach (var oldNode in oldNodes)
                {
                    foreach (var newNode in newNodes)
                    {
                        if (randomGenerator.Next(100) < Settings.changeOfEdge)
                        {
                            DAGgraph.CreateEdge(oldNode, newNode);
                        }
                    }
                }

                // Add new nodes to tree
                oldNodes = newNodes.ToList();
                DAGgraph.AddNodes(newNodes);
            }

            return DAGgraph;
        }

        public static Tree GenerateTestGraph()
        {
            Tree DAGgraph = new Tree();

            List<Node> nodes = new List<Node>();

            nodes.Add(new Node(1, 2000));

            nodes.Add(new Node(2, 2000));
            nodes.Add(new Node(3, 2000));
            nodes.Add(new Node(4, 1500));
            nodes.Add(new Node(5, 500));
            nodes.Add(new Node(6, 500));

            nodes.Add(new Node(7, 1000));
            nodes.Add(new Node(8, 1500));
            nodes.Add(new Node(9, 2000));

            nodes.Add(new Node(10, 2000));

            // Edges from node 1
            for (int i = 0; i < 5; i++)
            {
                DAGgraph.CreateEdge(nodes[0], nodes[i + 1]);
                //nodes[i + 1].AddDependency(nodes[0]);
            }

            // Edges from node 2
            DAGgraph.CreateEdge(nodes[1], nodes[7]);
            DAGgraph.CreateEdge(nodes[1], nodes[8]);


            // Edges from node 3
            DAGgraph.CreateEdge(nodes[2], nodes[6]);

            // Edges from node 4
            DAGgraph.CreateEdge(nodes[3], nodes[7]);
            DAGgraph.CreateEdge(nodes[3], nodes[8]);

            // Edges from node 5
            DAGgraph.CreateEdge(nodes[4], nodes[8]);

            // Edges from node 6
            DAGgraph.CreateEdge(nodes[5], nodes[7]);

            // Edges from node 7
            DAGgraph.CreateEdge(nodes[6], nodes[9]);

            // Edges from node 8
            DAGgraph.CreateEdge(nodes[7], nodes[9]);

            // Edges from node 8
            DAGgraph.CreateEdge(nodes[8], nodes[9]);

            DAGgraph.AddNodes(nodes);
            return DAGgraph;

        }

        public override string ToString()
        {
            StringBuilder graph = new StringBuilder();
            foreach (var item in Edges)
            {
                graph.Append(item.ToString());
            }
            
            return graph.ToString();
        }

    }
}
