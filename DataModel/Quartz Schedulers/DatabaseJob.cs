using Quartz;
using System;

namespace MvcApplication2.DataModel
{
    public class DatabaseJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            PictogramsDb.ScheduleValidationForCodes();
        }

    }
}
