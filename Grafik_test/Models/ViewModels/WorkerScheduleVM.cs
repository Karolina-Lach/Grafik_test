using Grafik_test.ScheduleLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grafik_test.Models.ViewModels
{
    public class WorkerScheduleVM
    {
        public Schedule Schedule;
        public Worker Worker;

        [Obsolete]
        public WorkerScheduleVM(Schedule schedule, Worker worker)
        {
            Schedule = schedule;
            Worker = worker;
        }


    }
}
