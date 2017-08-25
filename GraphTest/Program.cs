using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using GraphTest.Schedulers;
using System.Runtime;
using System.IO;

namespace GraphTest
{

    class Program
    {
        static List<List<TaskNode>> threadLists;
        static List<Worker> workerList;
        static int[] earliestStartTime;
        static ManualResetEvent[] waitsignals;
        public static StreamWriter w;
        static public TaskGraph DAGgraph;

        static void Reset()
        {
            earliestStartTime = new int[Settings.ThreadCount];
            waitsignals = new ManualResetEvent[Settings.ThreadCount];
            DAGgraph.ResetTasks();
            // Init all threadlists, one per thread
            threadLists = new List<List<TaskNode>>();
            workerList = new List<Worker>();
            for (int i = 0; i < Settings.ThreadCount; i++) {
                threadLists.Add(new List<TaskNode>());
                waitsignals[i] = new ManualResetEvent(false);
                earliestStartTime[i] = 0;
                workerList.Add(new Worker(ref waitsignals[i], i));
            }
        }


        static void Main(string[] args)
        {
            //w = new StreamWriter()
            //using (w = File.AppendText("log.txt"))
            //{
            //    InitLog(w);
            //}

            // Produce a randomgraph for testing
            //DAGgraph = TaskGraph.GenerateRandomWeightedDAG();
            DAGgraph = TaskGraph.GenerateTestGraph();

            // Init per thread variables 
            Reset();

            ThreadPool.SetMaxThreads(Settings.ThreadCount, Settings.ThreadCount);

            //foreach (var item in DAGgraph.SortByID())
            //{
            //    item.LogNodeInfo();
            //}

            TaskExecutionEstimator test = new TaskExecutionEstimator();

            //using (w = File.AppendText("log.txt"))
            //{
            //    Log("\r\nEnd of test run\r\n:\r\n", w);
            //    Log("-------------------------------", w);
            //}

            Console.Write(":");
            string consoleCmd = Console.ReadLine();

            while (consoleCmd != "exit")
            {
                if(consoleCmd != "")
                    HandleInput(consoleCmd);
                Console.Write(":");
                consoleCmd = Console.ReadLine();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        static void HandleInput(string input)
        {
            string[] cmdArgs = input.Split();
            //Console.WriteLine(input);

            switch (cmdArgs[0])
            {
                case "rand":
                    Console.WriteLine("Randomizing a new graph....");
                    break;
                case "cores":
                    break;
                case "load":
                    break;
                case "print":
                    Console.WriteLine("Printing graph....");
                    DAGgraph.PrintTree();
                    break;
                case "run":
                    if(DAGgraph == null) {
                        Console.WriteLine("No graph present, neeed to load or generate one!!");
                    }
                       
                    if (cmdArgs.Length == 2)
                        ExecuteAlgorithm(cmdArgs[1]);
                    else
                    {
                        ExecuteAlgorithm();
                    }
                    Reset();
                    break;
                case "help":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("\trand");
                    Console.WriteLine("\trun --algorithm");
                    break;
                default:
                    Console.WriteLine("\"" + cmdArgs[0] +"\" command does not exist");
                    break;
            }

            foreach (var arg in cmdArgs)
            {
                
            }
        }


        /// <summary>
        /// 
        /// </summary>
        static void ExecuteAlgorithm(string arg = "")
        {
            Stopwatch time = new Stopwatch();
            DAGgraph.ComputeSLevel();
            DAGgraph.ComputeTLevel();


            if (arg == "") {
                Console.WriteLine("Choose algorithm by name of number: HLFET::1, CP/MISF::2, ...");
                arg = Console.ReadLine();
            }

            switch(arg)
            {
                case "1":
                case "HLWET":
                    Console.WriteLine("Executing HLWET ....");
                    ExecuteHLWET();
                    Reset();
                    break;
                case "ILP":
                    break;
                case "2":
                case "CP/MISF":
                    CP_MISF tmp = new CP_MISF(DAGgraph, workerList, Settings.ThreadCount);
                    //CP_MISF tmp = new CP_MISF(DAGgraph, workerList, Settings.threadCount);
                    tmp.ScheduleDAG(DAGgraph);

                    Stopwatch time2 = new Stopwatch();
                    time2.Start();
                    var infoDisplayer = 0;
                    for (int i = 0; i < Settings.ThreadCount; i++) {
                        ThreadPool.QueueUserWorkItem(Worker.ExecuteTaskList, new object[] { workerList[i], infoDisplayer });
                    }
                    WaitHandle.WaitAll(waitsignals);
                    var parallelTime = time2.ElapsedMilliseconds;
                    time2.Stop();
                    time2.Reset();
                    Console.WriteLine("DF/IHS took {0}ms", parallelTime);
                    break;
                case "3":
                case "Seq":
                    ExecuteSequencial(DAGgraph.SortBySLevel());
                    break;                    
                case "":
                    Console.WriteLine("Wrong number of arguments");
                    break;
                default:
                    Console.WriteLine(arg + "algorithm does not exist");
                    break;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private static void ExecuteSchedule(Scheduler schedule)
        {
            Stopwatch time = new Stopwatch();
            time.Start();
            
            var parallelTime = time.ElapsedMilliseconds;
            time.Stop();
            time.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ExecuteHLWET()
        {

            Stopwatch time = new Stopwatch();
            time.Start();

            HighestLevel scheduler = new HighestLevel(DAGgraph, workerList, Settings.ThreadCount);

            scheduler.ScheduleDAG(DAGgraph);

            //var infoDisplayer = new SchedulerInfo(workerList);
            var infoDisplayer = 0;
            for (int i = 0; i < Settings.ThreadCount; i++) {
                ThreadPool.QueueUserWorkItem(Worker.ExecuteTaskList, new object[] { workerList[i], infoDisplayer });
            }

            Console.WriteLine("MakeSpan: " + workerList.Max(x => x.EarliestStartTime));



            //new Thread(() => new SchedulerInfo(workerList).ShowDialog()).Start();

            WaitHandle.WaitAll(waitsignals);

            foreach (var item in workerList) {
                Console.Write("worker" + item.WorkerID + ":");
                foreach (var tasks in item.TaskList) {
                    Console.Write(" " + tasks.ID + ",");
                }
                Console.WriteLine();
            }

            var parallelTime = time.ElapsedMilliseconds;
            time.Stop();
            time.Reset();

            Console.WriteLine("HLWET took {0}ms",parallelTime);
        }


        /// <summary>
        /// Run in sequential execution
        /// </summary>
        private static void ExecuteSequencial(IEnumerable<TaskNode> sortedList)
        {
           double sequentialTime = 0;
           Stopwatch time = new Stopwatch();
           time.Start();
           ExecuteTreeSeq(sortedList as object);

           sequentialTime = time.ElapsedMilliseconds;
           time.Stop();
        }

        private static void ExecuteTreeSeq(object v)
        {
            Console.WriteLine("Sequential execution");

            IEnumerable<TaskNode> localList = v as IEnumerable<TaskNode>;
            foreach (var item in localList)
            {
                item.WaitForParentsToFinish();
                item.Execute();
                item.Status = BuildStatus.Executed;
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " is done");
        }


        public static void InitLog(TextWriter w)
        {
            w.WriteLine("\n");
            w.Write("\r\nTest run log entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("-------------------------------");
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write(logMessage);
        }
    }
}
