using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GraphTest.Schedulers
{
    /// <summary>
    /// 
    /// </summary>
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

        public BranchWorker(int workerId, int finishTime, List<TaskNode> taskList, TaskNode currentTask)
        {
            this.WorkerId = workerId;
            this.FinishTime = finishTime;
            this.TaskList = taskList;
            this.currentTask = currentTask;
        }

        public void AssignTask(TaskNode task, int currentTime)
        {
            currentTask = task;
            if (task != null) {
                TaskList.Add(task);
                if (currentTime + task.SimulatedExecutionTime < task.EarliestStartTime + task.SimulatedExecutionTime)
                    FinishTime = task.EarliestStartTime + task.SimulatedExecutionTime;
                else {
                    FinishTime = currentTime + task.SimulatedExecutionTime;
                    foreach (var item in task.ChildNodes) {
                        if(item.EarliestStartTime < currentTime + task.SimulatedExecutionTime)
                            item.UpdateEST(FinishTime);
                    }
                }
            }
        }

        public BranchWorker Clone()
        {
            return new BranchWorker(WorkerId, FinishTime, TaskList.ToList(), currentTask);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    class Branch
    {
        // Local members
        List<TaskNode> readyList;
        int[] selectionPointer;
        int earliestTaskFinishTime;
        int depth;
        int numberOfAvailableCores;
        int[] availableCores;
        private static int maxCores;
        Dictionary<int, BranchWorker> workerMapping;
        List<TaskNode> localNonScheduledNodes;
        int totalTime;

        // Global
        static List<TaskNode> allNodes;
        public static int bestSolution;
        public static Dictionary<int, BranchWorker> bestSolutionWorker;
        public static int branchesExamined; 

        /// <summary>
        /// 
        /// </summary>
        public Branch(List<TaskNode> readyList, int time, int depth, int cores, int[] availCores, Dictionary<int, BranchWorker> workerMapping, int[] selection = null)
        {
            this.readyList = readyList;
            this.depth = depth;
            this.totalTime = time;
            this.earliestTaskFinishTime = 0;
            this.numberOfAvailableCores = cores;
            this.selectionPointer = selection;
            this.workerMapping = workerMapping;
            this.availableCores = availCores;
            if(workerMapping == null) {
                Init();               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            availableCores = new int[numberOfAvailableCores];
            workerMapping = new Dictionary<int, BranchWorker>();
            localNonScheduledNodes = new List<TaskNode>();
            for (int i = 0; i < numberOfAvailableCores; i++) {
                availableCores[0] = i;
                workerMapping[i] = new BranchWorker(i);
            }
        }


        /// <summary>
        /// Expand this branch and allocate tasks to workers acording to the selection pointer,
        /// Apply elimination rule and expand further if deemed to be a viable solution(so far).
        /// </summary>
        public int ExpandBranch()
        {
            localNonScheduledNodes = allNodes.Where(x => x.Status != BuildStatus.Scheduled).ToList();

            // Allocate tasks to proccesors using selection pointer,
            // set allocated nodes to scheduled,
            // insert nodes which are now ready to be scheduled into the readyList.
            var debugReadylist = readyList.ToList();
            List<TaskNode> itemsToBeRemoved = new List<TaskNode>();
            for (int i = 0; i < availableCores.Length; i++) {
                int coreID = availableCores[i];
                var task = readyList[selectionPointer[i]];
                workerMapping[coreID].AssignTask(task, totalTime);
                localNonScheduledNodes.Remove(readyList[selectionPointer[i]]);
                AddNewReadyNodes(readyList, selectionPointer[i]);
                itemsToBeRemoved.Add(readyList[selectionPointer[i]]);
            }
            foreach (var item in itemsToBeRemoved) {
                readyList.Remove(readyList.First(x => x == item));

            }

            this.earliestTaskFinishTime = FindEarliestTaskFinishTime();

            int coreCount = 0;
            foreach (var item in workerMapping) {
                if (item.Value.currentTask == null || item.Value.FinishTime <= earliestTaskFinishTime) {
                    ++coreCount;
                }
            }

            //Console.WriteLine("-------------------");
            //Console.Write("d=" + depth + "   t=" + earliestTaskFinishTime + "\r\nR=[");
            //debugReadylist.ForEach(x => Console.Write((x != null ? x.ID : 0) + ","));
            //Console.Write("]\r\nnewR=[");
            //readyList.ForEach(x => Console.Write((x != null ? x.ID : 0) + ","));
            //Console.WriteLine("]\r\ncores=" + coreCount);
            //Console.Write("SP:[");
            //for (int i = 0; i < selectionPointer.Length; i++) {
            //    Console.Write(availableCores[i] + "->" + selectionPointer[i] + " ,");
            //}
            //Console.WriteLine("]");
            //foreach (var item in workerMapping) {
            //    Console.WriteLine(item.Key + ":" + (item.Value.currentTask != null ? item.Value.currentTask.ID : 0) + " fin:" + workerMapping[item.Key].FinishTime);
            //}
            //Console.WriteLine("-------------------");

            // Find the time where the earliest task is finished and
            // compare with lower bound and best solution
            // if higher then terminate branch
            var highestFinishTime = workerMapping.Values.Max(x => x.FinishTime);

            if (readyList.Where(x => x != null).Count() == 0 && coreCount == maxCores) {

                if (highestFinishTime < bestSolution || bestSolution == 0)
                    bestSolutionWorker = workerMapping;

                return highestFinishTime;
            }
                //return new Tuple<int, Dictionary<int, BranchWorker>>(highestFinishTime,workerMapping);

            if (highestFinishTime > bestSolution && bestSolution != 0)
                return 0;

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
            foreach (var item in workerMapping) {
                if (item.Value.currentTask == null || item.Value.FinishTime <= earliestTaskFinishTime) {
                    item.Value.FinishTime = earliestTaskFinishTime;
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
 
            while (backTrackingList.Count > 0) {
                var readyListCopy = new List<TaskNode>(readyList);

                // Reset all tasks that where not scheduled at this time,
                // To prevent faulty nodes being inserted to the readyList
                foreach (var item in localNonScheduledNodes) {
                    item.Status = BuildStatus.None;
                    item.ResetEST();
                }

                ++branchesExamined; // just for debug
                var workerMappingCopy = workerMapping.ToDictionary(x => x.Key, x => x.Value.Clone()); // create copy of the workerMapping
                var newBranch = new Branch(readyListCopy, earliestTaskFinishTime, depth + 1, coreCount, availableCores.ToArray(), workerMappingCopy, backTrackingList[0]);

                // Recieve makespan value from a branch that are at the end or has been cut
                // return value will be 0 if branch is cut
                var solutionTime = newBranch.ExpandBranch();
                if (solutionTime != 0) {
                    if (solutionTime < bestSolution || bestSolution == 0)
                        bestSolution = solutionTime;
                }
                backTrackingList.RemoveAt(0);
            }

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
        public static Branch GenerateDummyStartNode(List<TaskNode> readyList, int cores, List<TaskNode> completeNodeSet)
        {
            foreach (var node in completeNodeSet) {
                node.Status = BuildStatus.None;
            }

            allNodes = completeNodeSet;
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
            var startBranch = Branch.GenerateDummyStartNode(readyList, WorkerCount, graph.Nodes);

            startBranch.GenerateBranchAlternatives();
            Console.WriteLine("Best solution is :" + Branch.bestSolution);
            Console.WriteLine("Branches examined :" + Branch.branchesExamined);//249883

            foreach (var item in Branch.bestSolutionWorker) {
                Console.Write("worker"+item.Key+":");
                foreach (var tasks in item.Value.TaskList) {
                    Console.Write(" " + tasks.ID + ",");
                }
                Console.WriteLine();
            }

            for (int i = 0; i < Settings.ThreadCount; i++) {
                foreach (var item in Branch.bestSolutionWorker[i].TaskList) {
                    workerList[i].AddTask(0, 0, item);
                }
            }
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
