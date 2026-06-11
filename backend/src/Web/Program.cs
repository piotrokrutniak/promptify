using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Infrastructure.Messaging;
using PromptifyWebApi.Web.Consumers;
using PromptifyWebApi.Web.Hubs;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();
if (!builder.Configuration.GetValue<bool>("FunctionalTesting:DisableMessaging"))
{
    builder.AddPromptifyMessaging(x => x.AddConsumer<PromptStatusChangedConsumer>());
}

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Configuration.GetValue("Database:InitializeOnStartup", false))
{
    var recreate = app.Configuration.GetValue("Database:RecreateOnStartup", false);
    await app.InitialiseDatabaseAsync(recreate);
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Docker"))
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
var corsOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];
app.UseCors(policy => policy
    .WithOrigins(corsOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.UseFileServer();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseExceptionHandler(options => { });

app.Map("/", () => Results.Redirect("/scalar"));

app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);
app.MapHub<PromptStatusHub>("/hubs/prompts");

app.Run();
