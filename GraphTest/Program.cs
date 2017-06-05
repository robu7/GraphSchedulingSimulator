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
       

        static List<List<Node>> threadLists;
        static List<Worker> workerList;
        static int[] earliestStartTime;
        static ManualResetEvent[] waitsignals;
        public static StreamWriter w;

        static void Main(string[] args)
        {
            //w = new StreamWriter()
            using (w = File.AppendText("log.txt"))
            {
                InitLog(w);
            }

            // Produce a randomgraph for testing
            Tree DAGgraph = Tree.GenerateRandomWeightedDAG();
            //Tree DAGgraph = Tree.GenerateTestGraph();

            // Init per thread variables 
            earliestStartTime = new int[Settings.threadCount];
            waitsignals = new ManualResetEvent[Settings.threadCount];

            // Init all threadlists, one per thread
            threadLists = new List<List<Node>>();
            workerList = new List<Worker>();
            for (int i = 0; i < Settings.threadCount; i++)
            {
                threadLists.Add(new List<Node>());
                waitsignals[i] = new ManualResetEvent(false);
                earliestStartTime[i] = 0;
                workerList.Add(new Worker(ref waitsignals[i], i));

            }

            Stopwatch time = new Stopwatch();

            ThreadPool.SetMaxThreads(Settings.threadCount, Settings.threadCount);

            DAGgraph.ComputeSLevel();
            DAGgraph.ComputeTLevel();

            double parallelTime = 0;
            double sequentialTime = 0;

            foreach (var item in DAGgraph.SortByID())
            {
                item.LogNodeInfo();
            }

            var sortedList = DAGgraph.SortBySLevel();


            /*
             *  Run in sequential execution,
             *  ------------------
             */
            time.Start();

            ExecuteTreeSeq(sortedList as object);

            sequentialTime = time.ElapsedMilliseconds;
            time.Reset();

            /* ------------------ */


            HighestLevel scheduler = new HighestLevel(DAGgraph,workerList,Settings.threadCount);

            scheduler.ScheduleDAG(DAGgraph);

            /*
             *  Run in Parallel execution,
             *  ------------------
             */
            Console.WriteLine(time.ElapsedMilliseconds);
            time.Start();

            for (int i = 0; i < Settings.threadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(Worker.ExecuteTaskList), workerList[i]);
            }

            WaitHandle.WaitAll(waitsignals);

            parallelTime = time.ElapsedMilliseconds;
            time.Stop();
            time.Reset();
            /* ------------------ */

            double speedup = sequentialTime / parallelTime;

            Console.WriteLine("Sequential took: "+ sequentialTime);
            Console.WriteLine("Parallel took: " + parallelTime);
            Console.WriteLine("Speedup is: " + speedup);
            Console.WriteLine("Efficiency per processor: " + speedup/ Settings.threadCount);

            using (w = File.AppendText("log.txt"))
            {
                Log("\r\nEnd of test run\r\n:\r\n", w);
                Log("-------------------------------", w);
            }
            

            Console.ReadKey();
        }

    

        private static void ExecuteTreeSeq(object v)
        {
            Console.WriteLine("Hello from thread: " + Thread.CurrentThread.ManagedThreadId);
            //object[] state = o as object[];
            List<Node> localList = v as List<Node>;
            foreach (var item in localList)
            {
                while (!item.AreParentsDone) { }
                item.Execute();
                item.IsDone = true;
            }
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " is done");
        }

        private static void ExecuteTree(object o)
        {
            Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " started work");
            object[] state = o as object[];
            List<Node> localList = state[0] as List<Node>;
            ManualResetEvent doneSignal = state[1] as ManualResetEvent;
            foreach (var item in localList)
            {
                while (!item.AreParentsDone) { }
                item.Execute();
                item.IsDone = true;
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
