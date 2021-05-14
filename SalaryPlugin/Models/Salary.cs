using System;
using System.Collections.Generic;
using System.Text;

namespace SalaryPlugin.Models
{
    public class Salary
    {
        public string? RoleId { get; set; }
        public decimal Payment { get; set; }
        public int Timer { get; set; }
    }
}
