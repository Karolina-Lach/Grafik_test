using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grafik_test.Models;

namespace Grafik_test.ScheduleLogic
{
    public class Schedule
    {
        private readonly int _workersPerShift = 2;
        public struct Shift
        {
            public int FirstWorker { get; set; }
            public int SecondWorker { get; set; }

            public void SetFirstWorker(int id)
            {
                FirstWorker = id;
            }

            public void SetSecondWorker(int id)
            {
                SecondWorker = id;
            }
        }

        
        readonly int _shiftsPerDay = 3;
        readonly int _minimumBreak = 11;
        readonly int _minimumWeekend = 35;

        readonly float _wagePerHour = 30;
        readonly float _wagePerHolidayHour = 60;

        readonly float _salaryPerShiftHoliday;
        readonly float _salaryPerShift;


        public Month Month;
        public int NumberOfAvailableWorkers;
        public int NumberOfShifts;

        public Dictionary<int, float> SalaryPerWorker;

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
            NumberOfShifts = Month.NumberOfDays * _shiftsPerDay;
            ScheduleTable = new Shift[NumberOfShifts];

            Workers = new List<int>();
            foreach (Worker worker in workers)
            {
                Workers.Add(worker.Id);
                SalaryPerWorker.Add(worker.Id, 0.0f);
            }

            foreach (Wage wage in wages)
            {
                if (wage.IsHoliday)
                {
                    _wagePerHolidayHour = wage.WagePerHour;
                    _salaryPerShiftHoliday = _wagePerHolidayHour * _shiftsPerDay;
                }
                else
                {
                    _wagePerHour = wage.WagePerHour;
                    _salaryPerShift = _wagePerHour * _shiftsPerDay;
                }
            }
        }

        [Obsolete]
        public Schedule(int month, int year, List<Worker> workers, List<Wage> wages,
                        int numberOfShifts, int minBreak, int minWeekend) : this(month, year, workers, wages)
        {
            _shiftsPerDay = numberOfShifts;
            _minimumBreak = minBreak;
            _minimumWeekend = minWeekend;
        }


        public Shift[] CreateSchedule()
        {
            List<Shift> scheduleList = InitSchedule();
            /*
                Manipulowanie grafikiem tak żeby było lepiej...
            */
            return ScheduleTable;
        }

        private List<Shift> InitSchedule()
        {
            
            int numberOfPositions = NumberOfShifts * _workersPerShift;
            int listSize = (int)Math.Ceiling(Convert.ToDecimal(numberOfPositions / NumberOfAvailableWorkers)) * NumberOfAvailableWorkers;
            List<Shift> scheduleList = new List<Shift>(listSize);
            int j = 0;
            for (int i=0;i<listSize;i++)
            {
                scheduleList[j].SetFirstWorker(j);

                j++;
                if (j >= NumberOfAvailableWorkers) j = 0;

                scheduleList[j].SetSecondWorker(j);

                j++;
                if (j >= NumberOfAvailableWorkers) j = 0;
            }

            InitSalaryDictionary(scheduleList);
            return scheduleList;
        }
        private int GetDay(int shiftNumber)
        {
            return (shiftNumber / _shiftsPerDay) + 1;
        }
        public bool IsDayHoliday(int day)
        {
            return Month.FreeDaysNumbers.Contains(day);
        }
        public bool IsShiftHoliday(int shiftNumber)
        {
            return IsDayHoliday(GetDay(shiftNumber));
        }

        private static int GetLastShift(List<Shift> schedule, int workerId, int currentShift)
        {
            return schedule.FindLastIndex(currentShift, w => w.FirstWorker == workerId
                                                                            || w.SecondWorker == workerId);
        }
        private int GetBreakLength(List<Shift> schedule, int workerId, int currentShift)
        {
            int lastShift = GetLastShift(schedule, workerId, currentShift);
            if (lastShift == -1)
            {
                return _minimumBreak;
            }

            return (currentShift - lastShift) * (24 / _shiftsPerDay);
        }
        private void InitSalaryDictionary(List<Shift> schedule)
        {
            // Liczymy tylko tyle ile jest zmian, nie dla całej listy "schedule"
            // W liście schedule może być więcej pozycji niż rzeczywiście jest zmian
            // w miesiącu (16 * 12)
            for (int i=0; i < NumberOfShifts; i++)
            {
                if (IsShiftHoliday(i))
                {
                    SalaryPerWorker[schedule[i].FirstWorker] += _salaryPerShiftHoliday;
                    SalaryPerWorker[schedule[i].SecondWorker] += _salaryPerShiftHoliday;
                }
                else
                {
                    SalaryPerWorker[schedule[i].FirstWorker] += _salaryPerShift;
                    SalaryPerWorker[schedule[i].SecondWorker] += _salaryPerShift;
                }
            }
        }
        private void AddToSalary(int workerId, int newShiftNumber)
        {
            if(newShiftNumber < NumberOfShifts)
            {
                if(IsShiftHoliday(newShiftNumber))
                {
                    SalaryPerWorker[newShiftNumber] += _salaryPerShiftHoliday;
                }
                else
                {
                    SalaryPerWorker[newShiftNumber] += _salaryPerShiftHoliday;
                }
            }
        }
        private void RemoveFromSalary(int workerId, int prevShiftNumber)
        {
            if (prevShiftNumber < NumberOfShifts)
            {
                if (IsShiftHoliday(prevShiftNumber))
                {
                    SalaryPerWorker[prevShiftNumber] -= _salaryPerShiftHoliday;
                }
                else
                {
                    SalaryPerWorker[prevShiftNumber] -= _salaryPerShiftHoliday;
                }
            }
        }
        
        private int WorkerWithMaxSalary()
        {
            var richestWorkers = SalaryPerWorker.Where(x => x.Value == SalaryPerWorker.Max(v => v.Value));
            return richestWorkers.FirstOrDefault().Key;
        }

        private int WorkerWithMinSalary()
        {
            var richestWorkers = SalaryPerWorker.Where(x => x.Value == SalaryPerWorker.Min(v => v.Value));
            return richestWorkers.FirstOrDefault().Key;
        }

        private float GetDifferenceMinMaxSalary()
        {
            return SalaryPerWorker[WorkerWithMaxSalary()] - SalaryPerWorker[WorkerWithMinSalary()];
        }
        /******************* LEGACY ******************/
        //private int GetLastShift(int workerId, int currentShift)
        //{
        //    for (int i = 0; i < currentShift; i++)
        //    {
        //        if (ScheduleTable[i].FirstWorker == workerId || ScheduleTable[i].SecondWorker == workerId)
        //            return i;
        //    }
        //    return -1;
        //}
        //private int GetBreakLength(int workerId, int currentShift)
        //{
        //    int lastShift = GetLastShift(workerId, currentShift);
        //    if (lastShift == -1)
        //    {
        //        return minimumBreak;
        //    }

        //    return (currentShift - lastShift) * 8;
        //}
        //private float GetCurrentSalary(int workerId, int currentShift)
        //{
        //    float salary = 0;
        //    for (int i = 0; i < currentShift; i++)
        //    {
        //        if (ScheduleTable[i].FirstWorker == workerId || ScheduleTable[i].SecondWorker == workerId)
        //        {
        //            if (IsShiftHoliday(i))
        //            {
        //                salary += wagePerHolidayHour * (24 / ShiftsPerDay);
        //            }
        //            else
        //            {
        //                salary += wagePerHour * (24 / ShiftsPerDay);
        //            }
        //        }
        //    }

        //    return salary;
        //}


        //public Shift[] CreateSchedule()
        //{
        //    Queue<int> availableWorkers = new Queue<int>();
        //    foreach (var worker in Workers)
        //    {
        //        availableWorkers.Enqueue(worker);
        //    }

        //    for (int i = 0; i < NumberOfShifts; i++)
        //    {
        //        if (availableWorkers.Count == 0)
        //        {
        //            /** Powinien być kod sprawdzający którzy pracownicy mogą pracować **/
        //            foreach (var worker in Workers)
        //            {
        //                availableWorkers.Enqueue(worker);
        //            }
        //        }
        //        ScheduleTable[i].FirstWorker = availableWorkers.Dequeue();

        //        if (availableWorkers.Count == 0)
        //        {
        //            /** Powinien być kod sprawdzający którzy pracownicy mogą pracować **/
        //            foreach (var worker in Workers)
        //            {
        //                availableWorkers.Enqueue(worker);
        //            }
        //        }

        //        ScheduleTable[i].SecondWorker = availableWorkers.Dequeue();
        //    }

        //    return ScheduleTable;
        //}
    }
}
