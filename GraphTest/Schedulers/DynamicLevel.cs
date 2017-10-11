using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest.Schedulers
{
    class DynamicLevel
    {
    }
}




//using Configura.CETOpLib.Data.BuildCentral.BuildItems;
//using Configura.CETOpLib.Utils;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;

//namespace Configura.CETOpLib.Machines.Builders.ParallelUtility
//{
//    class DynamicLevel : Scheduler
//    {
//        private List<TaskNode> readyList;

//        public DynamicLevel(TaskGraph graph, TaskExecutionEstimator estimator, int? maxWorkerCount = null) : base(graph, estimator, maxWorkerCount)
//        {
//            readyList = new List<TaskNode>();
//        }

//        public override void ScheduleDAG()
//        {
//            List<Tuple<float, float, Worker, TaskNode>> tasksToBestWorkerMapping = new List<Tuple<float, float, Worker, TaskNode>>();

//            graph.ComputeSLevel();
//            graph.ComputeTLevel();
//            this.readyList = graph.Nodes.Where(x => x.ReadyToSchedule).ToList();


//            float highestDynamicLevel;
//            Tuple<float, float, Worker, TaskNode> bestFit;
//            while (readyList.Count != 0) {
//                tasksToBestWorkerMapping.Clear();

//                foreach (var task in readyList) {
//                    tasksToBestWorkerMapping.Add(GetWorkerWithBestDL(task));
//                }


//                highestDynamicLevel = tasksToBestWorkerMapping.Max(x => x.Item2);
//                bestFit = tasksToBestWorkerMapping.Where(x => x.Item2 == highestDynamicLevel).First();
//                bestFit.Item3.AddTask(bestFit.Item4);

//                bestFit.Item3.EarliestStartTime = (int)bestFit.Item1 + bestFit.Item4.Weight;
//                bestFit.Item4.FinishTime = bestFit.Item3.EarliestStartTime;
//                bestFit.Item4.Status = BuildStatus.Scheduled;
//                readyList.Remove(bestFit.Item4);
//                readyList.AddRange(bestFit.Item4.ChildNodes.Where(x => x.ReadyToSchedule));
//            }
//        }

//        /// <summary>
//        /// Get the worker with the least estimated start time for a incoming task
//        /// </summary>
//        private Tuple<float, float, Worker, TaskNode> GetWorkerWithBestDL(TaskNode node)
//        {
//            Worker bestWorker = workerList[0];

//            float EarliestExecutionStartTime = float.MaxValue, bestEEST = EarliestExecutionStartTime; // EarliestExecutionStartTime (EEST)
//            float dynamicLevel = 0;
//            float? bestDL = null;

//            foreach (var worker in workerList) {
//                if (worker.EarliestStartTime < node.EarliestStartTime) {
//                    EarliestExecutionStartTime = (int)node.EarliestStartTime;
//                } else {
//                    EarliestExecutionStartTime = worker.EarliestStartTime;
//                }

//                dynamicLevel = (float)node.SLevel - EarliestExecutionStartTime;

//                if (dynamicLevel > bestDL || bestDL == null) {
//                    bestWorker = worker;
//                    bestDL = dynamicLevel;
//                    bestEEST = EarliestExecutionStartTime;
//                }
//            }
//            return new Tuple<float, float, Worker, TaskNode>(bestEEST, (float)bestDL, bestWorker, node);
//        }

//        public override string ToString()
//        {
//            return "DLS";
//        }
//    }
//}