using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grafik_test.Models;

namespace Grafik_test.ScheduleLogic
{
    public class Schedule
    {
        public struct Shift
        {
            public int FirstWorker { get; set; }
            public int SecondWorker { get; set; }
        }

        public int ShiftsPerDay = 3;
        readonly int minimumBreak = 11;
        readonly int minimumWeekend = 35;

        public float wagePerHour = 30;
        public float wagePerHolidayHour = 60;
        public Month Month;
        public int NumberOfAvailableWorkers;
        public int NumberOfShifts;

        public Shift[] ScheduleTable { get; set; }

        public List<int> Workers;

        [Obsolete]
        public Schedule(List<Worker> workers, List<Wage> wages) : this(DateTime.Today.Month, workers, wages)
        {
        }

        [Obsolete]
        public Schedule(int month, List<Worker> workers, List<Wage> wages) : this(month, DateTime.Today.Year, workers, wages)
        {

        }

        [Obsolete]
        public Schedule(int month, int year, List<Worker> workers, List<Wage> wages)
        {
            Month = new Month(month, year);
            NumberOfAvailableWorkers = workers.Count;
            NumberOfShifts = Month.NumberOfDays * ShiftsPerDay;
            ScheduleTable = new Shift[NumberOfShifts];

            Workers = new List<int>();
            workers.ForEach(worker => Workers.Add(worker.Id));

            foreach (Wage wage in wages)
            {
                if (wage.IsHoliday)
                {
                    wagePerHolidayHour = wage.WagePerHour;
                }
                else
                {
                    wagePerHour = wage.WagePerHour;
                }
            }
        }

        [Obsolete]
        public Schedule(int month, int year, List<Worker> workers, List<Wage> wages,
                        int numberOfShifts, int minBreak, int minWeekend) : this(month, year, workers, wages)
        {
            ShiftsPerDay = numberOfShifts;
            minimumBreak = minBreak;
            minimumWeekend = minWeekend;
        }


        public Shift[] CreateSchedule()
        {
            Queue<int> availableWorkers = new Queue<int>();
            foreach (var worker in Workers)
            {
                availableWorkers.Enqueue(worker);
            }

            for (int i = 0; i < NumberOfShifts; i++)
            {
                if (availableWorkers.Count == 0)
                {
                    /** Powinien być kod sprawdzający którzy pracownicy mogą pracować **/
                    foreach (var worker in Workers)
                    {
                        availableWorkers.Enqueue(worker);
                    }
                }
                ScheduleTable[i].FirstWorker = availableWorkers.Dequeue();

                if (availableWorkers.Count == 0)
                {
                    /** Powinien być kod sprawdzający którzy pracownicy mogą pracować **/
                    foreach (var worker in Workers)
                    {
                        availableWorkers.Enqueue(worker);
                    }
                }

                ScheduleTable[i].SecondWorker = availableWorkers.Dequeue();
            }

            return ScheduleTable;
        }

        private int GetDay(int shiftNumber)
        {
            return (shiftNumber / ShiftsPerDay) + 1;
        }
        public bool IsDayHoliday(int day)
        {
            return Month.FreeDaysNumbers.Contains(day);
        }
        public bool IsShiftHoliday(int shiftNumber)
        {
            return IsDayHoliday(GetDay(shiftNumber));
        }
        private int GetLastShift(int workerId, int currentShift)
        {
            for (int i = 0; i < currentShift; i++)
            {
                if (ScheduleTable[i].FirstWorker == workerId || ScheduleTable[i].SecondWorker == workerId)
                    return i;
            }
            return -1;
        }
        private int GetBreakLength(int workerId, int currentShift)
        {
            int lastShift = GetLastShift(workerId, currentShift);
            if (lastShift == -1)
            {
                return minimumBreak;
            }

            return (currentShift - lastShift) * 8;
        }
        private float GetCurrentSalary(int workerId, int currentShift)
        {
            float salary = 0;
            for (int i = 0; i < currentShift; i++)
            {
                if (ScheduleTable[i].FirstWorker == workerId || ScheduleTable[i].SecondWorker == workerId)
                {
                    if (IsShiftHoliday(i))
                    {
                        salary += wagePerHolidayHour * (24 / ShiftsPerDay);
                    }
                    else
                    {
                        salary += wagePerHour * (24 / ShiftsPerDay);
                    }
                }
            }

            return salary;
        }
    }
}
