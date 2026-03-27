FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY EatCompanion.sln .
COPY src/EatCompanion.Domain/EatCompanion.Domain.csproj src/EatCompanion.Domain/
COPY src/EatCompanion.Application/EatCompanion.Application.csproj src/EatCompanion.Application/
COPY src/EatCompanion.Infrastructure/EatCompanion.Infrastructure.csproj src/EatCompanion.Infrastructure/
COPY src/EatCompanion.Api/EatCompanion.Api.csproj src/EatCompanion.Api/
COPY tests/EatCompanion.Domain.Tests/EatCompanion.Domain.Tests.csproj tests/EatCompanion.Domain.Tests/
COPY tests/EatCompanion.Infrastructure.Tests/EatCompanion.Infrastructure.Tests.csproj tests/EatCompanion.Infrastructure.Tests/
RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish src/EatCompanion.Api -c Release -o /app/publish --no-restore

# Install EF tools and build migration bundle
RUN dotnet tool install --global dotnet-ef --version 9.0.*
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet ef migrations bundle --project src/EatCompanion.Infrastructure --startup-project src/EatCompanion.Api -o /app/publish/efbundle --self-contained

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

EXPOSE 5001
ENTRYPOINT ["./entrypoint.sh"]
