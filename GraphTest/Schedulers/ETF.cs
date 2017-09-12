using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest.Schedulers
{
    /// <summary>
    /// Earliest Time First (ETF) is a dynamic algorithm.
    /// At each step, estimate the earliest execution start time for all
    /// ready nodes and selects the one with the lowest value for scheduling.
    /// The ready node is defined as the node having all its parents scheduled
    /// </summary>
    //class EarliestTimeFirst : Scheduler
    //{
    //    private List<TaskNode> readyList;

    //    public EarliestTimeFirst(TaskGraph graph, TaskExecutionEstimator estimator, int? maxWorkerCount = null) : base(graph, estimator, maxWorkerCount)
    //    {
    //        readyList = new List<TaskNode>();
    //    }

    //    public override void ScheduleDAG()
    //    {
    //        List<Tuple<float, Worker, TaskNode>> tasksToBestWorkerMapping = new List<Tuple<float, Worker, TaskNode>>();

    //        graph.ComputeSLevel();
    //        graph.ComputeTLevel();
    //        this.readyList = graph.Nodes.Where(x => x.ReadyToSchedule).ToList();


    //        float lowestEEST;
    //        Tuple<float, Worker, TaskNode> bestFit;
    //        while (readyList.Count != 0) {
    //            tasksToBestWorkerMapping.Clear();

    //            foreach (var task in readyList) {
    //                tasksToBestWorkerMapping.Add(GetWorkerWithMinEst(task));
    //            }

    //            lowestEEST = tasksToBestWorkerMapping.Min(x => x.Item1);
    //            bestFit = tasksToBestWorkerMapping.Where(x => x.Item1 == lowestEEST).OrderByDescending(x => x.Item3.SLevel).First();
    //            bestFit.Item2.AddTask(bestFit.Item3);

    //            bestFit.Item2.EarliestStartTime = (int)bestFit.Item1 + bestFit.Item3.Weight;
    //            bestFit.Item3.BuildStatus = Status.Scheduled;
    //            readyList.Remove(bestFit.Item3);
    //            readyList.AddRange(bestFit.Item3.ChildNodes.Where(x => x.ReadyToSchedule));
    //        }
    //    }

    //    /// <summary>
    //    /// Get the worker with the least estimated start time for a incoming task
    //    /// </summary>
    //    private Tuple<float, Worker, TaskNode> GetWorkerWithMinEst(TaskNode node)
    //    {
    //        Worker bestWorker = workerList[0];

    //        float EEST = float.MaxValue, bestEEST = EEST;

    //        foreach (var worker in workerList) {
    //            if (worker.EarliestStartTime < node.EarliestStartTime) {
    //                EEST = (int)node.EarliestStartTime;
    //            } else {
    //                EEST = worker.EarliestStartTime;
    //            }

    //            if (EEST < bestEEST) {
    //                bestWorker = worker;
    //                bestEEST = EEST;
    //            }
    //        }
    //        return new Tuple<float, Worker, TaskNode>(bestEEST, bestWorker, node);
    //    }

    //    public override void ExecuteSchedule(BuildRecipe recipe, BuildMachine machine, string platform, DictionaryEx<string, BuildItemBase> lookupById)
    //    {
    //        Sampler = new DataSampler(this.workerList.Count);

    //        foreach (var worker in workerList) {
    //            worker.Recipe = recipe;
    //            worker.BuildMachine = machine;
    //            worker.Platform = platform;
    //            worker.ExecuteTaskList(lookupById);
    //        }
    //        Sampler.StartCpuSampling();

    //        // Wait for all workers to finish their tasklist
    //        WaitHandle.WaitAll(waitSignals);

    //        foreach (var worker in workerList) {
    //            int scheduledIdleTime = 0;
    //            foreach (var item in worker.IdleSlots) {
    //                scheduledIdleTime += item.Size;
    //            }
    //            Console.WriteLine("Worker " + worker.WorkerID + " scheduled idle time: " + scheduledIdleTime);
    //            Console.WriteLine("Worker " + worker.WorkerID + " total idle time: " + worker.idleTime);
    //        }

    //        Sampler.StopCpuSampling();
    //        Console.WriteLine("Average Cpu usage: " + Sampler.GetAverageCpuUsage());
    //        Console.WriteLine("Schedule Makespan: " + workerList.Max(x => x.EarliestStartTime));
    //    }

    //    public override string ToString()
    //    {
    //        return "ETF";
    //    }
    //}
}
