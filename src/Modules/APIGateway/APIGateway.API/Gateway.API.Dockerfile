FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the specific module
COPY ./src/Modules/APIGateway ./Modules/APIGateway
COPY ./src/Shared ./Shared

# 🔐 Copy the development certificate into the image
COPY ./certs/devcert.pfx /https/devcert.pfx

# Go to Worker project
WORKDIR /src/Modules/APIGateway/APIGateway.API

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Optional: Set environment to Development or Production
ENV ASPNETCORE_ENVIRONMENT="Development"

# 🔒 Configure Kestrel to use the certificate
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="testpass"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/https/devcert.pfx"

ENTRYPOINT ["dotnet", "APIGateway.API.dll"]
