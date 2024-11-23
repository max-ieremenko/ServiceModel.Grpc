FROM mcr.microsoft.com/dotnet/sdk:9.0-noble

RUN apt-get update && \
    apt-get install -yq --no-install-recommends clang zlib1g-dev