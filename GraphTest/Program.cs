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
            earliestStartTime = new int[Settings.threadCount];
            waitsignals = new ManualResetEvent[Settings.threadCount];

            // Init all threadlists, one per thread
            threadLists = new List<List<TaskNode>>();
            workerList = new List<Worker>();
            for (int i = 0; i < Settings.threadCount; i++) {
                threadLists.Add(new List<TaskNode>());
                waitsignals[i] = new ManualResetEvent(false);
                earliestStartTime[i] = 0;
                workerList.Add(new Worker(ref waitsignals[i], i));

            }
        }


        static void Main(string[] args)
        {
            //w = new StreamWriter()
            using (w = File.AppendText("log.txt"))
            {
                InitLog(w);
            }

            // Produce a randomgraph for testing
            //DAGgraph = TaskGraph.GenerateRandomWeightedDAG();
            DAGgraph = TaskGraph.GenerateTestGraph();

            // Init per thread variables 
            Reset();

            ThreadPool.SetMaxThreads(Settings.threadCount, Settings.threadCount);

            foreach (var item in DAGgraph.SortByID())
            {
                item.LogNodeInfo();
            }

            TaskExecutionEstimator test = new TaskExecutionEstimator();

            //double speedup = sequentialTime / parallelTime;

            //Console.WriteLine("Sequential took: "+ sequentialTime);
            //Console.WriteLine("Parallel took: " + parallelTime);
            //Console.WriteLine("Speedup is: " + speedup);
            //Console.WriteLine("Efficiency per processor: " + speedup/ Settings.threadCount);

            using (w = File.AppendText("log.txt"))
            {
                Log("\r\nEnd of test run\r\n:\r\n", w);
                Log("-------------------------------", w);
            }

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

        static void HandleInput(string input)
        {
            string[] cmdArgs = input.Split();
            //Console.WriteLine(input);

            switch (cmdArgs[0])
            {
                case "rand":
                    Console.WriteLine("Randomizing a new graph....");
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

        static void ExecuteAlgorithm(string arg = "")
        {
            Stopwatch time = new Stopwatch();

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
                    break;
                case "ILP":
                    break;
                case "2":
                case "CP/MISF":
                    CP_MISF tmp = new CP_MISF(DAGgraph, workerList, Settings.threadCount);
                    tmp.ScheduleDAG(DAGgraph);
                    break;
                    
                case "":
                    Console.WriteLine("Wrong number of arguments");
                    break;
                default:
                    Console.WriteLine(arg + "algorithm does not exist");
                    break;
            }
        }

        private static void ExecuteHLWET()
        {

            DAGgraph.ComputeSLevel();
            DAGgraph.ComputeTLevel();
            var sortedList = DAGgraph.SortBySLevel();

            Stopwatch time = new Stopwatch();
            time.Start();

            HighestLevel scheduler = new HighestLevel(DAGgraph, workerList, Settings.threadCount);

            scheduler.ScheduleDAG(DAGgraph);

            //var infoDisplayer = new SchedulerInfo(workerList);

            for (int i = 0; i < Settings.threadCount; i++) {
                ThreadPool.QueueUserWorkItem(Worker.ExecuteTaskList, new object[] { workerList[i], infoDisplayer });
            }

            new Thread(() => new SchedulerInfo(workerList).ShowDialog()).Start();

            WaitHandle.WaitAll(waitsignals);

            var parallelTime = time.ElapsedMilliseconds;
            time.Stop();
            time.Reset();

            Console.WriteLine("HLWET took {0}ms",parallelTime);

        }


        /// <summary>
        /// Run in sequential execution
        /// </summary>
        private static void ExecuteSequencial(List<TaskNode> sortedList)
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
            Console.WriteLine("Hello from thread: " + Thread.CurrentThread.ManagedThreadId);
            //object[] state = o as object[];
            List<TaskNode> localList = v as List<TaskNode>;
            foreach (var item in localList)
            {
                while (!item.IsReadyToExecute) { }
                item.Execute();
                item.Status = BuildStatus.Executed;
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " is done");
        }

        private static void ExecuteTree(object o)
        {
            Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " started work");
            object[] state = o as object[];
            List<TaskNode> localList = state[0] as List<TaskNode>;
            ManualResetEvent doneSignal = state[1] as ManualResetEvent;
            foreach (var item in localList)
            {
                while (!item.IsReadyToExecute) { }
                item.Execute();
                item.Status = BuildStatus.Executed;
            }
            doneSignal.Set();
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " just finished");
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
