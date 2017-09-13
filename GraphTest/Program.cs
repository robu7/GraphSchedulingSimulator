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
        static void Main(string[] args)
        {
            GraphSimulator tmp = new GraphSimulator();
            tmp.Run();

            //w = new StreamWriter()
            //using (w = File.AppendText("log.txt"))
            //{
            //    InitLog(w);
            //}

            // Produce a randomgraph for testing
            //DAGgraph = TaskGraph.GenerateRandomWeightedDAG();
            //DAGgraph = TaskGraph.GenerateTestGraph();

            // Init per thread variables 
            //Reset();

            //ThreadPool.SetMaxThreads(Settings.ThreadCount, Settings.ThreadCount);

            //foreach (var item in DAGgraph.SortByID())
            //{
            //    item.LogNodeInfo();
            //}

            //TaskExecutionEstimator test = new TaskExecutionEstimator();

            //using (w = File.AppendText("log.txt"))
            //{
            //    Log("\r\nEnd of test run\r\n:\r\n", w);
            //    Log("-------------------------------", w);
            //}


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
