using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GraphTest
{
    class IdleSlot : ScheduleSlot
    {
        public IdleSlot(int start, int end, TaskNode nextTask) :base(start, end)
        {

            NextExecutableTask = nextTask;
        }

        //public override int StartTime { get; set; }
        //public override int EndTime { get; set; }

        //public int Size { get { return EndTime - StartTime; } }

        /// <summary>
        /// Contains the next task in line to be executed after this idleslot
        /// </summary>
        public TaskNode NextExecutableTask { get; set; }

    }

    class WorkSlot : ScheduleSlot
    {
        public TaskNode TaskToBeExecuted { get; }

        public WorkSlot(int start, int end, TaskNode task) : base(start, end)
        {
            TaskToBeExecuted = task;
        }

    }

    public class ScheduleSlot
    {
        public ScheduleSlot(int start, int end)
        {
            StartTime = start;
            EndTime = end;
        }
        public int StartTime { get; set; }
        public int EndTime { get; set; }

        public virtual int Size { get { return EndTime - StartTime; } }
    }

    public class Worker
    {
        public int WorkerID { get; private set; }
        private List<ScheduleSlot> taskList;
        public List<ScheduleSlot> FullSchedule { get { return taskList; } } // Returns entire schedule, including idle tasks
        public IEnumerable<TaskNode> TaskList { // Returns all tasks tasks that need processing
            get {
                foreach (var slot in taskList) {
                    if(slot is WorkSlot) {
                        yield return (slot as WorkSlot).TaskToBeExecuted;
                    }
                }
                yield break;
            }
        }
        public int EarliestStartTime { get; set; }
        private ManualResetEvent readySignal;
        public ManualResetEvent ReadySignal { get { return readySignal; } }

        public Worker(int ID)
        {
            WorkerID = ID;
            //readySignal = waitSignal;
            Reset();
            
        }
        public void Reset()
        {
            readySignal = new ManualResetEvent(false);
            EarliestStartTime = 0;
            taskList = new List<ScheduleSlot>();
        }


        public void AddTask(int start, int end, TaskNode task) { taskList.Add(new WorkSlot(start, end, task)); }
        public void AddIdleTime(int start, int end, TaskNode nextTask) { taskList.Add(new IdleSlot(start, end, nextTask)); }

        public static void ExecuteTaskList(object o)
        {
            Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " started work");
            object[] tmp = o as object[];
            Worker worker = tmp[0] as Worker;
            SchedulerInfo infoDisplyer = tmp[1] as SchedulerInfo;
            var localList = worker.TaskList;

            foreach (var task in localList)
            {
                task.WaitForParentsToFinish();
                //infoDisplyer.UpdateStatus("Begin work on task: " + task.ID);
                task.Execute();
                task.Status = BuildStatus.Executed;
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
