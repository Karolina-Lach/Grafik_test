using Grafik_test.ScheduleLogic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Grafik_test.Models.ViewModels
{
    public class ScheduleVM
    {
        public List<SelectListItem> MonthList;
        public List<SelectListItem> YearList;


        public Schedule Schedule;
        public List<Worker> Workers;

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
        }

        [System.Obsolete]
        public ScheduleVM(int month, int year, List<Worker> workers, List<Wage> wages, int numberOfShifts, int minBreak, int minWeekend) : this()
        {

            Workers = workers;
            Schedule = new Schedule(month, year, workers, wages, numberOfShifts, minBreak, minWeekend);
        }
    }
}
