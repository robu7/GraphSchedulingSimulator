using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace GraphTest.Schedulers
{
    class Dynamic : Scheduler
    {
        ReadyTaskList readyList;
        ReadyWorkerList workers;

        public Dynamic(TaskGraph graph, int? maxThreadCount = null) : base(graph, maxThreadCount)
        {
            this.graph.ComputeSLevel();
            var initialList = this.graph.SortBySLevel().Where(x => x.IsReadyToExecute).ToList();
            readyList = new ReadyTaskList(initialList);
            workers = new ReadyWorkerList(WorkerCount);
        }

        public override void ExecuteSchedule()
        {
            Stopwatch time = new Stopwatch();
            time.Start(); 
            while (!graph.Nodes.TrueForAll(x => x.Status >= BuildStatus.Scheduled)) {
                workers.WaitForAnyWorker();
                if (!readyList.AreThereReadyTasks()) {
                    continue;
                }
                //readyList.WaitForReadyTasks();

                var worker = workers.GetFirstAvailableWorker();
                var task = readyList.GetFirstReadyTask();

                task.Status = BuildStatus.Scheduled;
                worker.ReadyStatus = false;
                worker.ReadySignal.Reset();
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate { worker.ExecuteTask(task, readyList); }));
            }

            workers.WaitForAllWorker();

            Console.WriteLine("Dynmic algorithm took: " + time.ElapsedMilliseconds + "ms");
            time.Stop();
        }

        public override void ScheduleDAG()
        {
        }
    }


    /// <summary>
    /// 
    /// </summary>
    class ReadyTaskList
    {
        public List<TaskNode> readyList;
        public ManualResetEvent TasksReady { get; private set; }
        
        public ReadyTaskList(List<TaskNode> initialList)
        {
            readyList = initialList;
            TasksReady = new ManualResetEvent(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void AddNewReadyNodes(TaskNode executedTask)
        {
            lock (readyList) {
                readyList.AddRange(executedTask.ChildNodes.Where(x => x.IsReadyToExecute));            
            }
            if (readyList.Count > 0) {
                TasksReady.Set();
            } else {
                TasksReady.Reset();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public TaskNode GetFirstReadyTask()
        {
            var task = readyList[0];
            readyList.RemoveAt(0);
            return task;
        }

        public bool AreThereReadyTasks()
        {
            if (readyList.Count > 0) {
                return true;
            }
            return false;
        }

        public void WaitForReadyTasks()
        {
            if (readyList.Count == 0) {
                TasksReady.Reset();
            }
            TasksReady.WaitOne();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    class ReadyWorkerList
    {
        List<DynamicWorker> workerList;
        public ManualResetEvent WorkersReady { get; private set; }

        public ReadyWorkerList(int workerCount)
        {
            WorkersReady = new ManualResetEvent(false);
            workerList = new List<DynamicWorker>();
            for (int i = 0; i < workerCount; i++) {
                workerList.Add(new DynamicWorker(i));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void WaitForAnyWorker()
        {
            var signals = workerList.Select(x => x.ReadySignal).ToArray();
            WaitHandle.WaitAny(signals);
        }

        /// <summary>
        /// 
        /// </summary>
        public DynamicWorker GetFirstAvailableWorker()
        {
            return workerList.First(x => x.ReadyStatus == true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void WaitForAllWorker()
        {
            var signals = workerList.Select(x => x.ReadySignal).ToArray();
            WaitHandle.WaitAll(signals);
        }
    }

    class DynamicWorker
    {
        public bool ReadyStatus { get; set; }
        public ManualResetEvent ReadySignal { get; set; }
        public int ID { get; set; }

        public DynamicWorker(int id)
        {
            ID = id;
            ReadyStatus = true;
            ReadySignal = new ManualResetEvent(true);
        }

        public void ExecuteTask(TaskNode taskNode, ReadyTaskList readyList)
        {
            Console.WriteLine("Worker: " + ID + " started work on task:" +taskNode.ID);

            Thread.Sleep(taskNode.SimulatedExecutionTime);
            taskNode.Status = BuildStatus.Executed;
            readyList.AddNewReadyNodes(taskNode);
            Console.WriteLine("Worker: " + ID + " finished work on task:" + taskNode.ID);
            ReadyStatus = true;
            ReadySignal.Set();
        }
    }
}
