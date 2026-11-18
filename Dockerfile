# Build na nuvem (Render). Não precisa ter Docker instalado no PC.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY HidroBairro.sln ./
COPY global.json ./
COPY src/HidroBairro.Domain/HidroBairro.Domain.csproj src/HidroBairro.Domain/
COPY src/HidroBairro.Application/HidroBairro.Application.csproj src/HidroBairro.Application/
COPY src/HidroBairro.Infrastructure/HidroBairro.Infrastructure.csproj src/HidroBairro.Infrastructure/
COPY src/HidroBairro.API/HidroBairro.API.csproj src/HidroBairro.API/
COPY tests/HidroBairro.UnitTests/HidroBairro.UnitTests.csproj tests/HidroBairro.UnitTests/
COPY tests/HidroBairro.IntegrationTests/HidroBairro.IntegrationTests.csproj tests/HidroBairro.IntegrationTests/

RUN dotnet restore HidroBairro.sln

COPY . .
RUN dotnet publish src/HidroBairro.API/HidroBairro.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render injeta a variável PORT em runtime.
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HidroBairro.API.dll"]
