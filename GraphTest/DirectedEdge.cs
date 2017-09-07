using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GraphTest
{
    public class DirectedEdge
    {
        /// <summary>
        /// Origin node in the edge, occurs before the child node in the total topological ordering
        /// </summary>
        private TaskNode parent;
        public TaskNode Parent { get { return parent; } }

        private TaskNode child;
        public TaskNode Child { get { return child; } }

        /// <summary>
        /// Simulated communication cost 
        /// </summary>
        public double CommunicationCost { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float Weight { get { return parent.SimulatedExecutionTime + child.SimulatedExecutionTime; }}

        public DirectedEdge(TaskNode n1, TaskNode n2)
        {
            this.parent = n1;
            this.child = n2;
        }

        public override string ToString()
        {
            return "Edge from: " + parent.ToString() + " to: " + child.ToString();
        }
    }
}
