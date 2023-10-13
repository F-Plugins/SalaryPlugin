using OpenMod.API.Jobs;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using OpenMod.Core.Permissions;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;

namespace SalaryPlugin;

public class SalaryTaskExecutor : ITaskExecutor
{
    private readonly IEconomyProvider _economyProvider;
    private readonly IUserManager _userManager;
    private readonly IPermissionRoleStore _permissionRoleStore;

    public SalaryTaskExecutor(IEconomyProvider economyProvider, IUserManager userManager, IPermissionRoleStore permissionRoleStore)
    {
        _economyProvider = economyProvider;
        _userManager = userManager;
        _permissionRoleStore = permissionRoleStore;
    }

    public async Task ExecuteAsync(JobTask task)
    {
        if (!task.Args.ContainsKey("amount"))
        {
            throw new Exception($"Job \"{task.JobName}\" is missing the amount field in args!");
        }

        if (!task.Args.ContainsKey("roleId"))
        {
            throw new Exception($"Job \"{task.JobName}\" is missing the roleId field in args!");
        }

        if (!task.Args.ContainsKey("online"))
        {
            throw new Exception($"Job \"{task.JobName}\" is missing the online field in args!");
        }

        var (amount, roleId, message, online) = ParseArgs(task);
        var users = await _userManager.GetUsersAsync(KnownActorTypes.Player);

        var salaryTasks = users.Select(async u =>
        {
            var roles = await _permissionRoleStore.GetRolesAsync(u, true);

            if (!roles.Any(c => string.Equals(c.Id, roleId, StringComparison.OrdinalIgnoreCase)))
                return;

            await PaySalaryAsync(u, amount, message, online);
        });

        await Task.WhenAll(salaryTasks);
    }

    public bool SupportsType(string taskType)
    {
        return string.Equals(taskType, "salary", StringComparison.OrdinalIgnoreCase);
    }

    private (decimal amount, string roleId, string? message, bool online) ParseArgs(JobTask task)
    {
        var amount = task.Args["amount"]!.ToString();
        var roleId = task.Args["roleId"]!.ToString();
        var online = task.Args["online"]!.ToString();
        string? message = null;

        if (task.Args.ContainsKey("message"))
        {
            message = task.Args["message"]!.ToString();
        }

        if (string.IsNullOrWhiteSpace(amount) || string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(online))
        {
            throw new ArgumentException($"Job \"{task.JobName}\" either amount, roleId or message are empty");
        }

        return (decimal.Parse(amount), roleId, message, bool.Parse(online));
    }

    private async Task PaySalaryAsync(IUser user, decimal amount, string? message, bool online)
    {
        if (!online)
            await _economyProvider.UpdateBalanceAsync(user.Id, user.Type, amount, "salary");

        if (user?.Session is null)
            return;

        if (online)
            await _economyProvider.UpdateBalanceAsync(user.Id, user.Type, amount, "salary");

        if (message is not null)
            await user.PrintMessageAsync(message);
    }
}
