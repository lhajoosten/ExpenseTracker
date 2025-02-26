# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExpenseTracker.Api/ExpenseTracker.Api.csproj", "ExpenseTracker.Api/"]
COPY ["ExpenseTracker.Application/ExpenseTracker.Application.csproj", "ExpenseTracker.Application/"]
COPY ["ExpenseTracker.Domain/ExpenseTracker.Domain.csproj", "ExpenseTracker.Domain/"]
COPY ["ExpenseTracker.Infrastructure.Identity/ExpenseTracker.Infrastructure.Identity.csproj", "ExpenseTracker.Infrastructure.Identity/"]
COPY ["ExpenseTracker.Infrastructure.Mailing/ExpenseTracker.Infrastructure.Mailing.csproj", "ExpenseTracker.Infrastructure.Mailing/"]
COPY ["ExpenseTracker.Infrastructure.Persistence/ExpenseTracker.Infrastructure.Persistence.csproj", "ExpenseTracker.Infrastructure.Persistence/"]
RUN dotnet restore "./ExpenseTracker.Api/ExpenseTracker.Api.csproj"
COPY . .
WORKDIR "/src/ExpenseTracker.Api"
RUN dotnet build "./ExpenseTracker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExpenseTracker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ExpenseTracker.Api.dll"]