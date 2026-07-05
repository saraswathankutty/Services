# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Publish the API project
RUN dotnet restore
RUN dotnet publish src/CareFlow.HMS.API/CareFlow.HMS.API.csproj -c Release -o /src/CareFlow.HMS.API/publish /p:UseAppHost=false

# Stage 2: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /src/CareFlow.HMS.API/publish .

# Expose port 10000 for Render
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "CareFlow.HMS.API.dll"]
