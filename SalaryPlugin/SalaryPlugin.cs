using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.Core.Plugins;
using OpenMod.API.Plugins;
using SalaryPlugin.Services;
using SalaryPlugin.Models;
using System.Collections.Generic;

[assembly: PluginMetadata("Feli.SalaryPlugin", DisplayName = "SalaryPlugin", Website = "fplugins.com", Author = "Feli")]
namespace SalaryPlugin
{
    public class SalaryPlugin : OpenModUniversalPlugin
    {
        private readonly ISalaryService _salaryService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalaryPlugin> _logger;

        public SalaryPlugin(
            ILogger<SalaryPlugin> logger,
            IConfiguration configuration,
            ISalaryService salaryService,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _salaryService = salaryService;
        }

        protected override async Task OnLoadAsync()
        {
            _logger.LogInformation("Salart Plugin 1.0.3 has been loaded");
            _logger.LogInformation("Get more plugins at fplugins.com");
            _logger.LogInformation("Setting up salaries");
            foreach (var salary in _configuration.GetSection("Salaries").Get<List<Salary>>())
            {
                _logger.LogInformation("Starting " + salary.RoleId);
                await _salaryService.StartSalaryService(salary.RoleId!);
            }
        }

        protected override async Task OnUnloadAsync()
        {
            _logger.LogInformation("Stopping salaries");
            foreach (var salary in _configuration.GetSection("Salaries").Get<List<Salary>>())
            {
                _logger.LogInformation("Stopping " + salary.RoleId);
                await _salaryService.StopSalaryService(salary.RoleId!);
            }
            _logger.LogInformation("Salart Plugin 1.0.3 has been unloaded");
        }
    }
}
