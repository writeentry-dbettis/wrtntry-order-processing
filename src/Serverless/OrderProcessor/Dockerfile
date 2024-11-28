# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy all and build project
COPY . .
RUN dotnet build Serverless/OrderProcessor/OrderProcessor.csproj -c Debug -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080 

ENTRYPOINT ["dotnet", "OrderProcessor.dll"]
