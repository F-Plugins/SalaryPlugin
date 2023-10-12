using OpenMod.API.Jobs;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;

namespace SalaryPlugin;

public class SalaryTaskExecutor : ITaskExecutor
{
    private readonly IUserDataStore _userDataStore;
    private readonly IEconomyProvider _economyProvider;
    private readonly IUserManager _userManager;

    public SalaryTaskExecutor(IUserDataStore userDataStore, IEconomyProvider economyProvider, IUserManager userManager)
    {
        _userDataStore = userDataStore;
        _economyProvider = economyProvider;
        _userManager = userManager;
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

        var (amount, roleId, message) = ParseArgs(task);
        var users = await _userDataStore.GetUsersDataAsync(KnownActorTypes.Player);

        var salaryTasks = users.Where(u => u.Roles?.Contains(roleId) ?? false).Select(u => PaySalaryAsync(u, amount, message));
        await Task.WhenAll(salaryTasks);
    }

    public bool SupportsType(string taskType)
    {
        return string.Equals(taskType, "salary", StringComparison.OrdinalIgnoreCase);
    }

    private (decimal amount, string roleId, string? message) ParseArgs(JobTask task)
    {
        var amount = task.Args["amount"]!.ToString();
        var roleId = task.Args["roleId"]!.ToString();
        string? message = null;

        if(task.Args.ContainsKey("message"))
        {
            message = task.Args["message"]!.ToString();
        }

        if (string.IsNullOrWhiteSpace(amount) || string.IsNullOrWhiteSpace(roleId))
        {
            throw new ArgumentException($"Job \"{task.JobName}\" either amount or roleId are empty");
        }

        return (decimal.Parse(amount), roleId, message);
    }

    private async Task PaySalaryAsync(UserData data, decimal amount, string? message)
    {
        await _economyProvider.UpdateBalanceAsync(data.Id!, data.Type!, amount, "salary");

        if (message is null)
            return;

        var user = await _userManager.FindUserAsync(data.Type!, data.Id!, UserSearchMode.FindById);
        if (user?.Session is null)
            return;

        await user.PrintMessageAsync(message);
    }
}
