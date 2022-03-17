using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoneyTrackr.Data.ExtensionMethods;
using MoneyTrackr.Data.Settings;

namespace MoneyTrackr.Data
{
    public static class Configure
    {
        public static IServiceCollection SetupMoneyTrackrDataAccess(this IServiceCollection services, DataConfigurationSettings configSettings)
        {
            if (configSettings is null)
                throw new ArgumentNullException(nameof(configSettings));

            if (configSettings.AppAssemblyName is null)
                throw new ArgumentNullException($"{nameof(configSettings)}.{nameof(configSettings.AppAssemblyName)}");

            services.ConfigureSettings<DatabaseSettings>((settings, config) => 
                settings.MapConfiguration(config));

            services.AddDbContext<MoneyTrackrDbContext>((s, opts) =>
            {
                var dbSettings = s.GetRequiredService<IOptionsMonitor<DatabaseSettings>>();

                opts.UseNpgsql(
                    dbSettings.CurrentValue.GetConnectionString(), 
                    b => b.MigrationsAssembly(configSettings.AppAssemblyName));
            });

            // Add Repositories here

            return services;
        }

        private static IServiceCollection ConfigureSettings<TOptions>(this IServiceCollection services, Action<TOptions, IConfiguration> configure) where TOptions : class
        {
            services
                .AddSingleton<IOptionsChangeTokenSource<TOptions>>(s => new ConfigurationChangeTokenSource<TOptions>(s.GetService<IConfiguration>()))
                .AddOptions<TOptions>()
                .Configure(configure);

            return services;
        }
    }
}
