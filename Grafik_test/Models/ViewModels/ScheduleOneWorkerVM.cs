using Grafik_test.ScheduleLogic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grafik_test.Models.ViewModels
{
    public class ScheduleOneWorkerVM
    {
        public Schedule Schedule;
        public Worker Worker;

    
        [System.Obsolete]
        public ScheduleOneWorkerVM(Schedule schedule, Worker worker)
        {
            Worker = worker;
            Schedule = schedule;
        }
    }
}
