# 1. Use the .NET SDK image as the build environment.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# 2. Set the working directory to a folder where source code will be copied.
WORKDIR /src

# 3. Copy the Web API project file into the container.
COPY API/GameSession.API.csproj ./API/

# 4. Copy other referenced project files (e.g., Application, Domain, Infrastructure).
COPY Application/GameSession.Application.csproj ./Application/
COPY Contracts/GameSession.Contracts.csproj ./Contracts/
COPY Domain/GameSession.Domain.csproj ./Domain/
COPY Infrastructure/GameSession.Infrastructure.csproj ./Infrastructure/

# 5. Restore dependencies.
RUN dotnet restore ./API/GameSession.API.csproj

# 6. Copy all source files for all projects.
COPY . .

# 7. Build and publish the Web API project.
WORKDIR /src/API
RUN dotnet publish -c Release -o /app

# Use the ASP.NET Core runtime image to run the app.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Expose the port your application will run on.
EXPOSE 8080

# Set the entry point to run the application.
ENTRYPOINT ["dotnet", "GameSession.API.dll"]
