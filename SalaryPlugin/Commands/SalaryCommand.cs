using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using SalaryPlugin.Models;
using SalaryPlugin.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SalaryPlugin.Commands
{
    [Command("salary", Priority = OpenMod.API.Prioritization.Priority.Highest)]
    [CommandSyntax("Correct command usage: /salary <RoleId> <on/off>")]
    [CommandDescription("A command to set the status of a salary")]
    public class SalaryCommand : Command
    {
        private readonly ISalaryService _salaryService;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IConfiguration _configuration;

        public SalaryCommand(IConfiguration configuration,IStringLocalizer stringLocalizer, ISalaryService salaryService, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _configuration = configuration;
            _salaryService = salaryService;
            _stringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            if(Context.Parameters.Length < 2)
            {
                await Context.Actor.PrintMessageAsync(_stringLocalizer["Usage"], Color.Green);
                return;
            }

            if (await Context.Parameters.GetAsync<string>(1) != "on" && await Context.Parameters.GetAsync<string>(1) != "off")
            {
                await Context.Actor.PrintMessageAsync(_stringLocalizer["Usage"], Color.Green);
                return;
            }

            var salary = _configuration.GetSection("Salaries").Get<List<Salary>>().FirstOrDefault(x => x.RoleId!.ToLower() == Context.Parameters[0].ToLower());

            if(salary == null)
            {
                await Context.Actor.PrintMessageAsync(_stringLocalizer["Usage"], Color.Green);
                return;
            }

            if (await Context.Parameters.GetAsync<string>(1) == "on")
            {
                await _salaryService.StartSalaryService(salary!.RoleId!);
                await Context.Actor.PrintMessageAsync(_stringLocalizer["Finish", new { RoleId = salary.RoleId, Status = "on"}], Color.Green);
                return;
            }
            else
            {
                await _salaryService.StopSalaryService(salary!.RoleId!);
                await Context.Actor.PrintMessageAsync(_stringLocalizer["Finish", new { RoleId = salary.RoleId, Status = "off" }], Color.Green);
                return;
            }
        }
    }
}
