using Microsoft.Extensions.Configuration;
using OpenMod.API.Ioc;
using SalaryPlugin.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenMod.Extensions.Economy.Abstractions;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Core.Permissions;
using OpenMod.API.Permissions;
using System.Drawing;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Helpers;

namespace SalaryPlugin.Services
{
    [PluginServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class SalaryService : ISalaryService
    {
        private List<string> _salaries = new List<string>();

        private readonly IConfiguration _configuration;
        private readonly IUserDataStore _userDataStore;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly Lazy<IEconomyProvider> _economyProvider;
        private readonly IUserManager _userManager;

        public SalaryService(IUserDataStore userDataStore, IUserManager userManager, IStringLocalizer stringLocalizer, IConfiguration configuration, Lazy<IEconomyProvider> economyProvider)
        {
            _configuration = configuration;
            _userDataStore = userDataStore;
            _stringLocalizer = stringLocalizer;
            _userManager = userManager;
            _economyProvider = economyProvider;
        }

        public Task StartSalaryService(string salaryId)
        {
            if (!_salaries.Contains(salaryId))
            {
                _salaries.Add(salaryId);
                AsyncHelper.Schedule(salaryId, () => SalaryTimer(salaryId));
            }
            return Task.CompletedTask;
        }

        private async Task SalaryTimer(string salaryId)
        {
            while (_salaries.Contains(salaryId))
            {
                var salary = _configuration.GetSection("Salaries").Get<List<Salary>>().FirstOrDefault(x => x.RoleId == salaryId);
                if(salary != null)
                {
                    foreach (var player in await _userManager.GetUsersAsync(KnownActorTypes.Player))
                    {
                        if (player.Session == null) continue;

                        var playerData = await _userDataStore.GetUserDataAsync(player.Id, KnownActorTypes.Player);
                        if (playerData!.Permissions!.Contains(salary!.RoleId!))
                        {
                            await _economyProvider.Value.UpdateBalanceAsync(player.Id, KnownActorTypes.Player, salary.Payment, "Salary");
                            await player.PrintMessageAsync(_stringLocalizer["SalaryPayment", new { Money = salary.Payment, RoleId = salary.RoleId }], Color.Green);
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(salary.Timer));
                }
                else
                {
                    if (_salaries.Contains(salaryId))
                    {
                        _salaries.Remove(salaryId);
                    }
                }
            }
        }

        public Task StopSalaryService(string salaryId)
        {
            if (_salaries.Contains(salaryId))
            {
                _salaries.RemoveAll(x => x == salaryId);
            }
            return Task.CompletedTask;
        }
    }
}
