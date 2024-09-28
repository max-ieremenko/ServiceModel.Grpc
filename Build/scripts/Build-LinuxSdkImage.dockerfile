FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy

RUN apt-get update && \
    apt-get install -yq --no-install-recommends clang zlib1g-dev