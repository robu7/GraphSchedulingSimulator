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


        /// <summary>
        /// 
        /// </summary>
        public void ResetNodes()
        {
            foreach (var node in Nodes) {
                node.Reset();
            }
        }

        public void AddNodes(List<TaskNode> newNodes)
        {
            Nodes.AddRange(newNodes);
        }

        public void AddNode(TaskNode newNode)
        {
            Nodes.Add(newNode);
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
        public void CreateEdge(int parentID, int childID)
        {
            TaskNode parent = Nodes.Find(x => x.ID == parentID);
            TaskNode child = Nodes.Find(x => x.ID == childID);
            CreateEdge(parent, child);
        }


        /// <summary>
        /// Writes the graph to a .gv file with the dot format
        /// Use one of these commands to create picture with the graph:
        ///     1. tred graph.gv | dot -T png > graph.png
        ///     2. dot -T png graph.gv > graph.png
        /// </summary>
        public void PrintImage()
        {
            StringBuilder dotFormat = new StringBuilder();

            dotFormat.AppendLine();
            dotFormat.Append("digraph G {");

            foreach (var node in Nodes) {
                    dotFormat.AppendLine("\t" + node.ID + " [label=\"" + node.ID + "| " + node.SimulatedExecutionTime/100 + "\"]" + ";");
            }
            foreach (var edge in Edges) {
                dotFormat.AppendLine("\t" + edge.Parent.ID + " -> " + edge.Child.ID + ";");
            }

            dotFormat.Append("}");

            System.IO.File.WriteAllText(@"graph.gv", dotFormat.ToString());

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //startInfo.WorkingDirectory = @"C:\Users\robda\Documents\graphviz-2.38\";
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C tred graph.gv | dot -T png > graph.png";
            process.StartInfo = startInfo;
            process.Start();

            // TODO: create a process which runs the create image command
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


        public static TaskGraph CreateGraphFromFile()
        {
            string[] lines = System.IO.File.ReadAllLines(@"rand3500.stg");
            TaskGraph loadedGraph = new TaskGraph();

            Dictionary<TaskNode, IEnumerable<string>> dependencies = new Dictionary<TaskNode, IEnumerable<string>>();

            for (int i = 1; i < lines.Count(); i++) {
                var line = lines[i];

                if (line.StartsWith("#"))
                    break;

                var lineItems = line.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);

                int ID = 0;
                int weight = 0;
                if (!int.TryParse(lineItems[0], out ID)) {
                    Console.WriteLine("Wrong graph format, ID cannot be parsed");
                }
                if (!int.TryParse(lineItems[1], out weight)) {
                    Console.WriteLine("Wrong graph format, Weight cannot be parsed");
                }

                var predecessors = lineItems.Skip(3).ToArray();
                //foreach (var item in predecessors) {
                //    Console.WriteLine(item);
                //}
                var newTaskNode = new TaskNode(ID, weight*10);

                dependencies[newTaskNode] = predecessors;
                loadedGraph.AddNode(newTaskNode);
            }

            foreach (var taskDependencies in dependencies) {
                foreach (var dependency in taskDependencies.Value) {
                    int predecessor = 0;
                    if (!int.TryParse(dependency, out predecessor)) {
                        Console.WriteLine("Wrong graph format, Predecessor cannot be parsed");
                    }
                    loadedGraph.CreateEdge(predecessor, taskDependencies.Key.ID);
                }
            }


            return loadedGraph;
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
