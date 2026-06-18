FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/TwitterClone.Domain/TwitterClone.Domain.csproj src/TwitterClone.Domain/
COPY src/TwitterClone.Application/TwitterClone.Application.csproj src/TwitterClone.Application/
COPY src/TwitterClone.Infrastructure/TwitterClone.Infrastructure.csproj src/TwitterClone.Infrastructure/
COPY src/TwitterClone.Api/TwitterClone.Api.csproj src/TwitterClone.Api/

COPY src/ src/
RUN dotnet publish src/TwitterClone.Api/TwitterClone.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TwitterClone.Api.dll"]
