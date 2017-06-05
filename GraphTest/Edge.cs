using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GraphTest
{
    public class Edge
    {
        private Node parent;
        public Node Parent { get { return parent; } }

        private Node child;
        public Node Child { get { return child; } }

        public double CommunicationCost { get; set; }

        public float Weight { get { return parent.SimulatedExecutionTime + child.SimulatedExecutionTime; }}

        public Edge(Node n1, Node n2)
        {
            parent = n1;
            child = n2;
            child.IsEntryNode = false;
        }

        public override string ToString()
        {
            return "Edge from: " + parent.ToString() + " to: " + child.ToString();
        }
    }
}
