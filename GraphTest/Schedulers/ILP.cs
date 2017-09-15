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










//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.SolverFoundation.Common;
//using Microsoft.SolverFoundation.Services;
//using Microsoft.SolverFoundation.Solvers;
//using System.IO;

//namespace ILPTest
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            var data = new List<Task>() {
//            new Task(){ Duration = 1, Name = "task0", Dependencies = new int[]{ } },
//            new Task(){ Duration = 3, Name = "task1", Dependencies = new int[]{ 0 } },
//            new Task(){ Duration = 1, Name = "task2", Dependencies = new int[]{ 0 } } ,
//            new Task(){ Duration = 1, Name = "task3", Dependencies = new int[]{ 1,2 } }
//            };


//            //var links = new List<Link>();
//            var links = new Link[data.Count * data.Count];

//            for (int i = 0; i < data.Count; i++) {
//                for (int j = 0; j < data.Count; j++) {
//                    if (data[i].Dependencies.Contains(j)) {
//                        links[i * data.Count + j] = new Link(i, j, 1);
//                    } else {
//                        links[i * data.Count + j] = new Link(i, j, 0);
//                    }
//                }
//            }

//            var allocationMapping = new WorkerAllocation[4 * data.Count];


//            for (int i = 0; i < 4; i++) {
//                for (int j = 0; j < data.Count; j++) {
//                    allocationMapping[i * 4 + j] = new WorkerAllocation(i, j, 0);
//                }
//            }


//            Test(data, links.AsQueryable(), allocationMapping.AsEnumerable());
//            //SolveScheduling(data, links);
//            Console.ReadLine();
//        }

//        public class WorkerAllocation
//        {
//            public int WorkerID { get; set; }
//            public int TaskID { get; set; }
//            public int isAssigned { get; set; }
//            public WorkerAllocation(int id, int task, int assign = 0)
//            {
//                WorkerID = id;
//                TaskID = task;
//                isAssigned = assign;
//            }
//        }


//        public class Link
//        {
//            public int Source { get; set; }
//            public int Parent { get; set; }
//            public int isDependent { get; set; }

//            public Link(int task, int parent, int isDepen)
//            {
//                Source = task;
//                Parent = parent;
//                isDependent = isDepen;
//            }
//        }

//        public class Task
//        {
//            private static int id_counter = 0;
//            public Task() { ID = id_counter++; }
//            public int ID { get; private set; }
//            public string Name { get; set; }
//            public double Duration { get; set; }
//            public int[] Dependencies { get; set; }
//            public int[] childTasks { get; set; }
//        }

//        private static void Test(IEnumerable<Task> data, IQueryable<Link> links, IEnumerable<WorkerAllocation> alloc)
//        {
//            SolverContext context = SolverContext.GetContext();
//            Model model = context.CreateModel();

//            var taskSet = new Set(0, data.Count(), 1);
//            var machineSet = new Set(0, 4, 1);

//            var duration = new Parameter(Domain.IntegerNonnegative, "durations", taskSet);
//            var id = new Parameter(Domain.IntegerNonnegative, "id", taskSet);
//            var dependencies = new Parameter(Domain.IntegerRange(0, 1), "dependencies", taskSet, taskSet);

//            duration.SetBinding(data, "Duration", "ID");
//            id.SetBinding(data, "ID", "ID");
//            dependencies.SetBinding(links, "isDependent", "Source", "Parent");

//            model.AddParameters(duration, id, dependencies);

//            var projectFinish = new Decision(Domain.RealNonnegative, "projectFinish");
//            var start = new Decision(Domain.RealNonnegative, "starts", taskSet);
//            var finish = new Decision(Domain.RealNonnegative, "finishs", taskSet);
//            var taskToMachineMapping = new Decision(Domain.RealRange(0, 1), "taskAllocations", machineSet, taskSet);


//            model.AddDecisions(projectFinish, start, finish, taskToMachineMapping);
//            taskToMachineMapping.SetBinding(alloc, "isAssigned", "WorkerID", "TaskID");

//            // === Constraints ===
//            model.AddConstraint("PrecedenceConstraints", Model.ForEach(taskSet, task =>
//                Model.ForEach(taskSet, parent =>
//                Model.Implies(dependencies[task, parent] == 1, start[task] >= finish[parent]))));

//            // start + duration = finish
//            model.AddConstraint("constraint1", Model.ForEach(taskSet, (t) => start[t] + duration[t] == finish[t]));

//            // projectFinish after all tasks finished
//            model.AddConstraint("constraint2", Model.ForEach(taskSet, t => projectFinish >= finish[t]));

//            // At most one task at each machine 
//            model.AddConstraint("TaskMapping", Model.ForEach(taskSet, (task) => Model.Sum(Model.ForEach(machineSet, (machine) => taskToMachineMapping[machine, task])) == 1));
//            //model.AddConstraint("Mapping", Model.ForEach(machineSet, (task) => Model.Sum(Model.ForEach(taskSet, (machine) => taskToMachineMapping[machine, task])) == 1));

//            // Not more than one task at a time
//            //model.AddConstraint("constraint3", Model.ForEach(machineSet, machine =>
//            //    Model.ForEach(taskSet, t =>
//            //    Model.ForEachWhere(taskSet, t2 => Model.Implies(Model.And(taskToMachineMapping[machine, t] == 1, taskToMachineMapping[machine, t2] == 1), start[t2] > finish[t]), (t2) => id[t] != id[t2]))));

//            //model.AddConstraint("constraint3", Model.ForEach(machineSet, machine =>
//            //   Model.ForEach(taskSet, t =>
//            //   Model.ForEachWhere(taskSet, t2 => Model.Implies(Model.And(start[t2] > start[t], start[t2] < finish[t]), taskToMachineMapping[machine, t2] == 1), (t2) => id[t] != id[t2]))));

//            //model.AddConstraint("TaskExecution", Model.ForEach(machineSet, (machine) =>
//            //    Model.ForEach(taskSet, (task) => Model.If(taskToMachineMapping[machine, task] == 1, finish[task] > 10, start[task] > 10))));



//            //Model.ForEachWhere(taskSet, (task2) => Model.Implies(finish[task] > 1, Model.And(taskToMachineMapping[machine, task] == 1, taskToMachineMapping[machine, task2] == 1)), (task2) => id[task] != id[task2]))));


//            // === Goals ===
//            model.AddGoal("goal0", GoalKind.Minimize, projectFinish); // minimieren der projekt zeit

//            // === Solve ===
//            context.CheckModel();
//            //using (FileStream fs = File.Open("Bicycle.mps", FileMode.OpenOrCreate))
//            //using (StreamWriter sw = new StreamWriter(fs)) {
//            //    context.SaveModel(FileFormat.MPS, sw); ;
//            //}
//            Solution solution = context.Solve();
//            Report report = solution.GetReport();
//            Console.WriteLine(@"===== report =====");
//            Console.Write("{0}", report);
//            Console.ReadLine();

//        }
//    }
//}
