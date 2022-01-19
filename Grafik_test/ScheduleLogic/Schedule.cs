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


        /// <summary>
        /// Minimalna przerwa między zmianami
        /// </summary>
        readonly int _minimumBreak = 11;
        /// <summary>
        /// Weekend raz na 7 dni
        /// </summary>
        readonly int _minimumWeekend = 35;
        /// <summary>
        /// Stawka godzinowa w normalny dzień
        /// </summary>
        readonly float _wagePerHour = 30;
        /// <summary>
        /// Stawka godzinowa w święto
        /// </summary>
        readonly float _wagePerHolidayHour = 60;

        /// <summary>
        /// Stawki za całą zmianę
        /// </summary>
        readonly float _salaryPerShiftHoliday;
        readonly float _salaryPerShift;

        public int ShiftsPerDay = 3;

        public Month Month;
        public int NumberOfAvailableWorkers;
        /// <summary>
        /// Liczba zmian w miesiącu
        /// </summary>
        public int NumberOfShifts;
        /// <summary>
        /// Liczba pozycji do wypełnienia w miesiącu
        /// </summary>
        public int NumberOfPositions;

        /// <summary>
        /// Słownik z wynagrodzeniem dla każdego pracownika
        /// klucz - id pracownika
        /// wartość - wynagrodzenie
        /// </summary>
        public Dictionary<int, float> SalaryPerWorker;
        /// <summary>
        /// Słownik z liczbą weekendów dla każdego pracownika
        /// klucz - id pracownika
        /// wartość - liczba weekendów
        /// </summary>
        public Dictionary<int, int> WeekendsPerWorker;
        /// <summary>
        ///  Lista z grafikiem
        /// </summary>
        private List<int> ScheduleList { get; set; }

        /// <summary>
        /// Grafik w formie tabeli - na razie potrzebny do interfejsu, potem zobaczymy
        /// </summary>
        public Shift[] ScheduleTable { get; set; }

        /// <summary>
        /// List z id dla wszystkich pracowników
        /// </summary>
        public List<int> Workers;
        Random rnd = new Random();
        /************ CONSTRUCTORS *************************************************************************/

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
            // Inicjowanie właściwości
            Month = new Month(month, year);
            NumberOfAvailableWorkers = workers.Count;
            NumberOfShifts = Month.NumberOfDays * ShiftsPerDay;
            NumberOfPositions = NumberOfShifts * _workersPerShift;

            SalaryPerWorker = new Dictionary<int, float>();
            WeekendsPerWorker = new Dictionary<int, int>();

            // Stworzenie listy id na podstawie listy pracowników
            Workers = new List<int>();
            foreach (Worker worker in workers)
            {
                Workers.Add(worker.Id);
                SalaryPerWorker.Add(worker.Id, 0.0f);
                WeekendsPerWorker.Add(worker.Id, 0);
            }

            // inicjowanie stawek (zrobione w formie listy, ale wiadomo że u nas powinna mieć 2 elementy dla holiday i nie-holiday
            foreach (Wage wage in wages)
            {
                if (wage.IsHoliday)
                {
                    _wagePerHolidayHour = wage.WagePerHour;
                    _salaryPerShiftHoliday = _wagePerHolidayHour * ShiftsPerDay;
                }
                else
                {
                    _wagePerHour = wage.WagePerHour;
                    _salaryPerShift = _wagePerHour * ShiftsPerDay;
                }
            }
        }

        [Obsolete]
        public Schedule(int month, int year, List<Worker> workers, List<Wage> wages,
                        int numberOfShifts, int minBreak, int minWeekend) : this(month, year, workers, wages)
        {
            ShiftsPerDay = numberOfShifts;
            _minimumBreak = minBreak;
            _minimumWeekend = minWeekend;
        }

        /***********  GRAFIK  *******************************************************************************************/
        /// <summary>
        /// Funkcja układająca grafik
        /// </summary>
        /// <returns></returns>
        public Shift[] CreateSchedule()
        {
            ScheduleList = InitSchedule();
            ChangeSchedules();

            return CreateShiftArrayFromList();
        }

        /// <summary>
        /// Inicjowanie grafiki i słownika z wynagrodzeniem
        /// Tworzy listę o odpowiednim rozmiarze, przypisuje wszystkich pracowników po kolei
        /// Na podstawie wyznaczonej kolejności wylicza wszyskim pensję
        /// </summary>
        /// <returns></returns>
        private List<int> InitSchedule()
        {
            
            //int numberOfPositions = NumberOfShifts * _workersPerShift;

            //int listSize = (int)Math.Ceiling(Convert.ToDecimal(numberOfPositions) / Convert.ToDecimal(NumberOfAvailableWorkers));
            //List<int> scheduleList = new List<int>(listSize);
            //for (int i = 0; i < listSize; i++)
            //{
            //    scheduleList.AddRange(Workers);
            //}
            

            Random rnd = new Random();

            int numberOfPositions = NumberOfShifts * _workersPerShift;

            int listSize = (int)Math.Ceiling(Convert.ToDecimal(numberOfPositions) / Convert.ToDecimal(NumberOfAvailableWorkers));
            List<int> scheduleList = new List<int>(listSize);
            for (int i = 0; i < listSize; i++)
            {
                var workers = new List<int>(Workers);
                while (workers.Count > 0)
                {
                    int index = rnd.Next(0, workers.Count);
                    int workerId = workers[index];
                    scheduleList.Add(workerId);
                    workers.RemoveAt(index);
                }
            }


            UpdateSalaryDictionary(scheduleList);
            UpdateWeekendsDictionary(scheduleList);
            return scheduleList;
        }

        /// <summary>
        /// Funkcja wykonuje operację "insert" lub "switch".
        /// Jeśli jest to możliwe funkcja usuwa pracownika z losowo wybranego miejsca i wstawia go w losowe wybrane miejsce na liście.
        /// Jeśli insert nie jest możliwy ze względu na zadanego ograniczenia, sprawdzana jest możliwość wykonania operacji switch, tzn zamienienie miejscami 
        /// dwóch pracowników.
        /// Jeśli żadna z tych operacji nie jest możliwa, funkcja nie zmienia grafiku.
        /// </summary>
        /// <param name="numberOfInserts">Ilość opracji insert wykonanych do tej pory</param>
        /// <param name="numberOfSwitches">Ilość operacji switch wykonanych do tej pory</param>
        /// <returns>Uaktualnione liczby wykonanych operacji</returns>
        private Tuple<int, int> ChangeSchedulePosition(int numberOfInserts, int numberOfSwitches)
        {
            int from = rnd.Next(0, ScheduleList.Count);
            int to = rnd.Next(0, ScheduleList.Count);
            bool wasInsertPossible = InsertWorker(from, to);
            if (!wasInsertPossible)
            {
                bool wasSwitchPossible = SwitchWorkers(from, to);
                if (wasSwitchPossible)
                {
                    numberOfSwitches++;
                }
            }
            else
            {
                numberOfInserts++;
            }
            UpdateSalaryDictionary(ScheduleList);
            UpdateWeekendsDictionary(ScheduleList);
            return Tuple.Create(numberOfInserts, numberOfSwitches);
        }

        /// <summary>
        /// Funkcja zamienia miejscami dwóch pracowników podanych na wejściu, jeśli ograniczenia na to pozwalają
        /// </summary>
        /// <param name="positionInList1">Pozycja pierwszego pracownika</param>
        /// <param name="positionInList2">Pozycja drugiego pracownika</param>
        /// <returns>Zwraca informację, czy zamiana mogła zostać wykonana</returns>
        private bool SwitchWorkers(int positionInList1, int positionInList2)
        {
            bool isPositionPossible = false;
            if (IsPositionPossible(ScheduleList[positionInList1], positionInList2) &&
                IsPositionPossible(ScheduleList[positionInList2], positionInList1))
            {
                isPositionPossible = true;
                int temp = ScheduleList[positionInList1];
                ScheduleList[positionInList1] = ScheduleList[positionInList2];
                ScheduleList[positionInList2] = temp;
            }

            return isPositionPossible;
        }


        /// <summary>
        /// Główna pętla programu.
        /// W pętli grafik jest modyfikowany za pomocą funkcji ChangeSchedulePositions. 
        /// Następnie wyznaczana jest nowa różnica pomiędzy wynagrodzeniem pracowników oraz różnica między liczbą przepracowanych weekendów i świąt.
        /// Zapamiętywane jest rozwiązanie z najmniejszą różnicą w wynagrodzeniach.
        /// W przypadku znalezienie rozwiązania o takiej samej różnicy, co zapamiętana, sprawdzana jest różnica w liczbie weekendów. 
        /// Jeśli ta różnica jest mniejsza- zapamiętywane jest nowe rozwiązanie.
        /// 
        /// Zmienne numberOfInserts i numberOfSwitches były wykorzystywane podczas debugowanie w celu porównanie ile razy grafik jest rzeczywiście modyfikowany.
        /// </summary>
        private void ChangeSchedules()
        {
            int numberOfInserts = 0, numberOfSwitches = 0;
            float bestDifference = 10000.0f;
            int bestDifferenceWeekends = 10000;
            List<int> bestScheduleList = new List<int>();
            for (int i = 0; i < 10000; i++)
            {
                (numberOfInserts, numberOfSwitches) = ChangeSchedulePosition(numberOfInserts, numberOfSwitches);
                float currentDifference = GetDifferenceMinMaxSalary();
                int currentDifferenceWeekends = GetDifferenceMinMaxWeekends();
                if (currentDifference < bestDifference)
                {
                    bestDifference = currentDifference;
                    bestDifferenceWeekends = currentDifferenceWeekends;
                    bestScheduleList = new List<int>(ScheduleList);
                }
                else if(currentDifference == bestDifference)
                {
                    if (currentDifferenceWeekends < bestDifferenceWeekends)
                    {
                        bestDifference = currentDifference;
                        bestDifferenceWeekends = currentDifferenceWeekends;
                        bestScheduleList = new List<int>(ScheduleList);
                    }
                }
                
            }
            ScheduleList = bestScheduleList;
            UpdateSalaryDictionary(ScheduleList);
            UpdateWeekendsDictionary(ScheduleList);
        }

        /// <summary>
        /// Funkcja usuwa pracownika z wskazanego miejsca do wskazanego miejsca, jeśli pozwalają na to ograniczenia.
        /// </summary>
        /// <param name="from">Pozycja na liście, z której trzeba usunąć pracownika</param>
        /// <param name="to">Pozycja na liście, na którą trzeba wstawić pracownika</param>
        /// <returns>Zwraca informację, czy operacja mogła zostać wykonana</returns>
        public bool InsertWorker(int from, int to)
        {
            int tempElement = ScheduleList[from];
            bool isChangePossible = true;
            if (IsPositionPossible(tempElement, to))
            {
                Insert(from, to, tempElement);
                if (from > to)
                {
                    for (int i = to + 1; i <= from; i++)
                    {
                        int workerId = ScheduleList[i];
                        if (!IsPositionPossible(workerId, i))
                        {
                            isChangePossible = false;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = from; i < to; i++)
                    {
                        int workerId = ScheduleList[i];
                        if (!IsPositionPossible(workerId, i))
                        {
                            isChangePossible = false;
                            break;
                        }
                    }
                }
                if (!isChangePossible)
                {
                    Insert(to, from, tempElement);
                }
            }

            return isChangePossible;
        }
        
        /// <summary>
        /// Wstawienie zadanej wartości value na pozycję to i usunięcie tej wartości z pozycji from
        /// </summary>
        /// <param name="from">Skąd usunąć</param>
        /// <param name="to">Gdzie wstawić</param>
        /// <param name="value">Wartość do usunięcia/wstawienia</param>
        private void Insert(int from, int to, int value)
        {
            ScheduleList.RemoveAt(from);
            ScheduleList.Insert(to, value);
        }
        /****************************************************************************************************************/
        /// <summary>
        /// Funkcja zwraca nr zmiany na podstawie pozycji w grafiku. Zmiany numerowane są od 0 (dlatego, że takiej numeracji w pierwotnej wersji oczekiwała
        /// funkcja GetDay).
        /// Numer zmiany zmienia się co 2 (bo mamy 2 pracowników na zmianę), więc np dla pozycji 0,1 -> zwrócone 0
        ///                                                                                      2,3 -> zwrócione 1 itd.
        /// </summary>
        /// <param name="positionInList">
        /// Numer pozycji w liście scheduleList.
        /// </param>
        /// <returns>Numer zmiany</returns>
        private int GetShiftNumber(int positionInList)
        {
            return (positionInList / _workersPerShift);
        }

        /// <summary>
        /// Funkcja zwraca numer dnia miesiąca na podstawie pozycji w grafiku.
        /// Dzień miesiąca zmmienia się co 6 pozycji w liście (bo są 3 zmiany na dzień po 2 pracowników), więc np dla pozycji 0-5 -> zwrócone 1
        /// </summary>
        /// <param name="positionInList">Pozycja w grafiku</param>
        /// <returns>Dzień miesiąca</returns>
        private int GetDayFromPositionInList(int positionInList)
        {
            return GetDayFromShift(GetShiftNumber(positionInList));
        }

        /// <summary>
        /// Zwraca dzień miesiąca na podstawie numeru zmiany. 
        /// Dzień miesiąca zmmienia się co 3 zmiany, więc np dla zmian 0-2 -> zwrócone 1
        /// </summary>
        /// <param name="shiftNumber">Numer zmiany</param>
        /// <returns>Dzień miesiąca</returns>
        public int GetDayFromShift(int shiftNumber)
        {
            return (shiftNumber / ShiftsPerDay) + 1;
        }

        /**************** WYZNACZANIE CZY DZIEŃ WOLNY CZY NIE ****************/

        /// <summary>
        /// Sprawdzenie czy dzień miesiąca jest świętem (lub weekendem) czy nie. 
        /// Sprawdza czy podany dzień znajduje się na liście wolnych dni w obiekcie Month.
        /// </summary>
        /// <param name="day">Dzień miesiąca</param>
        /// <returns></returns>
        public bool IsDayHoliday(int day)
        {
            return Month.FreeDaysNumbers.Contains(day);
        }

        /// <summary>
        /// Sprawdzenie czy numer zmiany wypada w święto (lub weekend) czy nie. 
        /// Na podstawie numeru zmiany oblicza dzień miesiąca i sprawdza czy podany dzień znajduje się na liście wolnych dni w obiekcie Month.
        /// </summary>
        /// <param name="day">Numer zmiany</param>
        /// <returns></returns>
        public bool IsShiftHoliday(int shiftNumber)
        {
            return IsDayHoliday(GetDayFromShift(shiftNumber));
        }

        /// <summary>
        /// Sprawdzenie czy pozycja na liście w grafiku wypada w święt (lub weekend) czy nie. 
        /// Na podstawie pozycji na liście oblicza dzień miesiąca i sprawdza czy podany dzień znajduje się na liście wolnych dni w obiekcie Month.
        /// </summary>
        /// <param name="day">Numer pozycji na liście</param>
        /// <returns></returns>
        public bool IsPositionInListHoliday(int positionInList)
        {
            return IsDayHoliday(GetDayFromPositionInList(positionInList));
        }


        /*********** WYZNACZANIE PRZERWY ***************/
        /// <summary>
        /// Zwraca ostatnie pojawienie się pracownika w grafiku tzn. ostatnie pojewienie się workerId na lewo od currentPosition 
        /// </summary>
        /// <param name="workerId">id pracownika</param>
        /// <param name="currentPosition">pozycja od której liczymy ostatnie wystąpienie</param>
        /// <returns></returns>
        private int GetLastPositionInList(int workerId, int currentPosition)
        {
            if (currentPosition > 0)
                return ScheduleList.FindLastIndex(currentPosition - 1, w => w == workerId);
            else
                return -1;

        }

        /// <summary>
        /// Zwraca kolejne pojawienie się pracownika w grafiku tzn. kolejne pojewienie się workerId na prawo od currentPosition 
        /// </summary>
        /// <param name="workerId">id pracownika</param>
        /// <param name="currentPosition">pozycja od której liczymy ostatnie wystąpienie</param>
        /// <returns></returns>
        private int GetNextPositionInList(int workerId, int currentPosition)
        {
            if (currentPosition < ScheduleList.Count)
                return ScheduleList.FindIndex(currentPosition + 1, w => w == workerId);
            else
                return -1;
        }

        /// <summary>
        /// Zwraca przerwę (w godzinach) między dwomoa pozycjami na liście.
        /// 
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="workerId"></param>
        /// <param name="position1"></param>
        /// <param name="position2"></param>
        /// <returns></returns>
        private int GetBreakBetweenPositions(int position1, int position2)
        {
            int firstShift = GetShiftNumber(position1);
            int secondShift = GetShiftNumber(position2);
            return (Math.Abs(value: firstShift - secondShift) - 1) * (24 / ShiftsPerDay);
        }

        /// <summary>
        /// Zwraca przerwę (w godzinach) dla pracownika między podaną pozycją na liście a jego ostatnią zmianą (ostatnią pozycją, gdzie pojawiło się id pracownika)
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private int GetLastBreakLength(int workerId, int currentPosition)
        {
            int lastShift = GetLastPositionInList(workerId, currentPosition);
            if (lastShift == -1)
            {
                return _minimumBreak;
            }

            return GetBreakBetweenPositions(currentPosition, lastShift);
        }

        /// <summary>
        /// Zwraca przerwę (w godzinach) dla pracownika między podaną pozycją na liście a jego następną zmianą (następną pozycją, gdzie pojawiło się id pracownika)
        /// </summary>
        /// <param name="workerId"></param>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private int GetNextBreakLength(int workerId, int currentPosition)
        {
            int lastShift = GetNextPositionInList(workerId, currentPosition);
            if (lastShift == -1)
            {
                return _minimumBreak;
            }

            return GetBreakBetweenPositions(currentPosition, lastShift);
        }

        /// <summary>
        /// Funkcja sprawdza dany pracownik może pracować na podanej zmianie (czy spełnione są ograniczenia)
        /// </summary>
        /// <param name="workerId">Id pracownika</param>
        /// <param name="newPosition">Pozycja na liście</param>
        /// <returns></returns>
        private bool IsPositionPossible(int workerId, int newPosition)
        {
            int lastBreakLength = GetLastBreakLength(workerId, newPosition);
            int nextBreakLength = GetNextBreakLength(workerId, newPosition);
            return (lastBreakLength >= _minimumBreak) && (nextBreakLength >= _minimumBreak);
        }

        /************* SALARY ****************************/
        /// <summary>
        /// Aktualizuje cały słownik z wypłatami dla pracownikow. Iteruje po liście z grafikiem i dla każdego id dodaje odpowiednią wypłatę (w zależności od
        /// tego czy numer pozycji na liście wypada w dzień wolny czy nie
        /// </summary>
        /// <param name="schedule"></param>
        private void UpdateSalaryDictionary(List<int> schedule)
        {
            SalaryPerWorker.Keys.ToList().ForEach(i => SalaryPerWorker[i] = 0);

            // Liczymy tylko tyle ile jest zmian, nie dla całej listy "schedule"
            // W liście schedule może być więcej pozycji niż rzeczywiście jest zmian
            // w miesiącu (16 * 12)
            for (int i = 0; i < NumberOfPositions; i++)
            {
                if (IsPositionInListHoliday(i))
                {
                    SalaryPerWorker[schedule[i]] += _salaryPerShiftHoliday;
                }
                else
                {
                    SalaryPerWorker[schedule[i]] += _salaryPerShift;
                }
            }
        }




        /// <summary>
        /// Dodaje do aktualnej wypłaty pracownika workerId pensję, jaka należy się za positionNumber. 
        /// Pensja akutalizowana jest tylko wtedy, gdy numer pozycji na liście nie wykracza rzeczywistej liczby pozycji w danym miesiącu
        /// </summary>
        /// <param name="workerId">id pracownika</param>
        /// <param name="positionNumber">Numer pozycji, za którą trzeba dodać pensję</param>
        private void AddToSalary(int workerId, int positionNumber)
        {
            if (positionNumber < NumberOfPositions)
            {
                if (IsPositionInListHoliday(positionNumber))
                {
                    SalaryPerWorker[workerId] += _salaryPerShiftHoliday;
                }
                else
                {
                    SalaryPerWorker[workerId] += _salaryPerShiftHoliday;
                }
            }
        }

        /// <summary>
        /// Odejmuje od aktualnej wypłaty pracownika workerId pensję, jaka należy się za positionNumber. 
        /// Pensja akutalizowana jest tylko wtedy, gdy numer pozycji na liście nie wykracza rzeczywistej liczby pozycji w danym miesiącu
        /// </summary>
        /// <param name="workerId">id pracownika</param>
        /// <param name="positionNumber">Numer pozycji, za którą trzeba odjąć pensję</param>
        private void RemoveFromSalary(int workerId, int positionNumber)
        {
            if (positionNumber < NumberOfPositions)
            {
                if (IsPositionInListHoliday(positionNumber))
                {
                    SalaryPerWorker[workerId] -= _salaryPerShiftHoliday;
                }
                else
                {
                    SalaryPerWorker[workerId] -= _salaryPerShiftHoliday;
                }
            }
        }

        /// <summary>
        /// Zwraca id pracownika z największą pensją w słowniku.
        /// </summary>
        /// <returns></returns>
        public int WorkerIdWithMaxSalary()
        {
            var richestWorkers = SalaryPerWorker.Where(x => x.Value == SalaryPerWorker.Max(v => v.Value));
            return richestWorkers.FirstOrDefault().Key;
        }

        /// <summary>
        /// Zwraca id pracownika z najmniejszą pensją w słowniku.
        /// </summary>
        /// <returns></returns>
        public int WorkerIdWithMinSalary()
        {
            var poorestWorkers = SalaryPerWorker.Where(x => x.Value == SalaryPerWorker.Min(v => v.Value));
            return poorestWorkers.FirstOrDefault().Key;
        }

        /// <summary>
        /// Zwraca różnice między największą i najmniejszą pensją
        /// </summary>
        /// <returns></returns>
        public float GetDifferenceMinMaxSalary()
        {
            return SalaryPerWorker[WorkerIdWithMaxSalary()] - SalaryPerWorker[WorkerIdWithMinSalary()];
        }

        /// <summary>
        /// Zamienia grafik w postaci listy id pracowników na tablicę.
        /// </summary>
        /// <returns></returns>
        public Shift[] CreateShiftArrayFromList()
        {
            ScheduleTable = new Shift[NumberOfShifts];
            int j = 0;
            for (int i = 0; i < NumberOfShifts; i++)
            {
                ScheduleTable[i].SetFirstWorker(ScheduleList[j]);
                ScheduleTable[i].SetSecondWorker(ScheduleList[j + 1]);

                j = j + 2;
            }

            return ScheduleTable;
        }

        /// <summary>
        /// Aktualizuje cały słownik z liczbą weekendów dla pracownikow. Iteruje po liście z grafikiemm, sprawdza czy zmiana jest w weekend/święto i zlicza te zmiany.
        /// </summary>
        /// <param name="schedule"></param>
        private void UpdateWeekendsDictionary(List<int> schedule)
        {
            WeekendsPerWorker.Keys.ToList().ForEach(i => WeekendsPerWorker[i] = 0);

            // Liczymy tylko tyle ile jest zmian, nie dla całej listy "schedule"
            // W liście schedule może być więcej pozycji niż rzeczywiście jest zmian
            // w miesiącu (16 * 12)
            for (int i = 0; i < NumberOfPositions; i++)
            {
                if (IsPositionInListHoliday(i))
                {
                    WeekendsPerWorker[schedule[i]]++;
                }
            }
        }

        /// <summary>
        /// Zwraca id pracownika z największą liczbą weekendów w słowniku WeekendsPerWorker.
        /// </summary>
        /// <returns></returns>
        public int WorkerIdWithMaxWeekends()
        {
            var weekendWorkers = WeekendsPerWorker.Where(x => x.Value == WeekendsPerWorker.Max(v => v.Value));
            return weekendWorkers.FirstOrDefault().Key;
        }

        /// <summary>
        /// Zwraca id pracownika z najmniejszą liczbą weekendów w słowniku WeekendsPerWorker.
        /// </summary>
        /// <returns></returns>
        public int WorkerIdWithMinWeekends()
        {
            var weekWorkers = WeekendsPerWorker.Where(x => x.Value == WeekendsPerWorker.Min(v => v.Value));
            return weekWorkers.FirstOrDefault().Key;
        }

        /// <summary>
        /// Zwraca różnice między największą i najmniejszą liczbą weekendów
        /// </summary>
        /// <returns></returns>
        public int GetDifferenceMinMaxWeekends()
        {
            return WeekendsPerWorker[WorkerIdWithMaxWeekends()] - WeekendsPerWorker[WorkerIdWithMinWeekends()];
        }

        /// <summary>
        /// Funkcja zwraca liczbę przepracowanych zmian w weekendy/święta dla zadanego pracownika.
        /// </summary>
        /// <param name="worker">Id pracownika</param>
        /// <returns>Liczba przepracowanych zmian</returns>
        public int GetWorkerShiftsWeekends(int worker)
        {
            int shifts = 0;
            for (int i = 0; i < NumberOfPositions; i++)
            {
                if (IsPositionInListHoliday(i))
                {
                    if (ScheduleList[i] == worker)
                        shifts++;
                }
            }
            return shifts;
        }

        /// <summary>
        /// Funkcja zwraca liczbę wszystkich zmian dla zadanego pracownika.
        /// </summary>
        /// <param name="worker">Id pracownika</param>
        /// <returns>Liczba przepracowanych zmian</returns>
        public int GetWorkerShifts(int worker)
        {
            int shifts = 0;
            for (int i = 0; i < NumberOfPositions; i++)
            {
                if (ScheduleList[i] == worker)
                    shifts++;
            }
            return shifts;
        }

    }
}
