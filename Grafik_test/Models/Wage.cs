using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grafik_test.Models
{
    public class Wage
    {
        [Key]
        public int Id { get; set; }
        public float WagePerHour { get; set; }
        public bool IsHoliday { get; set; }

        public Wage()
        {

        }

        public Wage(int id, float wagePerHoue, bool isHoliday)
        {
            Id = id;
            wagePerHoue = WagePerHour;
            IsHoliday = isHoliday;
        }
    }
}
