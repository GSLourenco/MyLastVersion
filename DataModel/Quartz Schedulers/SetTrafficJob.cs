using Quartz;
using System;

namespace MvcApplication2.DataModel
{
    public class SetTrafficJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            PictogramsDb.SetTraffic();
        }

    }
}