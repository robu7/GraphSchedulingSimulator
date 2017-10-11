using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest
{
    static class Settings
    {
        public static int minRanks = 50; // Minimum height of the graph
        public static int maxRanks = 50; // Maximum height of the graph
        public static int minWidth = 50; // Minimum width of the graph
        public static int maxWidth = 100; // Maximum width of the graph
        public static int minSimTime = 300;
        public static int maxSimTime = 1400;
        public static double changeOfEdge = 40;
        public static int ThreadCount { get; private set; } = 4;

        public static void SetWorkerCount(int newNumberOfWorkers)
        {
            Console.WriteLine("Switching to one worker!");
            ThreadCount = newNumberOfWorkers;
        } 

        /// <summary>
        /// 
        /// </summary>
        public static void SetRanks(int min, int max)
        {
            minRanks = min;
            maxRanks = max;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SetWidth(int min, int max)
        {
            minWidth = min;
            maxWidth = max;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void SetRandomTaskWeight(int minValue, int maxValue)
        {
            minSimTime = minValue;
            maxSimTime = maxValue;
        }
    }
}
