using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphTest
{
    class Worker
    {
        public int WorkerID { get; private set; }
        private List<Node> taskList;
        public int EarliestStartTime { get; set; }
        private ManualResetEvent readySignal;

        public Worker(ref ManualResetEvent waitSignal, int ID)
        {
            WorkerID = ID;
            readySignal = waitSignal;
            EarliestStartTime = 0;
            taskList = new List<Node>();
    }

        public void AddTask(Node task) { taskList.Add(task); }

        public static void ExecuteTaskList(object o)
        {
            Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " started work");
            Worker worker = o as Worker;
            List<Node> localList = worker.taskList;

            foreach (var item in localList)
            {
                while (!item.AreParentsDone) { }
                item.Execute();
                item.IsDone = true;
            }
            worker.readySignal.Set();
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " just finished");
        }

        internal void LogSchedule()
        {
            StringBuilder logString = new StringBuilder();

            logString.AppendLine();
            logString.AppendFormat("Worker {0} schedule {1} ", WorkerID, "{");

            //logString.Append("Worker schedule: {");
            foreach (var item in taskList)
            {
                logString.Append(item.ToString() + ", ");                   
            }

            logString.Remove(logString.Length-2, 2);
            logString.AppendLine("}\n");

            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Program.Log(logString.ToString(), w);
            }
        }
    }
}
