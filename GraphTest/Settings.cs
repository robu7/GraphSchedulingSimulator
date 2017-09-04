using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest
{
    static class Settings
    {
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


        public static int minRanks = 5; // Minimum height of the graph
        public static int maxRanks = 5; // Maximum height of the graph
        public static int minWidth = 5; // Minimum width of the graph
        public static int maxWidth = 10; // Maximum width of the graph
        public const int minSimTime = 3000;
        public const int maxSimTime = 14000;
        public const double changeOfEdge = 60;
        public const int ThreadCount = 4;
    }
}
