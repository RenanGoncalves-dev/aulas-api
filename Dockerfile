FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Aulas.Api/Aulas.Api.csproj", "Aulas.Api/"]
RUN dotnet restore "Aulas.Api/Aulas.Api.csproj"

COPY . .
WORKDIR "/src/Aulas.Api"
RUN dotnet publish "Aulas.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aulas.Api.dll"]
