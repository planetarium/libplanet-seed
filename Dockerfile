FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Libplanet.Seed/Libplanet.Seed.csproj ./Libplanet.Seed/
COPY ./Libplanet.Seed.Executable/Libplanet.Seed.Executable.csproj ./Libplanet.Seed.Executable/
RUN dotnet restore Libplanet.Seed
RUN dotnet restore Libplanet.Seed.Executable

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -r linux-x64 -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .

# Install native deps & utilities for production
RUN apt-get update \
    && apt-get install -y --allow-unauthenticated \
        libc6-dev jq \
     && rm -rf /var/lib/apt/lists/*

# Runtime settings
EXPOSE 5000
VOLUME /data

ENTRYPOINT ["dotnet", "Libplanet.Seed.Executable.dll"]