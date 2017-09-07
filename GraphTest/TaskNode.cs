using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace GraphTest
{
    public static class ListCloner
    {
        public static List<TaskNode> Clone(this List<TaskNode> listToClone , Dictionary<int, TaskNode> visited)
        {
            return listToClone!=null? listToClone.Select(item => item?.Clone(visited)).ToList(): new List<TaskNode>();
        }
    }


    public enum BuildStatus { None, Scheduled, Executed}

    /// <summary>
    /// This class represents a graph of build tasks. 
    /// The graph is a directed acyclic graph (DAG) where each node is a build task 
    /// and edges represents precedence contraints between tasks. 
    /// </summary>
    public class TaskNode
    {
        public int ID { get; }
        public BuildStatus Status { get; set; } = BuildStatus.None;
        public ManualResetEvent ReadySignal { get; private set; }

        private List<TaskNode> parentNodes;
        public List<TaskNode> ParentNodes { get { return parentNodes; } }
        private List<TaskNode> childNodes;
        public List<TaskNode> ChildNodes { get { return childNodes; } }

        public int executionTime;
        public int SimulatedExecutionTime { get { return executionTime; }
            set {
                executionTime = value;
                //executionTime = (int)Math.Round(value / 1000.0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            earliestStartTime = 0;
            finishTime = 0;
            Status = BuildStatus.None;
            ReadySignal.Reset();
        }


        /// <summary>
        /// 
        /// </summary>
        private int finishTime;
        public int FinishTime {
            get {
                return finishTime;
            }
            set { finishTime = value;  }
        }
        public void UpdateFinishTime()
        {
            FinishTime = parentNodes.Count > 0 ? parentNodes.Max(x => x.EarliestStartTime)+SimulatedExecutionTime : 0;
        }

        /// <summary>
        /// 
        /// </summary>
        private int earliestStartTime;
        public int EarliestStartTime {
            get {
                UpdateEST();
                return earliestStartTime;
            } private set { earliestStartTime = value;  } }

        public void ResetEST()
        {
            FinishTime = 0;
            UpdateEST();
            //EarliestStartTime = (int)tLevel;
        }
        public void UpdateEST()
        {
            EarliestStartTime = parentNodes.Count > 0 ? parentNodes.Max(x => x.FinishTime) : 0;
            FinishTime = earliestStartTime + SimulatedExecutionTime;
        }

        public int Level {
            get {
                if (childNodes.Count == 0)
                    return SimulatedExecutionTime;
                else
                    return childNodes.Max(x => x.Level)+ SimulatedExecutionTime;
            } }


        public int? tLevel { get; set; }
        public int? slLevel { get; private set; }

        // old, switch to WaitForParentsToFinish
        public bool IsReadyToExecute {
            get {
                foreach (var item in parentNodes)
                {
                    if(item.Status != BuildStatus.Executed)
                    {
                        return false;
                    }
                }
                return true;
            }}

        public TaskNode()
        {
            parentNodes = new List<TaskNode>();
            childNodes = new List<TaskNode>();
            tLevel = null;
            slLevel = null;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        private TaskNode(TaskNode prevTask, Dictionary<int, TaskNode> visited)
        {
            ID = prevTask.ID;
            visited.Add(ID, this);
            SimulatedExecutionTime = prevTask.SimulatedExecutionTime;
            parentNodes = prevTask.ParentNodes.Clone(visited);
            childNodes = prevTask.ChildNodes.Clone(visited);
            tLevel = prevTask.tLevel;
            slLevel = prevTask.slLevel;
            Status = BuildStatus.None;
            ReadySignal = new ManualResetEvent(false);
        }

        public TaskNode(int id,int executionTime)
        {
            ID = id;
            SimulatedExecutionTime = executionTime;
            parentNodes = new List<TaskNode>();
            childNodes = new List<TaskNode>();
            tLevel = null;
            slLevel = null;
            ReadySignal = new ManualResetEvent(false);
        }

        /// <summary>
        /// Wait until all parents are done
        /// </summary>
        public void WaitForParentsToFinish()
        {
            var parentSignals = GetParentSignals().ToArray();
            if (parentSignals.ToArray() == null) {
                Console.WriteLine("Task {ID} is null");
                return;
            }
            if (parentSignals.Length > 0) {
                WaitHandle.WaitAll(parentSignals.ToArray());
            }
        }

        /// <summary>
        /// Assemble an array of parents readySignals
        /// </summary>
        private IEnumerable<ManualResetEvent> GetParentSignals()
        {
            foreach (var parent in ParentNodes) {
                yield return parent.ReadySignal;
            }

            yield break;
        }

        /// <summary>
        /// Check if all parents have been scheduled
        /// </summary>
        public bool IsReadyToSchedule {
            get {
                foreach (var item in parentNodes) {
                    if (item.Status != BuildStatus.Scheduled) {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// This function calculates the top level of a node, here named TLevel.
        /// The top level of a node is the combined weight of all nodes along the longest path
        /// from an entry node to this node(excluding the weight of this node).
        /// </summary>
        public int ComputeTLevel()
        {
            if (tLevel != null)
                return (int)tLevel;

            int max = 0;
            int value = 0;

            foreach (var item in parentNodes)
            {
                value = (item.ComputeTLevel() + item.SimulatedExecutionTime);
                if (value > max)
                    max = value;
            }
            //Console.WriteLine("T-Level:" + max + " " +(parentNodes.Count == 0 ? "entry" : "") + " ID:" + ID);
            tLevel = max;
            return max;
        }

        /// <summary>
        /// This function calculates the static level of a node, here named SLevel.
        /// The static level of a node is the combined weight of all nodes along the longest path
        /// from this node to an exit node(excluding edge weigths).
        /// </summary>
        public int ComputeSLevel()
        {

            if (slLevel != null)
                return (int)slLevel;

            int max = 0;
            int value = 0;

            if (childNodes.Count == 0)
                slLevel = SimulatedExecutionTime;
            else
                foreach (var child in childNodes)
                {
                    value = child.ComputeSLevel();
                    if (value > max)
                        max = value;
                }

            slLevel = max + SimulatedExecutionTime;
            //Console.WriteLine("SL-Level: " + slLevel + " " +(childNodes.Count == 0 ? "end" : "") + " ID:" + ID);
            return (int)slLevel;
        }

        /// <summary>
        /// Function which simulates actual work of the task
        /// </summary>
        public void Execute()
        {
            Thread.Sleep(SimulatedExecutionTime);
            ReadySignal.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddParent(TaskNode parent)
        {
            parentNodes.Add(parent);
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddChild(TaskNode child)
        {
            childNodes.Add(child);
        }

        /// <summary>
        /// Prints all relvevant information about the node
        /// </summary>
        public void PrintNodeInfo()
        {
            Console.Write("Node ID: " + ID + "\t Parents: " + parentNodes.Count + "\t Children: " + childNodes.Count + "\t t-level: " + tLevel + "\t sl-level: " + slLevel + "\t Work load: " + SimulatedExecutionTime + "\t Parent IDs: {");
            parentNodes.ForEach(x => Console.Write(x.ToString() + ","));
            //Console.Write("} \t Children: {");
            //childNodes.ForEach(x => Console.Write(x.ToString() + ","));
            Console.WriteLine("}");
        }

        /// <summary>
        /// Saves relevant node information to a file
        /// </summary>
        public void LogNodeInfo()
        {
            StringBuilder logString = new StringBuilder();
            logString.Append("Node ID: " + ID + "\t Parents: " + parentNodes.Count + "\t Children: " + childNodes.Count + "\t t-level: " + tLevel + "\t sl-level: " + slLevel + "\t Work load: " + SimulatedExecutionTime + "\t Parent IDs: {");
            //Console.Write("Node ID: " + ID + "\t Parents: " + parentNodes.Count + "\t Children: " + childNodes.Count + "\t t-level: " + tLevel + "\t sl-level: " + slLevel + "\t Simulated time: " + SimulatedExecutionTime + "\t Parent IDs: {");
            parentNodes.ForEach(x => logString.Append(x.ToString() + ","));
            //Console.Write("} \t Children: {");
            //childNodes.ForEach(x => Console.Write(x.ToString() + ","));
            logString.AppendLine("}");

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Program.Log(logString.ToString(), w);
            }

        }

        public override string ToString()
        {
            return ID.ToString();
        }

        public TaskNode Clone(Dictionary<int, TaskNode> visited)
        {
            if(visited.ContainsKey(this.ID)) {
                return visited[this.ID];
            }
            else {
                var taskCopy = new TaskNode(this, visited);
                //visited.Add(taskCopy.ID, taskCopy);
                return taskCopy;
            }
        }
    }
}
