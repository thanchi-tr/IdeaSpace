FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the specific module
COPY ./src/Modules/Crud ./Modules/Crud
COPY ./src/Shared ./Shared

# Go to Worker project
WORKDIR /src/Modules/Crud/Crud.API

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Crud.API.dll"]
