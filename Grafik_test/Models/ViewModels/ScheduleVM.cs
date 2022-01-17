using Grafik_test.ScheduleLogic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grafik_test.Models.ViewModels
{
    public class ScheduleVM
    {
        public List<SelectListItem> MonthList;
        public List<SelectListItem> YearList;
        public List<SelectListItem> WorkerList;

        public Schedule Schedule;
        public List<Worker> Workers;

        public SelectListItem DefaultMonth;
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
        }
    }
}
