FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy only the project files to restore dependencies first
COPY src/EmployeePermissions.Core/EmployeePermissions.Core.csproj src/EmployeePermissions.Core/
COPY src/EmployeePermissions.Application/EmployeePermissions.Application.csproj src/EmployeePermissions.Application/
COPY src/EmployeePermissions.Infrastructure/EmployeePermissions.Infrastructure.csproj src/EmployeePermissions.Infrastructure/
COPY src/EmployeePermissions.Api/EmployeePermissions.Api.csproj src/EmployeePermissions.Api/

# Restore dependencies for the API project
RUN dotnet restore src/EmployeePermissions.Api/EmployeePermissions.Api.csproj

# Copy the remaining files
COPY . .

WORKDIR /app/src/EmployeePermissions.Api
RUN dotnet build EmployeePermissions.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish EmployeePermissions.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeePermissions.Api.dll"]
