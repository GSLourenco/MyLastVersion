using System;
using Quartz;
using Quartz.Impl;

namespace MvcApplication2.DataModel
{
    public static class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<DatabaseJob>().Build();
            IJobDetail job1 = JobBuilder.Create<SetTrafficJob>().Build();


            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInMinutes(1)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(0, 0))
                  )
                .Build();

            ITrigger trigger1 = TriggerBuilder.Create()
            .WithDailyTimeIntervalSchedule
                (s =>
                    s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(20, 0))
                )
            .Build();


            scheduler.ScheduleJob(job, trigger);
            scheduler.ScheduleJob(job1, trigger1);
        }
    }
}
