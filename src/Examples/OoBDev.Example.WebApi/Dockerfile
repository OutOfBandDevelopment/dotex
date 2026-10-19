# Build context is the repository root:  docker build -f src/Examples/OoBDev.Example.WebApi/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY . .
RUN dotnet publish src/Examples/OoBDev.Example.WebApi/OoBDev.Example.WebApi.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "OoBDev.Example.WebApi.dll"]
