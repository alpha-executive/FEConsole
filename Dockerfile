
#build stage
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY *.sln .
COPY . .
RUN dotnet restore

# copy everything else and build app
WORKDIR /app/FE.Creator.FEConsolePortal
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/FE.Creator.FEConsolePortal/out ./
ENTRYPOINT ["dotnet", "FE.Creator.FEConsolePortal.dll"]


