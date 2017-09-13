using GraphTest.Schedulers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace GraphTest
{
    class GraphSimulator
    {
        TaskGraph activeGraph;
        List<TaskGraph> savedGraphs;

        public GraphSimulator()
        {
            activeGraph = null;
            savedGraphs = new List<TaskGraph>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            activeGraph.ResetNodes();
            //workerList = new List<Worker>();
            //for (int i = 0; i < Settings.ThreadCount; i++) {
            //    workerList.Add(new Worker(i));
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            Console.Write(":");
            string consoleCmd = Console.ReadLine();

            while (consoleCmd != "exit") {
                if (consoleCmd != "")
                    HandleInput(consoleCmd);
                Console.Write(":");
                consoleCmd = Console.ReadLine();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void HandleInput(string consoleCmd)
        {
            string[] cmdArgs = consoleCmd.Split();

            switch (cmdArgs[0]) {
                case "rand":
                    Console.WriteLine("Randomizing a new graph....");
                    activeGraph = TaskGraph.GenerateRandomWeightedDAG();
                    activeGraph.PrintImage();
                    break;
                case "cores":
                    break;
                case "load":
                    break;
                case "print":
                    Console.WriteLine("Printing graph....");
                    activeGraph.PrintImage();
                    activeGraph.PrintTree();
                    break;
                case "run":
                    if (activeGraph == null) {
                        Console.WriteLine("No graph present, neeed to load or generate one!!");
                        return;
                    }
                    if (cmdArgs.Length == 2)
                        ExecuteAlgorithm(cmdArgs[1]);
                    else {
                        ExecuteAlgorithm();
                    }                  
                    break;
                case "help":
                    Console.WriteLine("Available commands:");
                    Console.WriteLine("\trand");
                    Console.WriteLine("\trun --algorithm");
                    break;
                default:
                    Console.WriteLine("\"" + cmdArgs[0] + "\" command does not exist");
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ExecuteAlgorithm(string arg = "")
        {

            activeGraph.ComputeSLevel();
            activeGraph.ComputeTLevel();

            if (arg == "") {
                Console.WriteLine("Choose algorithm by name of number: HLFET::1, CP/MISF::2, ...");
                arg = Console.ReadLine();
            }

            Scheduler schedule = null;
            switch (arg) {
                case "1":
                case "HLWET":
                    Console.WriteLine("Executing HLWET ....");
                    //HighestLevel HlwetScheduler = new HighestLevel(activeGraph,workerList,Settings.ThreadCount);
                    schedule = new HighestLevel(activeGraph, Settings.ThreadCount);

                    break;
                case "ILP":
                    break;
                case "2":
                case "CP/MISF":
                    Console.WriteLine("Executing CP/MISF ....");
                    //CP_MISF Cp_MisfScheduler = new CP_MISF(activeGraph, workerList, Settings.ThreadCount);
                    schedule = new CP_MISF(activeGraph, Settings.ThreadCount);
                    break;
                case "3":
                case "Dynamic":
                    Console.WriteLine("Executing Dynamic ....");
                    schedule = new Dynamic(activeGraph, Settings.ThreadCount);
                    schedule.ExecuteSchedule();
                    Reset();
                    return;
                case "4":
                case "Seq":
                    Console.WriteLine("Executing Sequencial ....");
                    schedule = new HighestLevel(activeGraph, 1);
                    Settings.SetWorkerCount(1);
                    //ExecuteSequencial(activeGraph.SortBySLevel());
                    break;
                case "":
                    Console.WriteLine("Wrong number of arguments");
                    return;
                default:
                    Console.WriteLine(arg + "algorithm does not exist");
                    return;
            }
            ScheduleGraph(schedule); 
            ExecuteSchedule(schedule);
            Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ScheduleGraph(Scheduler schedule)
        {
            Stopwatch time = new Stopwatch();
            time.Start();
            schedule.ScheduleDAG();
            var schedulingTime = time.ElapsedMilliseconds;
            time.Stop();
            Console.WriteLine("Scheduling took {0}ms", schedulingTime);
            Console.WriteLine("MakeSpan: " + schedule.GetMakespan());
        }
        /// <summary>
        /// 
        /// </summary>
        private void ExecuteSchedule(Scheduler schedule)
        {
            Stopwatch executionTime = new Stopwatch();
            executionTime.Start();
            var infoDisplayer = 0;
            var workerList = schedule.GetWorkers();
            for (int i = 0; i < Settings.ThreadCount; i++) {
                ThreadPool.QueueUserWorkItem(Worker.ExecuteTaskList, new object[] { workerList.ElementAt(i), infoDisplayer });
            }
            //WaitHandle.WaitAll(waitsignals);
            WaitHandle.WaitAll(schedule.GetWorkerSingnals().ToArray());
            var parallelTime = executionTime.ElapsedMilliseconds;
            executionTime.Stop();
            Console.WriteLine("Execution took {0}ms", parallelTime);
        }

    }
}
