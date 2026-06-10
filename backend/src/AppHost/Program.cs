using PromptifyWebApi.Shared;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env");

var databaseServer = builder
    .AddAzurePostgresFlexibleServer(Services.DatabaseServer)
    .WithPasswordAuthentication()
    .RunAsContainer(container => 
        container.WithLifetime(ContainerLifetime.Persistent))
    .AddDatabase(Services.Database);

var messaging = builder.AddRabbitMQ(Services.Messaging)
    .WithManagementPlugin();

var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WithReference(messaging)
    .WaitFor(databaseServer)
    .WaitFor(messaging)
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

builder.AddProject<Projects.Worker>(Services.Worker)
    .WithReference(databaseServer)
    .WithReference(messaging)
    .WaitFor(databaseServer)
    .WaitFor(messaging)
    .WithAspNetCoreEnvironment();

builder.Build().Run();
