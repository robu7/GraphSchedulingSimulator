using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace GraphTest
{
    public class Node
    {
        public int ID { get; }
        public bool IsEntryNode { get; set; }
        private List<Node> parentNodes;
        public List<Node> ParentNodes { get { return parentNodes; } }
        private List<Node> childNodes;
        public List<Node> ChildNodes { get { return childNodes; } }
        public int SimulatedExecutionTime { get; set; }

        public bool IsDone { get; set; }
        public bool IsScheduled { get; set; } = false;
        private bool isCriticalNode;

        public int? tLevel { get; private set; }
        public int? slLevel { get; private set; }

        public bool AreParentsDone {
            get {
                foreach (var item in parentNodes)
                {
                    if(!item.IsDone)
                    {
                        return false;
                    }
                }
                return true;
            }}

        public bool ReadyToSchedule
        {
            get
            {
                foreach (var item in parentNodes)
                {
                    if (!item.IsScheduled)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        // Maybe contain a ChangeListInfo object?

        public Node()
        {
            parentNodes = new List<Node>();
            childNodes = new List<Node>();
            tLevel = null;
            slLevel = null;
            IsEntryNode = true;
        }

        public Node(int id,int executionTime)
        {
            ID = id;
            SimulatedExecutionTime = executionTime;
            parentNodes = new List<Node>();
            childNodes = new List<Node>();
            tLevel = null;
            slLevel = null;
            IsDone = false;
            //IsEntryNode = true;
        }


        public int ComputeTLevel()
        {
            if (tLevel != null)
                return (int)tLevel;

            int max = 0;
            int value = 0;

            foreach (var item in parentNodes)
            {
                value = (item.ComputeTLevel() + item.SimulatedExecutionTime / 100);
                if (value > max)
                    max = value;
            }
            //Console.WriteLine("T-Level:" + max + " " +(parentNodes.Count == 0 ? "entry" : "") + " ID:" + ID);
            tLevel = max;
            return max;
        }

        internal int ComputeSLevel()
        {

            if (slLevel != null)
                return (int)slLevel;

            int max = 0;
            int value = 0;

            if (childNodes.Count == 0)
                slLevel = SimulatedExecutionTime / 100;
            else
                foreach (var child in childNodes)
                {
                    value = child.ComputeSLevel();
                    if (value > max)
                        max = value;
                }

            slLevel = max + SimulatedExecutionTime / 100;
            //Console.WriteLine("SL-Level: " + slLevel + " " +(childNodes.Count == 0 ? "end" : "") + " ID:" + ID);
            return (int)slLevel;
        }

        public void Execute()
        {
            Thread.Sleep(SimulatedExecutionTime);
        }

        internal void AddDependency(Node parent)
        {
            parentNodes.Add(parent);
            parent.AddParent(this);
        }

        internal void AddParent(Node parent)
        {
            parentNodes.Add(parent);
        }

        internal void AddChild(Node child)
        {
            childNodes.Add(child);
        }

        public void PrintNodeInfo()
        {
            Console.Write("Node ID: " + ID + "\t Parents: " + parentNodes.Count + "\t Children: " + childNodes.Count + "\t t-level: " + tLevel + "\t sl-level: " + slLevel + "\t Simulated time: " + SimulatedExecutionTime/100 + "\t Parent IDs: {");
            parentNodes.ForEach(x => Console.Write(x.ToString() + ","));
            //Console.Write("} \t Children: {");
            //childNodes.ForEach(x => Console.Write(x.ToString() + ","));
            Console.WriteLine("}");
        }


        public void LogNodeInfo()
        {
            StringBuilder logString = new StringBuilder();
            logString.Append("Node ID: " + ID + "\t Parents: " + parentNodes.Count + "\t Children: " + childNodes.Count + "\t t-level: " + tLevel + "\t sl-level: " + slLevel + "\t Work load: " + SimulatedExecutionTime / 100 + "\t Parent IDs: {");
            //Console.Write("Node ID: " + ID + "\t Parents: " + parentNodes.Count + "\t Children: " + childNodes.Count + "\t t-level: " + tLevel + "\t sl-level: " + slLevel + "\t Simulated time: " + SimulatedExecutionTime/100 + "\t Parent IDs: {");
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

    }
}
