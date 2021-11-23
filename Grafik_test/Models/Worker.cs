using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grafik_test.Models
{
    public class Worker
    {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public Worker(int id, string name, string lastName)
            {
                Id = id;
                Name = name;
                LastName = lastName;
            }

            public Worker()
            {

            }
        
    }
}
