using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.Core.Jobs;

namespace SalaryPlugin;

public class ServiceConfigurator : IServiceConfigurator
{
    public void ConfigureServices(IOpenModServiceConfigurationContext openModStartupContext, IServiceCollection serviceCollection)
    {
        serviceCollection.Configure<JobExecutorOptions>(options =>
        {
            options.AddJobExecutor<SalaryTaskExecutor>();
        });
    }
}
