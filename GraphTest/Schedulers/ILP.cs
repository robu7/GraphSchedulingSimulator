using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest.Schedulers
{
    /// <summary>
    /// This scheduler finds the most optimal schedule 
    /// </summary>
    class ILP
    {
        public ILP(TaskGraph graph, int? maxThreadCount = null)
        {}






    }
}










//using Microsoft.SolverFoundation.Services;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ILPTest
//{
//    class Node
//    {
//        private static int id_counter = 0;
//        public int ID { get; set; }
//        public double Weight { get; set; }
//        public int[] Dependencies { get; set; }

//        public Node(int weight)
//        {
//            this.ID = id_counter++;
//            this.Weight = weight;
//        }
//    }
//    class Link
//    {
//        public int Source { get; set; }
//        public int Parent { get; set; }
//        public int isDependent { get; set; }

//        public Link(int task, int parent, int isDepen)
//        {
//            Source = task;
//            Parent = parent;
//            isDependent = isDepen;
//        }
//    }

//    class Worker
//    {
//        private static int id_counter = 0;
//        public int WorkerID { get; set; }

//        public Worker()
//        {
//            WorkerID = id_counter++;
//        }
//    }

//    class ILP
//    {

//        public void run()
//        {
//            List<Node> nodes = new List<Node>() {
//                new Node(2) { Dependencies = new int[] { } },
//                new Node(1) { Dependencies = new int[] { 0 } },
//                new Node(2) { Dependencies = new int[] { 0 } },
//                new Node(3) { Dependencies = new int[] { 2 } },
//                new Node(2) { Dependencies = new int[] { 1 } },
//                new Node(2) { Dependencies = new int[] { 4,3 }},
//                new Node(1) { Dependencies = new int[] { 0 } },
//                new Node(2) { Dependencies = new int[] { 1 } },
//                new Node(1) { Dependencies = new int[] { 6 } },
//                new Node(3) { Dependencies = new int[] { 1 } }};

//            var links = new Link[nodes.Count * nodes.Count];

//            for (int i = 0; i < nodes.Count; i++) {
//                for (int j = 0; j < nodes.Count; j++) {
//                    if (nodes[i].Dependencies.Contains(j)) {
//                        links[i * nodes.Count + j] = new Link(i, j, 1);
//                    } else {
//                        links[i * nodes.Count + j] = new Link(i, j, 0);
//                    }
//                }
//            }

//            List<Worker> workers = new List<Worker>() {
//                new Worker(),
//                new Worker(),
//                new Worker()
//            };

//            Solve(nodes, links.AsEnumerable(), workers);
//        }

//        private void Solve(IEnumerable<Node> nodes, IEnumerable<Link> links, IEnumerable<Worker> workers)
//        {
//            SolverContext context = SolverContext.GetContext();
//            Model model = context.CreateModel();

//            var nodeSet = new Set(0, nodes.Count(), 1);
//            var workerSet = new Set(0, workers.Count(), 1);

//            //-------------Parameters--------------
//            var weights = new Parameter(Domain.IntegerNonnegative, "weights", nodeSet);
//            weights.SetBinding(nodes, "Weight", "ID");
//            var dependencies = new Parameter(Domain.IntegerRange(0, 1), "dependencies", nodeSet, nodeSet);
//            dependencies.SetBinding(links, "isDependent", "Source", "Parent");

//            model.AddParameters(weights, dependencies);

//            //-------------Decisions--------------
//            var startTimes = new Decision(Domain.IntegerNonnegative, "starts", nodeSet);
//            var finishTimes = new Decision(Domain.IntegerNonnegative, "finishes", nodeSet);
//            var makespan = new Decision(Domain.IntegerNonnegative, "makespan");
//            var allocation = new Decision(Domain.IntegerRange(0, 1), "allocation", nodeSet, workerSet);

//            model.AddDecisions(startTimes, finishTimes, makespan, allocation);

//            //-------------Constraints--------------
//            model.AddConstraint("FinishTime", Model.ForEach(nodeSet, (node) => startTimes[node] + weights[node] == finishTimes[node]));

//            //model.AddConstraint("OneAtATime", Model.ForEach(nodeSet, (n) => 
//            //    Model.ForEachWhere(nodeSet, (n2) => Model.Or(finishTimes[n] < startTimes[n2], startTimes[n] > finishTimes[n2]), (n2) => n != n2)));

//            model.AddConstraint("Allocatee", Model.ForEach(nodeSet, (n) => Model.Sum(Model.ForEach(workerSet, (w) => allocation[n, w])) == 1));
//            //model.AddConstraint("Allocatee", Model.ForEach(nodeSet, (n) => Model.ExactlyMofN(1,allocation[n])));

//            model.AddConstraint("OneAtATime",
//                Model.ForEach(workerSet, (w) =>
//                Model.ForEach(nodeSet, (n) =>
//                Model.ForEachWhere(nodeSet, (n2) => Model.Implies(Model.And(allocation[n, w] == 1, allocation[n2, w] == 1),
//                    Model.Or(finishTimes[n] <= startTimes[n2], startTimes[n] >= finishTimes[n2])), (n2) => n != n2))));

//            model.AddConstraint("PrecedenceConstraints", Model.ForEach(nodeSet, task =>
//                Model.ForEach(nodeSet, parent =>
//                Model.Implies(dependencies[task, parent] == 1, startTimes[task] >= finishTimes[parent]))));

//            model.AddConstraint("ProjectFinish", Model.ForEach(nodeSet, (n) => makespan >= finishTimes[n]));

//            model.AddGoal("MinMakeSpan", GoalKind.Minimize, makespan);

//            context.CheckModel();
//            //using (StreamWriter sw = new StreamWriter("Stadium.oml")) {
//            //    context.SaveModel(FileFormat.OML, sw); ;
//            //}
//            Solution solution = context.Solve();
//            Report report = solution.GetReport();
//            Console.WriteLine(@"===== report =====");
//            Console.Write("{0}", report);
//            Console.ReadLine();
//        }

//    }
//}
