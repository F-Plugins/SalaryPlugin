﻿using OpenMod.API.Ioc;
using SalaryPlugin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SalaryPlugin.Services
{
    [Service]
    public interface ISalaryService
    {
        Task StartSalaryService(string salaryId);

        Task StopSalaryService(string salaryId);
    }
}
