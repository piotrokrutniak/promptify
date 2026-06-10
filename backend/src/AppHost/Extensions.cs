internal static class AspireExtensions
{
    public static IResourceBuilder<T> WithAspNetCoreEnvironment<T>(this IResourceBuilder<T> builder) 
        where T : IResourceWithEnvironment
    {
        builder.WithEnvironment(context =>
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Development";
            context.EnvironmentVariables["DOTNET_ENVIRONMENT"] = environment;
            context.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = environment;
        });

        return builder;
    }
}