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

        if (!task.Args.ContainsKey("online"))
        {
            throw new Exception($"Job \"{task.JobName}\" is missing the online field in args!");
        }

        var (amount, roleId, message, online) = ParseArgs(task);
        var users = await _userDataStore.GetUsersDataAsync(KnownActorTypes.Player);

        var salaryTasks = users.Where(u => u.Roles?.Contains(roleId) ?? false).Select(u => PaySalaryAsync(u.Id!, u.Type!, amount, message, online));
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

        if(task.Args.ContainsKey("message"))
        {
            message = task.Args["message"]!.ToString();
        }

        if (string.IsNullOrWhiteSpace(amount) || string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(online))
        {
            throw new ArgumentException($"Job \"{task.JobName}\" either amount, roleId or message are empty");
        }

        return (decimal.Parse(amount), roleId, message, bool.Parse(online));
    }

    private async Task PaySalaryAsync(string id, string type, decimal amount, string? message, bool online)
    {
        if(!online) 
            await _economyProvider.UpdateBalanceAsync(id, type, amount, "salary");

        if (!online && message is null)
            return;

        var user = await _userManager.FindUserAsync(id, type, UserSearchMode.FindById);
        if (user?.Session is null)
            return;

        if(online)
            await _economyProvider.UpdateBalanceAsync(id, type, amount, "salary");
        
        if(message is not null)
            await user.PrintMessageAsync(message);
    }
}
