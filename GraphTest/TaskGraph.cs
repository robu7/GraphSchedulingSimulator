using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest
{
    class TaskGraph
    {
        public List<TaskNode> Nodes { get; set; }
        public List<DirectedEdge> Edges { get; set; }

        public TaskGraph()
        {
            Nodes = new List<TaskNode>();
            Edges = new List<DirectedEdge>();
        }

        public void ResetTasks()
        {
            foreach (var node in Nodes) {
                node.ReadySignal.Reset();
            }
        }


        public void AddNodes(List<TaskNode> newNodes)
        {
            Nodes.AddRange(newNodes);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateEdge(TaskNode parent, TaskNode child)
        {
            Edges.Add(new DirectedEdge(parent, child));
            parent.AddChild(child);
            child.AddParent(parent);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ComputeTLevel()
        {
            foreach (var node in Nodes)
            {
                node.ComputeTLevel();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ComputeSLevel()
        {
            var RevTopList = Nodes;
            RevTopList.Reverse();

            foreach (var node in RevTopList)
            {
                node.ComputeSLevel();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<TaskNode> SortBySLevel()
        {
            return Nodes.OrderByDescending(x => x.slLevel).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        public List<TaskNode> SortByTLevel()
        {
            return Nodes.OrderBy(x => x.tLevel).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TaskNode> SortByID()
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
        public static TaskGraph GenerateRandomWeightedDAG()
        {
            TaskGraph DAGgraph = new TaskGraph();
            Random randomGenerator = new Random();
            List<TaskNode> newNodes = new List<TaskNode>();
            List<TaskNode> oldNodes = new List<TaskNode>();

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
                    newNodes.Add(new TaskNode(DAGgraph.Nodes.Count + 1 + n, randomGenerator.Next(Settings.minSimTime, Settings.maxSimTime)));
                }

                /*
                 * Generate edges from old nodes to new
                 */
                foreach (var oldNode in DAGgraph.Nodes)
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
                //oldNodes = newNodes.ToList();
                DAGgraph.AddNodes(newNodes);
            }

            return DAGgraph;
        }

        /// <summary>
        /// Generate a standard tree which can be used for testing
        /// </summary>
        public static TaskGraph GenerateTestGraph()
        {
            TaskGraph DAGgraph = new TaskGraph();

            List<TaskNode> nodes = new List<TaskNode>();

            nodes.Add(new TaskNode(1, 2000));

            nodes.Add(new TaskNode(2, 2000));
            nodes.Add(new TaskNode(3, 2000));
            nodes.Add(new TaskNode(4, 1500));
            nodes.Add(new TaskNode(5, 500));
            nodes.Add(new TaskNode(6, 500));

            nodes.Add(new TaskNode(7, 1000));
            nodes.Add(new TaskNode(8, 1500));
            nodes.Add(new TaskNode(9, 2000));

            nodes.Add(new TaskNode(10, 2000));

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
        
        /// <summary>
        /// Print all nodes in the tree
        /// </summary>
        public void PrintTree()
        {
            foreach (var node in Nodes)
            {
                node.PrintNodeInfo();
            }
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
