using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace GraphTest
{
    public struct Graph
    {
        public Vertex[] Nodes;
        public Link[] Links;

        public Graph(List<TaskNode> nodes, List<DirectedEdge> edges)
        {
            Nodes = new Vertex[nodes.Count];
            Links = new Link[edges.Count];

            int i = 0;
            foreach (var node in nodes)
            {
                Nodes[i] = new Vertex(node.ID.ToString(), node.ID.ToString());
                ++i;
            }

            i = 0;
            foreach (var edge in edges)
            {
                Links[i] = new Link(edge.Parent.ToString(), edge.Child.ToString());
                ++i;
            }
        }
    }

    public struct Link
    {
        [XmlAttribute]
        public string Source;
        [XmlAttribute]
        public string Target;
        [XmlAttribute]
        public string Label;

        public Link(string source, string target, string label = "")
        {
            this.Source = source;
            this.Target = target;
            this.Label = label;
        }
    }

    public struct Vertex
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Label;

        public Vertex(string id, string label)
        {
            this.Id = id;
            this.Label = label;
        }
    }

    class DGMLWriter
    {       
        public void Serialize(TaskGraph tree, string xmlPath = "tree.dgml")
        {
            Graph graph = new Graph(tree.Nodes, tree.Edges);

            XmlRootAttribute root = new XmlRootAttribute("DirectedGraph");
            root.Namespace = "http://schemas.microsoft.com/vs/2009/dgml";
            XmlSerializer serializer = new XmlSerializer(typeof(Graph), root);
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(xmlPath, settings);
            serializer.Serialize(xmlWriter, graph);

        }
    }
}
