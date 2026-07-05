# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["EmailService.csproj", "./"]
RUN dotnet restore "./EmailService.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/."
RUN dotnet publish "EmailService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Standard port for .NET 8 containers is 8080
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EmailService.dll"]
