FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props Directory.Build.props global.json ./
COPY PromptifyWebApi.slnx ./
COPY src/ src/

RUN dotnet restore src/Web/Web.csproj
RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PromptifyWebApi.Web.dll"]
