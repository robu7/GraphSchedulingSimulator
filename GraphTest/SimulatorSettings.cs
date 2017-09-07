using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest
{
    class SimulatorSettings
    {
        public int minRanks = 5; // Minimum height of the graph
        public int maxRanks = 5; // Maximum height of the graph
        public int minWidth = 5; // Minimum width of the graph
        public int maxWidth = 10; // Maximum width of the graph
        public int minSimTime = 3000;
        public int maxSimTime = 14000;
        public double changeOfEdge = 60;
        public int ThreadCount { get; set; } = 4;

        /// <summary>
        /// 
        /// </summary>
        public void SetRanks(int min, int max)
        {
            minRanks = min;
            maxRanks = max;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetWidth(int min, int max)
        {
            minWidth = min;
            maxWidth = max;
        }
     
        public void SetRandomTaskWeight(int minValue, int maxValue)
        {
            minSimTime = minValue;
            maxSimTime = maxValue;
        }


    }
}
