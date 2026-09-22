FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TravelPlanner.csproj", "./"]
RUN dotnet restore "TravelPlanner.csproj"

COPY . .
RUN dotnet publish "TravelPlanner.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Frontend is served through a PhysicalFileProvider, so it is not included in
# the ASP.NET publish output and must be copied into the runtime image.
COPY --from=build /src/Frontend ./Frontend

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "TravelPlanner.dll"]
