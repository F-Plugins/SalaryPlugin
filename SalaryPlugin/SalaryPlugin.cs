using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Core.Plugins;

[assembly: PluginMetadata("SalaryPlugin", Author = "Feli", Website = "discord.fplugins.com")]

namespace SalaryPlugin;

public class SalaryPlugin : OpenModUniversalPlugin
{
    public SalaryPlugin(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override Task OnLoadAsync()
    {
        Logger.LogInformation("Thanks for installing the SalaryPlugin by FPlugins !");
        Logger.LogWarning("Setting up this plugin is different than others. We recommend you checking this site for setup information. https://github.com/F-Plugins/SalaryPlugin");
        return Task.CompletedTask;
    }
}