# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để cache restore
COPY FitupProject.sln ./
COPY FitupProject/FitupProject.csproj FitupProject/
COPY FitupProject.BLL/FitupProject.BLL.csproj FitupProject.BLL/
COPY FitupProject.DAL/FitupProject.DAL.csproj FitupProject.DAL/
COPY FitupProject.Core/FitupProject.Core.csproj FitupProject.Core/

RUN dotnet restore

# Copy source
COPY . .

WORKDIR /src/FitupProject
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Container listen 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FitupProject.dll"]
