using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphTest.Schedulers
{
    class BranchWorker
    {
        public List<TaskNode> TaskList;
        public TaskNode currentTask;
        public int FinishTime { get; set; }
        public int WorkerId { get; set; }

        public BranchWorker(int workerId)
        {
            this.WorkerId = workerId;
            this.FinishTime = 0;
            this.TaskList = new List<TaskNode>();
        }

        public BranchWorker(int workerId, int finishTime, List<TaskNode> taskList)
        {
            this.WorkerId = workerId;
            this.FinishTime = finishTime;
            this.TaskList = taskList;
        }

        public void AssignTask(TaskNode task, int currentTime)
        {
            currentTask = task;
            if (task != null) {
                TaskList.Add(task);
                if(currentTime + task.SimulatedExecutionTime < task.tLevel + task.SimulatedExecutionTime)
                    FinishTime = (int)task.tLevel + task.SimulatedExecutionTime;
                else
                    FinishTime = currentTime + task.SimulatedExecutionTime;
            }
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
        int[] selectionPointer;
        int earliestTaskFinishTime;
        int depth;
        int numberOfAvailableCores;
        int[] availableCores;
        private static int maxCores;
        Dictionary<int, TaskNode> coreToTask;
        Dictionary<int, BranchWorker> workerMapping;
        int totalTime;
        static int bestSolution;

        /// <summary>
        /// 
        /// </summary>
        public Branch(List<TaskNode> readyList, int time, int depth, int cores, int[] availCores, Dictionary<int, TaskNode> coreToTaskMapping, Dictionary<int, BranchWorker> workerMapping, int[] selection = null)
        {
            this.readyList = readyList;
            this.depth = depth;
            this.totalTime = time;
            this.earliestTaskFinishTime = 0;
            this.numberOfAvailableCores = cores;
            this.selectionPointer = selection;
            this.coreToTask = coreToTaskMapping;
            this.workerMapping = workerMapping;
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
            workerMapping = new Dictionary<int, BranchWorker>();
            for (int i = 0; i < numberOfAvailableCores; i++) {
                availableCores[0] = i;
                coreToTask[i] = null;
                workerMapping[i] = new BranchWorker(i);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public int ExpandBranch()
        {

            // Allocate tasks to proccesors using selection pointer
            // and set allocated nodes to scheduled 
            // and insert nodes which are now ready to be scheduled

            List<TaskNode> itemsToBeRemoved = new List<TaskNode>();
            for (int i = 0; i < availableCores.Length; i++) {
                int coreID = availableCores[i];
                workerMapping[coreID].AssignTask(readyList[selectionPointer[i]], totalTime);
                coreToTask[coreID] = readyList[selectionPointer[i]];
                AddNewReadyNodes(readyList, selectionPointer[i]);
                itemsToBeRemoved.Add(readyList[selectionPointer[i]]);
                //readyList.RemoveAt(selectionPointer[i]);
            }
            foreach (var item in itemsToBeRemoved) {
                readyList.Remove(readyList.First(x => x == item));
            }


            this.earliestTaskFinishTime = FindEarliestTaskFinishTime();

            Console.Write("d=" + depth + "   t=" + earliestTaskFinishTime + "\r\nR=[");
            readyList.ForEach(x => Console.Write(x != null ? x.ID : 0));
            Console.WriteLine("]\r\ncores="+numberOfAvailableCores);


            // Find the time where the earliest task is finished and
            // compare with lower bound and best solution
            // if higher then terminate branch

            //var longestTime = workerMapping.Values.Max(x => x.FinishTime);

            //if (!readyList.Exists(x => x != null)) {
            //    return longestTime;
            //}
            //else if (longestTime > bestSolution && bestSolution != 0)
            //    return 0;



            GenerateBranchAlternatives();

            return 0;
        }


        /// <summary>
        /// 
        /// </summary>
        public void GenerateBranchAlternatives()
        {
            readyList.RemoveAll(x => x == null);

            // Determine how many cores will be available at earliestTaskFinishTime
            int coreCount = 0;
            List<int> availableCores = new List<int>();
            //foreach (var item in coreToTask) {
            //    if (item.Value == null || item.Value.SimulatedExecutionTime <= earliestTaskFinishTime) {
            //        ++coreCount;
            //        availableCores.Add(item.Key);
            //    }
            //}
            foreach (var item in workerMapping) {
                if (item.Value.currentTask == null || item.Value.FinishTime <= earliestTaskFinishTime) {
                    ++coreCount;
                    availableCores.Add(item.Key);
                }
            }



            this.numberOfAvailableCores = coreCount;
            this.availableCores = availableCores.ToArray();


            // First determine how many idle nodes that should be available
            int numberOfIdleNodes = GenerateIdleNodes(readyList);


            // List which will contain branch alternatives 
            List<int[]> backTrackingList = GenerateSelectionPointerAlternatives();
 
            //Console.WriteLine(readyList.Count);

            //var readyListCopy = readyList.ToList();

            // Find the time where the earliest task is finished
            // Change to include tasks in progress

            // ----------------------------- Fix to include tasks in progress
            //int earliestTaskFinishTime = FindEarliestTaskFinishTime(backTrackingList, 0);

            //int[] taskIndexes = backTrackingList[0];

            ////for (int i = 0; i < numberOfAvailableCores; i++) {
            ////    coreToTask[i] = readyListCopy[taskIndexes[i]];
            ////}

            //foreach (var index in taskIndexes) {
            //    AddNewReadyNodes(readyListCopy, index);
            //    readyListCopy.RemoveAt(0);
            //}

            //--------------------------------

            //int backtrackingIndex = 0;
            //List<TaskNode> readyListCopy;
            while (backTrackingList.Count > 0) {
                var readyListCopy = new List<TaskNode>(readyList);
                //var workerMappingCopy = new Dictionary<int, BranchWorker>(workerMapping);
                var workerMappingCopy = workerMapping.ToDictionary(x => x.Key, x => x.Value.Clone());
                var newBranch = new Branch(readyListCopy, earliestTaskFinishTime, depth + 1, coreCount, availableCores.ToArray(), coreToTask, workerMappingCopy, backTrackingList[0]);
                var solutionTime = newBranch.ExpandBranch();
                //if (solutionTime != 0) {
                //    if (solutionTime < bestSolution || bestSolution == 0)
                //        bestSolution = solutionTime;
                //}
                backTrackingList.RemoveAt(0);
            }

            //readyListCopy.RemoveAll(x => x == null);



            //newBranch.GenerateBranchAlternatives();

            //}


        }

        /// <summary>
        /// 
        /// </summary>
        private List<int[]> GenerateSelectionPointerAlternatives()
        {
            var backTrackingList = new List<int[]>();

            for (int i = 0; i <= readyList.Count - numberOfAvailableCores; i++) {
                var selectionPointer = new int[numberOfAvailableCores];

                for (int j = 0; j < numberOfAvailableCores; j++) {
                    selectionPointer[j] = i + j;
                }
                backTrackingList.Add(selectionPointer);
                if (numberOfAvailableCores == 1) {
                    continue;
                }

                var tmpSelectionPointer = (int[])selectionPointer.Clone();
                for (int x = tmpSelectionPointer[numberOfAvailableCores - 1]; x < readyList.Count - 1; x++) {
                    tmpSelectionPointer[numberOfAvailableCores - 1]++;
                    backTrackingList.Add(tmpSelectionPointer);
                    tmpSelectionPointer = (int[])tmpSelectionPointer.Clone();
                }
            }

            return backTrackingList;
        }


        /// <summary>
        /// Determine the number of idle slots to be inserted
        /// TODO: Not represent idle slots with null
        /// </summary>
        private int GenerateIdleNodes(List<TaskNode> readyList)
        {
            int numberOfIdleNodes = 0;
            if (numberOfAvailableCores == maxCores) {
                numberOfIdleNodes = numberOfAvailableCores - 1;
            } else
                numberOfIdleNodes = numberOfAvailableCores;

            for (int i = 0; i < numberOfIdleNodes; i++) {
                readyList.Add(null);
            }

            return numberOfIdleNodes;
        }


        /// <summary>
        /// 
        /// </summary>
        private int FindEarliestTaskFinishTime()
        {
            int earliestWorkerFinishTime = 0;
            //foreach (var task in coreToTask.Values) {
            //    if (task == null)
            //        continue;

            //    if (task.SimulatedExecutionTime < earliestTaskFinishTime || earliestTaskFinishTime == 0) {
            //        earliestTaskFinishTime = task.SimulatedExecutionTime;
            //    }
            //}

            foreach (var worker in workerMapping.Values) {
                if (worker.currentTask  == null)
                    continue;

                if (worker.FinishTime < earliestWorkerFinishTime || earliestWorkerFinishTime == 0) {
                    earliestWorkerFinishTime = worker.FinishTime;
                }
            }

            return earliestWorkerFinishTime;
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
            var dummyNode = new Branch(readyList,0, 0, cores, null, null, null);
            dummyNode.readyList = readyList;
            return dummyNode;
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
