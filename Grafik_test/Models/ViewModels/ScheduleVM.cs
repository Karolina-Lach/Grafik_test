using Grafik_test.ScheduleLogic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Grafik_test.Models.ViewModels
{
    public class ScheduleVM
    {
        /**** Listy do list rozwijanych ****/
        public List<SelectListItem> MonthList;
        public List<SelectListItem> YearList;
        public List<SelectListItem> WorkerList;
        public SelectListItem DefaultMonth;

        /**** Grafiki ****/
        public Schedule Schedule;
        public List<Worker> Workers;
        public Worker WorkerWithMaxSalary;
        public Worker WorkerWithMinSalary;
        public Worker WorkerWithMaxWeekends;
        public Worker WorkerWithMinWeekends;
        public float WeekWagePerHour;
        public float WeekendWagePerHour;

        public ScheduleVM()
        {
            MonthList = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                MonthList.Add(
                new SelectListItem
                {
                    Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[i - 1],
                    Value = i.ToString()
                });
            }



            YearList = new List<SelectListItem>();
            for (int i = System.DateTime.Today.Year - 3; i <= System.DateTime.Today.Year + 3; i++)
            {
                YearList.Add(
                new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }

            DefaultMonth = new SelectListItem
            {
                Text = @System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[@System.DateTime.Today.Month - 1],
                Value = @System.DateTime.Today.Month.ToString()
            };
        }
        public ScheduleVM(float wageWeek, float wageWeekend) : this()
        {
            WeekendWagePerHour = wageWeekend;
            WeekWagePerHour = wageWeek;
            
        }

        [System.Obsolete]
        public ScheduleVM(int month, int year, List<Worker> workers, List<Wage> wages, int numberOfShifts, int minBreak, int minWeekend) : this()
        {
            DefaultMonth = new SelectListItem
            {
                Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[month - 1],
                Value = month.ToString()
            };

            Workers = workers;
            Schedule = new Schedule(month, year, workers, wages, numberOfShifts, minBreak, minWeekend);
            WorkerList = new List<SelectListItem>();
            foreach(var worker in workers)
            {
                WorkerList.Add(
                    new SelectListItem
                    {
                        Text = worker.Name + " " + worker.LastName,
                        Value = worker.Id.ToString()
                    });
            }

            WorkerWithMaxSalary = workers.First(item => item.Id == Schedule.WorkerIdWithMaxSalary());
            WorkerWithMinSalary = workers.First(item => item.Id == Schedule.WorkerIdWithMinSalary());
            WorkerWithMaxWeekends = workers.First(item => item.Id == Schedule.WorkerIdWithMaxWeekends());
            WorkerWithMinWeekends = workers.First(item => item.Id == Schedule.WorkerIdWithMinWeekends());
        }
    }
}
