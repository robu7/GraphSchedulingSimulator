using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphTest.Schedulers
{
    class CP_MISF : Scheduler
    {

        public CP_MISF(Tree DAGgraph, List<Worker> workerList, int? maxThreadCount = null) : base(DAGgraph, maxThreadCount)
        {

        }

        public override void ExecuteSchedule()
        {
            throw new NotImplementedException();
        }

        public override void ScheduleDAG(Tree graph)
        {
            throw new NotImplementedException();
        }
    }
}
