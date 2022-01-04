using Nager.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grafik_test.ScheduleLogic
{
    public class Month
    {
        public int MonthNumber;
        public int Year;
        public int NumberOfDays;
        public List<DateTime> FreeDays;
        public List<int> FreeDaysNumbers;

        [Obsolete]
        public Month(int month, int year)
        {
            MonthNumber = month;
            Year = year;
            NumberOfDays = DateTime.DaysInMonth(year, month);
            FreeDays = new List<DateTime>();
            FreeDaysNumbers = new List<int>();

            CreateFreeDaysList(month, year);
            CreateFreeDaysNumbers();
        }

        [Obsolete]
        private void CreateFreeDaysList(int month, int year)
        {
            var holidays = DateSystem.GetPublicHoliday(new DateTime(year, month, 1),
                                                          new DateTime(year, month, NumberOfDays),
                                                          CountryCode.PL);
            foreach (var holiday in holidays)
            {
                FreeDays.Add(new DateTime(holiday.Date.Year, holiday.Date.Month, holiday.Date.Day));
            }
            for (int day = 1; day <= NumberOfDays; day++)
            {
                var date = new DateTime(year, month, day);
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    if (!FreeDays.Contains(date))
                    {
                        FreeDays.Add(date);
                    }
                }
            }

            if (month == 8)
            {
                var date = new DateTime(year, month, 14);
                if (!FreeDays.Contains(date))
                {
                    FreeDays.Add(date);
                }
            }

            FreeDays.Sort((x, y) => x.Day.CompareTo(y.Day));
        }

        private void CreateFreeDaysNumbers()
        {
            FreeDays.ForEach(date => FreeDaysNumbers.Add(date.Day));
        }
    }
}
