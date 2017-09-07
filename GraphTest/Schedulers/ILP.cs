using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest.Schedulers
{
    /// <summary>
    /// This scheduler finds the most optimal schedule 
    /// </summary>
    class ILP : Scheduler
    {
        public ILP(TaskGraph graph, int? maxThreadCount = null) : base(graph, maxThreadCount)
        {}

        public override void ExecuteSchedule()
        {
            throw new NotImplementedException();
        }

        public override void ScheduleDAG()
        {
            int cMax = 0;

            /*
             * 1. Start by arranging the nodes in topological order before the branching phase is begun
             */
            


            /*
             * 2.
             */



        }



        private void Init()
        {

        }




    }
}
