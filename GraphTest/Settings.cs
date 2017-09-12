using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest
{
    static class Settings
    {
        public static int minRanks = 5; // Minimum height of the graph
        public static int maxRanks = 5; // Maximum height of the graph
        public static int minWidth = 5; // Minimum width of the graph
        public static int maxWidth = 10; // Maximum width of the graph
        public static int minSimTime = 3000;
        public static int maxSimTime = 14000;
        public static double changeOfEdge = 40;
        public static int ThreadCount { get; set; } = 4;

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
