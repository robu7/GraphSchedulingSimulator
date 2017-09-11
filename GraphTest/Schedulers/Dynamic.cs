using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GraphTest.Schedulers
{
    class ReadyTaskList
    {
        List<TaskNode> readyList;
        public ManualResetEvent TasksReady { get; private set; }
        
        public ReadyTaskList(List<TaskNode> initialList)
        {
            readyList = initialList;
            TasksReady = new ManualResetEvent(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddNewReadyNodes(TaskNode executedTask)
        {
            lock (readyList) {
                readyList.AddRange(executedTask.ChildNodes.Where(x => x.IsReadyToExecute));
            }
            if(readyList.Count > 0) {
                TasksReady.Set();
            } else {
                TasksReady.Reset();
            }
        }
    }
    class ReadyWorkerList
    {

        public ReadyWorkerList()
        {

        }
    }


    class Dynamic : Scheduler
    {
        ReadyTaskList readyList;

        public Dynamic(TaskGraph graph, int? maxThreadCount = null) : base(graph, maxThreadCount)
        {
            this.graph.ComputeSLevel();
            var initialList = this.graph.SortBySLevel().Where(x => x.IsReadyToExecute).ToList();
            readyList = new ReadyTaskList(initialList);
        }

        public override void ExecuteSchedule()
        {

            while (!graph.Nodes.TrueForAll(x => x.Status == BuildStatus.Executed)) {
                readyList.TasksReady.WaitOne();

            }
        }

        public override void ScheduleDAG()
        {
        }
    }
}
