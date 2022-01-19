using Grafik_test.Data;
using Grafik_test.Models;
using Grafik_test.Models.ViewModels;
using Grafik_test.ScheduleLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grafik_test.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            List<Wage> wages = new List<Wage>();
            wages = CreateWagesList();

            float weekWagePerHour = wages.First(wage => wage.IsHoliday == false).WagePerHour;
            float weekendWagePerHour = wages.First(wage => wage.IsHoliday == true).WagePerHour;
            return View(new ScheduleVM(weekWagePerHour, weekendWagePerHour));
        }

        [HttpPost]
        [Obsolete]
        public IActionResult Index(string month, string year, string minBreak, string minWeekend, string numberOfShifts)
        {
            if (month == "" || year == "")
            {
                return View(new ScheduleVM());
            }
            List<Worker> workers = new List<Worker>();
            workers = CreateListOfWorkers();

            List<Wage> wages = new List<Wage>();
            wages = CreateWagesList();

            float weekWagePerHour = wages.First(wage => wage.IsHoliday == false).WagePerHour;
            float weekendWagePerHour = wages.First(wage => wage.IsHoliday == true).WagePerHour;
            ScheduleVM scheduleVM = new ScheduleVM(int.Parse(month), int.Parse(year), workers, wages,
                int.Parse(numberOfShifts), int.Parse(minBreak), int.Parse(minWeekend), weekendWagePerHour, weekendWagePerHour);
            scheduleVM.Schedule.CreateSchedule();
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(int.Parse(month));
            ViewBag.Name = char.ToUpper(monthName[0]) + monthName[1..] + " " + year;
            return View(scheduleVM);
        }


        public List<Worker> CreateListOfWorkers()
        {

            string fileWorker = "Data/workers.txt";
            string line;

            System.IO.StreamReader sr = new System.IO.StreamReader(fileWorker);
            List<Worker> workers = new List<Worker>();

            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                Worker w = new Worker(Int32.Parse(parts[0]), parts[1].Trim(), parts[2].Trim());
                workers.Add(w);

            }
            sr.Close();

            return workers;
        }

        public List<Wage> CreateWagesList()
        {
            string fileWages = "Data/wages.txt";
            string line;

            System.IO.StreamReader sr = new System.IO.StreamReader(fileWages);
            List<Wage> wages = new List<Wage>();

            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                bool isHol = (parts[0].Trim() == "2");

                Wage w = new Wage(Int32.Parse(parts[0]), Int32.Parse(parts[1]), isHol);
                wages.Add(w);

            }
            sr.Close();

            return wages;
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
