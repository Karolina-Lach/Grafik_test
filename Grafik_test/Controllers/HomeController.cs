﻿using Grafik_test.Data;
using Grafik_test.Models;
using Grafik_test.Models.ViewModels;
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
            var test = _db.Set<Test>();
            var test1 = test.ToList();
            return View(new ScheduleVM());
        }

        [HttpPost]
        [Obsolete]
        public IActionResult Index(string month, string year, string minBreak, string minWeekend, string numberOfShifts)
        {
            if (month == "" || year == "")
            {
                return View(new ScheduleVM());
            }

            //List<Worker> workers = _db.Set<Worker>().ToList();
            //List<Wage> wages = _db.Set<Wage>().ToList();
            List<Worker> workers = new List<Worker>
            {
                new Worker(1, "A", "Aa"),
                new Worker(2, "B", "Bb"),
                new Worker(3, "C", "Cc"),
                new Worker(4, "D", "Dd"),
                new Worker(5, "E", "Ee"),
                new Worker(6, "F", "Ff"),
                new Worker(7, "G", "Gg"),
                new Worker(8, "H", "Hh"),
                new Worker(9, "I", "Ii"),
                new Worker(10, "J", "Jj"),
                new Worker(11, "K", "Kk"),
                new Worker(12, "L", "Ll")
            };

            List<Wage> wages = new List<Wage>
            {
                new Wage(1, 30, false),
                new Wage(2, 60, true)
            };


            ScheduleVM scheduleVM = new ScheduleVM(int.Parse(month), int.Parse(year), workers, wages,
                int.Parse(numberOfShifts), int.Parse(minBreak), int.Parse(minWeekend));
            scheduleVM.Schedule.CreateSchedule();
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(int.Parse(month));
            ViewBag.Name = char.ToUpper(monthName[0]) + monthName[1..] + " " + year;

            return View(scheduleVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
