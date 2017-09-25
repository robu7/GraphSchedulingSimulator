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
    /// Scheduler which follows the highest level first with estimated times (HLFET) alogoritm 
    /// </summary>
    class HighestLevel : Scheduler
    {

        public HighestLevel(TaskGraph DAGgraph, int? maxThreadCount = null) : base(DAGgraph, maxThreadCount)
        {}

        public override void ExecuteSchedule()
        {

            foreach (var worker in workerList) {
                //worker.ExecuteTaskList(lookupById);
            }
            // Wait for all workers to finish their tasklist
            //WaitHandle.WaitAll(waitSignals);



            Console.WriteLine("Schedule Makespan: " + workerList.Max(x => x.EarliestStartTime));
        }

        public override void ScheduleDAG()
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

                //if (worker.EarliestStartTime < task.tLevel) {
                //    worker.AddIdleTime(worker.EarliestStartTime, (int)task.tLevel, task);
                //    worker.EarliestStartTime = (int)task.tLevel;
                //} 

                if (worker.EarliestStartTime < task.EarliestStartTime) {
                    worker.AddIdleTime(worker.EarliestStartTime, (int)task.EarliestStartTime, task);
                    worker.EarliestStartTime = (int)task.EarliestStartTime;
                }


                worker.AddTask(worker.EarliestStartTime, worker.EarliestStartTime + task.SimulatedExecutionTime, task);
                task.Status = BuildStatus.Scheduled;
                worker.EarliestStartTime += task.SimulatedExecutionTime;
                task.FinishTime = worker.EarliestStartTime;
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
            int total = 0;
            foreach (var item in workerList)
            {
                total = 0;
                foreach (var slot in item.FullSchedule) {
                    total += slot.Size;
                }
                Console.WriteLine("Worker"+item.WorkerID + ": "+ total);
                item.LogSchedule();
            }
        }

        ///// <summary>
        ///// Insert ready task into idle slots to minimize idle time
        ///// </summary>
        //private void UtilizeIdleSlot(List<TaskNode> readyList, IdleSlot idleSlot, Worker worker)
        //{
        //    // Extract all readyItems which can fit in the idle slot
        //    var viableTasks = readyList.Where(x => (x.Weight <= idleSlot.Size) && (x.EarliestStartTime <= idleSlot.StartTime)).ToList();
        //    //var viableTasks = readyList.Where(x => (x.Weight <= idleSlot.Size)).ToList();
        //    viableTasks.OrderBy(x => x.EarliestStartTime);

        //    /*
        //     * Check if no other worker can allow earlier scheduling,
        //     * 
        //     */
        //    IdleSlot slotBeforeTask = null;
        //    IdleSlot slotAfterTask = null;
        //    foreach (var task in viableTasks) {

        //        if (workerList.Where(x => x.EarliestStartTime < task.EarliestStartTime).Count() > 0)
        //            continue;

        //        int oldStartTime = idleSlot.StartTime;
        //        idleSlot.StartTime = (int)task.EarliestStartTime;

        //        if (oldStartTime != idleSlot.StartTime) {
        //            slotBeforeTask = worker.CreateIdleSlot(oldStartTime, idleSlot.StartTime, task);
        //        }

        //        int oldEndTime = idleSlot.EndTime;
        //        idleSlot.EndTime = idleSlot.StartTime + task.Weight;

        //        if (oldEndTime != idleSlot.EndTime) {
        //            slotAfterTask = worker.CreateIdleSlot(idleSlot.EndTime, oldEndTime, idleSlot.NextExecutableTask);
        //        }

        //        worker.InsertAheadOfTask(task, idleSlot.NextExecutableTask);
        //        readyList.Remove(task);
        //        task.Status = BuildStatus.Scheduled;

        //        worker.RemoveIdleSlot(idleSlot);

        //        if (slotBeforeTask != null) {
        //            UtilizeIdleSlot(readyList, slotBeforeTask, worker);
        //        }
        //        if (slotAfterTask != null) {
        //            UtilizeIdleSlot(readyList, slotAfterTask, worker);
        //        }

        //        readyList.AddRange(task.ChildNodes.Where(x => x.ReadyToSchedule));
        //        break;
        //    }
        //}


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
