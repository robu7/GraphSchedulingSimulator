﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphTest.Schedulers
{

    /// <summary>
    /// Scheduler which follows the highest level first with estimated times (HLFET) alogoritm 
    /// </summary>
    class HighestLevel : Scheduler
    {

        public HighestLevel(TaskGraph DAGgraph, List<Worker> workerList, int? maxThreadCount = null) : base(DAGgraph, maxThreadCount)
        {
            this.workerList = workerList;
        }

        public override void ExecuteSchedule()
        {

            foreach (var worker in workerList) {
                //worker.ExecuteTaskList(lookupById);
            }
            // Wait for all workers to finish their tasklist
            WaitHandle.WaitAll(waitSignals);



            Console.WriteLine("Schedule Makespan: " + workerList.Max(x => x.EarliestStartTime));
        }

        public override void ScheduleDAG(TaskGraph DAGgraph)
        {

            // Step 1: Sort by Static b-level
            var sortedList = graph.SortBySLevel();

            // Step 2: Create ready list, will only contain entry nodes at first
            var readyList = sortedList.Where(x => x.IsReadyToSchedule).ToList();
            readyList.OrderByDescending(x => x.slLevel);

            var task = new TaskNode();
            while (readyList.Count != 0)
            {
                /*
                 * Step 3: 
                 *  Schedule the first node in the ready list to a processor that allows the earliest
                 *  execution, using the non-insertion approach.
                 */
                task = readyList.ElementAt(0);

                var worker = GetWorkerWithMinEst();

                if (worker.EarliestStartTime < task.tLevel) {
                    worker.AddIdleTime(worker.EarliestStartTime, (int)task.tLevel, task);
                    worker.EarliestStartTime = (int)task.tLevel;
                } 
                    

                worker.AddTask(worker.EarliestStartTime, worker.EarliestStartTime + task.SimulatedExecutionTime, task);
                task.Status = BuildStatus.Scheduled;
                worker.EarliestStartTime += task.SimulatedExecutionTime;
                /*
                 * Step 4: 
                 *  Update the ready list by inserting the nodes that are now ready
                 */
                readyList.AddRange(task.ChildNodes.Where(x => x.IsReadyToSchedule));
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


        /// <summary>
        /// Find the worker which allows earliest start time 
        /// </summary>
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