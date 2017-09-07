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

            for (int i = 0; i < WorkerCount; i++) {
                workerList.Add(new Worker(i));
            }

        }

        public IEnumerable<Worker> GetWorkers()
        {
            return workerList.AsEnumerable();
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ManualResetEvent> GetWorkerSingnals()
        {
            foreach (var worker in workerList) {
                yield return worker.ReadySignal;
            }
            yield break;
        }

        /// <summary>
        /// 
        /// </summary>
        public int GetMakespan()
        {
            return workerList.Max(x => x.EarliestStartTime);
        }

        public abstract void ScheduleDAG();
        public abstract void ExecuteSchedule();
    }
}
