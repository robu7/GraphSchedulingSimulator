using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;

namespace GraphTest.Schedulers
{
    class GurobiILP
    {
        //GRBModel
        public void Run()
        {
        }
    }
}















//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
////using Gurobi;
//using OPTANO.Modeling.Common;
//using OPTANO.Modeling.Optimization;
//using OPTANO.Modeling.Optimization.Configuration;
//using OPTANO.Modeling.Optimization.Enums;
//using OPTANO.Modeling.Optimization.Solver.Cplex127;


//namespace ILPTest
//{
//    public class Task
//    {

//        /// <summary>
//        /// Task ID
//        /// </summary>
//        public int ID { get; set; }


//        /// <summary>
//        /// Computation cost of this task
//        /// </summary>
//        public int Weight { get; set; }


//        /// <summary>
//        /// a readable representation of the job
//        /// </summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            return ID.ToString();
//        }
//    }

//    public class Machine
//    {
//        /// <summary>
//        /// label for the machine
//        /// </summary>
//        public int MachineId { get; set; }

//        ///// <summary>
//        ///// the tasks <see cref="JobScheduling.Task"/> that are supported by this machine
//        ///// if a machine is assigned to a job those tasks need to match these
//        ///// </summary>
//        //public List<Task> SupportedTasks { get; set; } = new List<Task>();


//        /// <summary>
//        /// a readable representation of the machine
//        /// </summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            return MachineId.ToString();
//        }
//    }





//    class GurobiSolver
//    {
//        public GurobiSolver()
//        {

//        }

//        public void Run()
//        {
//            var machines = new List<Machine>() {
//                new Machine { MachineId = 1}
//            };

//            var tasks = new List<Task>() {
//                new Task { ID = 1, Weight=2},
//                new Task { ID = 2, Weight=1}
//            };

//            var config = new Configuration();
//            config.NameHandling = NameHandlingStyle.UniqueLongNames;
//            config.ComputeRemovedVariables = true;

//            using (var scope = new ModelScope(config)) {
//                var jobScheduleModel = new JobScheduleModel(tasks, machines);
//            }
//        }

//    }

//    class JobScheduleModel
//    {

//        /// <summary>
//        /// 
//        /// </summary>
//        public List<Task> Tasks { get; set; }

//        /// <summary>
//        /// 
//        /// </summary>
//        public List<Machine> Machines { get; set; }

//        /// <summary>
//        /// 
//        /// </summary>
//        public Model Model { get; private set; }

//        public VariableCollection<Task, Machine, int> StartTime { get; private set; }



//        /// <summary>
//        /// 
//        /// </summary>
//        public JobScheduleModel(List<Task> tasks, List<Machine> machines)
//        {
//            Tasks = tasks;
//            Machines = machines;

//            this.Model = new Model();
//            var ranks = Enumerable.Range(0, tasks.Count).ToList();

//            this.StartTime = new VariableCollection<Task, Machine, int>(
//                Model,
//                Tasks,
//                Machines,
//                ranks,
//                "StartTime",
//                (t, m, r) => $"StartTime_t{t}_m{m}_r{r}",
//                (t, m, r) => 0,
//                (t, m, r) => double.PositiveInfinity,
//                VariableType.Continuous);
//        }

//    }
//}
