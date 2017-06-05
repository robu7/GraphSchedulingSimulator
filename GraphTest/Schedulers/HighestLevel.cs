using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphTest.Schedulers
{

    /// <summary>
    /// Abstract scheduler class
    /// </summary>
    abstract class Scheduler
    {
        readonly int threadCount;
        protected Tree graph;

        protected Scheduler(Tree graph,int? maxThreadCount)
        {
            this.graph = graph;

            // If no threadCount is given, set to one per core.
            if (maxThreadCount == null)
            {
                // TODO
            }
            else
                threadCount = (int)maxThreadCount;

            ThreadPool.SetMaxThreads(threadCount, threadCount);
        }

        public abstract void ScheduleDAG(Tree graph);
        public abstract void ExecuteSchedule();
    }


    /// <summary>
    /// Scheduler which follows the highest level first with estimated times (HLFET) alogoritm 
    /// </summary>
    class HighestLevel : Scheduler
    {
        List<Worker> workerList;

        public HighestLevel(Tree DAGgraph, List<Worker> workerList, int? maxThreadCount = null) : base(DAGgraph, maxThreadCount)
        {
            this.workerList = workerList;
        }

        public override void ExecuteSchedule()
        {
            throw new NotImplementedException();
        }

        public override void ScheduleDAG(Tree DAGgraph)
        {

            // Step 1: Sort by Static b-level
            var sortedList = graph.SortBySLevel();

            var task = new Node();
            foreach (var node in sortedList)
            {
                node.PrintNodeInfo();
            }

            // Step 2: Create ready list, will only contain entry nodes at first
            var readyList = sortedList.Where(x => x.ReadyToSchedule).ToList();
            readyList.OrderByDescending(x => x.slLevel);


            while (readyList.Count != 0)
            {
                /*
                 * Step 3: 
                 *  Schedule the first node in the ready list to a processor that allows the earliest
                 *  execution, using the non-insertion approach.
                 */
                task = readyList.ElementAt(0);

                var worker = GetWorkerWithMinEst();

                worker.AddTask(task);
                task.IsScheduled = true;

                if (worker.EarliestStartTime < task.tLevel)
                    worker.EarliestStartTime = task.SimulatedExecutionTime / 100 + (int)task.tLevel;
                else
                    worker.EarliestStartTime += task.SimulatedExecutionTime / 100;

                /*
                 * Step 4: 
                 *  Update the ready list by inserting the nodes that are now ready
                 */
                readyList.AddRange(task.ChildNodes.Where(x => x.ReadyToSchedule));
                readyList.RemoveAt(0);
                readyList.OrderByDescending(x => x.slLevel);
            }

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Program.Log("\r\nHighest Level Workers: \r\n", w);
            }
            foreach (var item in workerList)
            {
                item.LogSchedule();
            }
        }

        private Worker GetWorkerWithMinEst()
        {
            Worker retWorker = workerList[0];

            foreach (var worker in workerList)
            {
                if (worker.EarliestStartTime < retWorker.EarliestStartTime)
                {
                    retWorker = worker;
                }                
            }

            return retWorker;
        }

    }
}
