using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest
{
    static class Settings
    {
        public const int minRanks = 25; // Minimum height of the graph
        public const int maxRanks = 25; // Maximum height of the graph
        public const int minWidth = 2; // Minimum width of the graph
        public const int maxWidth = 10; // Maximum width of the graph
        public const int minSimTime = 200;
        public const int maxSimTime = 1000;
        public const double changeOfEdge = 70;
        public const int threadCount = 4;
    }
}
