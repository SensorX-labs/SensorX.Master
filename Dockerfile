FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SensorX.Master.WebApi/SensorX.Master.WebApi.csproj", "SensorX.Master.WebApi/"]
COPY ["SensorX.Master.Infrastructure/SensorX.Master.Infrastructure.csproj", "SensorX.Master.Infrastructure/"]
COPY ["SensorX.Master.Application/SensorX.Master.Application.csproj", "SensorX.Master.Application/"]
COPY ["SensorX.Master.Domain/SensorX.Master.Domain.csproj", "SensorX.Master.Domain/"]

RUN dotnet restore "SensorX.Master.WebApi/SensorX.Master.WebApi.csproj"

COPY . .
RUN dotnet publish "SensorX.Master.WebApi/SensorX.Master.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SensorX.Master.WebApi.dll"]