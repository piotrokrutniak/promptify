using PromptifyWebApi.Shared;

namespace PromptifyWebApi.TestAppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        var postgres = builder.AddPostgres(Services.DatabaseServer)
            .AddDatabase(Services.Database);

        var messaging = builder.AddRabbitMQ(Services.Messaging);

        builder.AddProject<Projects.Worker>(Services.Worker)
            .WithReference(postgres)
            .WithReference(messaging)
            .WaitFor(postgres)
            .WaitFor(messaging);

        builder.Build().Run();
    }
}
