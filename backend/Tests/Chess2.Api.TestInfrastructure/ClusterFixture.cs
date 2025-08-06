using Chess2.Api.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace Chess2.Api.TestInfrastructure;

public class ClusterFixture<TSiloConfigurator> : IDisposable
    where TSiloConfigurator : ISiloConfigurator, new()
{
    public TestCluster Cluster { get; } =
        new TestClusterBuilder().AddSiloBuilderConfigurator<TSiloConfigurator>().Build();

    public ClusterFixture()
    {
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
        GC.SuppressFinalize(this);
    }
}

public abstract class BaseSiloConfig : ISiloConfigurator
{
    public virtual void Configure(ISiloBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        builder.ConfigureServices(services =>
        {
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        });
    }
}
