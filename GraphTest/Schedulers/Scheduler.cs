using System;
using System.Collections.Generic;
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
        public int WorkerCount;
        protected TaskGraph graph;
        protected List<Worker> workerList;

        // Array of signals which is used to notify the master instance when a worker is done. 
        // Worker 1 (workList[0]) will have a refrence to waitSignals[0] etc....
        protected ManualResetEvent[] waitSignals;

        protected Scheduler(TaskGraph graph, int? maxThreadCount)
        {
            this.graph = graph;
            this.workerList = new List<Worker>();

            // If no threadCount is given, set to one per core.
            if (maxThreadCount == null) {
                int coreCount = 0;
                foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get()) {
                    coreCount += int.Parse(item["NumberOfCores"].ToString());
                }
                Console.WriteLine("Number Of Cores: {0}", coreCount);
                WorkerCount = coreCount;
            } else
                WorkerCount = (int)maxThreadCount;

            ThreadPool.SetMaxThreads(WorkerCount, WorkerCount);

            waitSignals = new ManualResetEvent[WorkerCount];
            for (int i = 0; i < WorkerCount; i++) {
                waitSignals[i] = new ManualResetEvent(false);
                workerList.Add(new Worker(ref waitSignals[i], i + 1));
            }
        }

        public abstract void ScheduleDAG(TaskGraph graph);
        public abstract void ExecuteSchedule();
    }
}
