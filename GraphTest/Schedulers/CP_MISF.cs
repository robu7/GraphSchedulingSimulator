using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphTest.Schedulers
{
    class BranchWorker
    {
        public List<int> TaskList;
        public int FinishTime { get; set; }
        public int WorkerId { get; set; }

        public BranchWorker(int workerId, int finishTime, List<int> taskList)
        {
            this.WorkerId = workerId;
            this.FinishTime = finishTime;
            this.TaskList = taskList;
        }

        public BranchWorker Clone()
        {
            return new BranchWorker(WorkerId, FinishTime, TaskList.ToList());
        }
    }


    /// <summary>
    /// 
    /// </summary>
    class Branch
    {
        List<TaskNode> readyList;
        //List<Tuple<int, int>> selectionPointer;
        int[] selectionPointer;
        int currentTime;
        int depth;
        int numberOfAvailableCores;
        int[] availableCores;
        private static int maxCores;
        Dictionary<int, TaskNode> coreToTask;
        //Dictionary<int, BranchWorker> workerCollection;
        //List<BranchWorker> workerList;

        /// <summary>
        /// 
        /// </summary>
        public Branch(List<TaskNode> readyList, int time, int depth, int cores, int[] availCores, Dictionary<int, TaskNode> coreToTaskMapping, int[] selection = null)
        {
            this.readyList = readyList;
            this.depth = depth;
            this.currentTime = time;
            this.numberOfAvailableCores = cores;
            this.selectionPointer = new int[cores];
            this.coreToTask = coreToTaskMapping;
            this.availableCores = availCores;
            if(coreToTask == null) {
                Init();               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            coreToTask = new Dictionary<int, TaskNode>();
            availableCores = new int[numberOfAvailableCores];

            for (int i = 0; i < numberOfAvailableCores; i++) {
                availableCores[0] = i;
                coreToTask[i] = null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void GenerateBranchAlternatives()
        {
            // First determine how many idle nodes that should be available
            int numberOfIdleNodes = 0;
            if (numberOfAvailableCores == maxCores) {
                numberOfIdleNodes = numberOfAvailableCores - 1;
            } else
                numberOfIdleNodes = numberOfAvailableCores;

            for (int i = 0; i < numberOfIdleNodes; i++) {
                readyList.Add(null);
            }

            var backTrackingList = new List<int[]>();

            for (int i = 0; i <= readyList.Count - numberOfAvailableCores; i++) {
                var selectionPointer = new int[numberOfAvailableCores];

                for (int j = 0; j < numberOfAvailableCores; j++) {
                    selectionPointer[j] = i + j;
                }
                backTrackingList.Add(selectionPointer);

                var tmpSelectionPointer = (int[])selectionPointer.Clone();
                for (int x = tmpSelectionPointer[numberOfAvailableCores - 1]; x < readyList.Count-1; x++) {
                    tmpSelectionPointer[numberOfAvailableCores-1]++;
                    backTrackingList.Add(tmpSelectionPointer);
                    tmpSelectionPointer = (int[])tmpSelectionPointer.Clone();
                }
            }            
            

            var readyListCopy = readyList;
            //while (backTrackingList.Count > 0) {

            int earliestTaskFinishTime = FindEarliestTaskFinishTime(backTrackingList, 0);

            int[] taskIndexes = backTrackingList[0];

            for (int i = 0; i < numberOfAvailableCores; i++) {
                coreToTask[i] = readyListCopy[taskIndexes[i]];
            }

            foreach (var index in taskIndexes) {
                AddNewReadyNodes(readyListCopy, index);
                readyListCopy.RemoveAt(0);
            }

            //var coreCount = coreToTask.Where(x => x.Value == null || x.Value.SimulatedExecutionTime <= earliestTaskFinishTime).Count();
            int coreCount = 0;
            List<int> availableCores = new List<int>();
            foreach (var item in coreToTask) {
                if(item.Value == null || item.Value.SimulatedExecutionTime <= earliestTaskFinishTime) {
                    ++coreCount;
                    availableCores.Add(item.Key);
                }
            }


            if (readyListCopy.Count == 0)
                Console.WriteLine("");

            readyListCopy.RemoveAll(x => x == null);

            var newBranch = new Branch(readyListCopy, earliestTaskFinishTime, depth + 1,coreCount, availableCores.ToArray(), coreToTask, backTrackingList[0]);       

            newBranch.GenerateBranchAlternatives();

            //}


        }


        /// <summary>
        /// 
        /// </summary>
        private int FindEarliestTaskFinishTime(List<int[]> backTrackingList, int index)
        {
            int earliestTaskFinishTime = 0;
            for (int i = 0; i < backTrackingList[index].Count(); i++) {
                if (readyList[i] == null)
                    continue;

                if (earliestTaskFinishTime == 0)
                    earliestTaskFinishTime = readyList[i].SimulatedExecutionTime;
                else
                    earliestTaskFinishTime = Math.Min(readyList[i].SimulatedExecutionTime, earliestTaskFinishTime);
            }
            return earliestTaskFinishTime;
        }


        /// <summary>
        /// 
        /// </summary>
        private void AddNewReadyNodes(List<TaskNode> readyList, int index)
        {
            if (readyList[index] == null)
                return;
            var task = readyList[index];
            task.Status = BuildStatus.Scheduled;
            readyList.AddRange(task.ChildNodes.Where(x => x.IsReadyToSchedule));
        }

        /// <summary>
        /// 
        /// </summary>
        public static Branch GenerateDummyStartNode(List<TaskNode> readyList, int cores)
        {
            maxCores = cores;
            return new Branch(readyList, 0, 0, cores,null, null);
        }       
    }





    /// <summary>
    /// 
    /// </summary>
    class CP_MISF : Scheduler
    {
        List<Worker> workerList;

        public CP_MISF(TaskGraph DAGgraph, List<Worker> workerList, int? maxThreadCount = null) : base(DAGgraph, maxThreadCount)
        {
            this.workerList = workerList;
        }

        public override void ExecuteSchedule()
        {
            throw new NotImplementedException();
        }

        public override void ScheduleDAG(TaskGraph graph)
        {

            // Part 1: Preproccessing 
            DetermineLevels();
            var sortedList = TaskRenumbering();


            // Part 2: Depth-First Search
            var readyList = sortedList.Where(x => x.IsReadyToSchedule).ToList();
            StartDepthFirstSearch(readyList);

            using (StreamWriter w = File.AppendText("log.txt")) {
                Program.Log("\r\nCP_MISF Workers: \r\n", w);
            }
            foreach (var item in workerList) {
                item.LogSchedule();
            }
        }

        private void StartDepthFirstSearch(List<TaskNode> readyList)
        {
            var startBranch = Branch.GenerateDummyStartNode(readyList, WorkerCount);

            startBranch.GenerateBranchAlternatives();
        }

        private void DetermineLevels()
        {
            graph.ComputeSLevel();
            graph.ComputeTLevel();           
        }

        private List<TaskNode> TaskRenumbering()
        {
            return graph.Nodes.OrderByDescending(x => x.slLevel).ThenByDescending(x => x.ChildNodes.Count).ToList();
        }



        private Worker GetWorkerWithMinEst()
        {
            Worker retWorker = workerList[0];

            foreach (var worker in workerList) {
                if (worker.EarliestStartTime < retWorker.EarliestStartTime) {
                    retWorker = worker;
                }
            }

            return retWorker;
        }
    }
}
