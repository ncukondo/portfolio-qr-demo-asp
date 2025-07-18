FROM mcr.microsoft.com/devcontainers/dotnet:8.0

# Install additional tools
RUN apt-get update && apt-get install -y \
    postgresql-client \
    curl \
    wget \
    git \
    && rm -rf /var/lib/apt/lists/*

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y nodejs

# Install Entity Framework Core tools
RUN dotnet tool install --global dotnet-ef

# Make sure dotnet tools are in PATH
ENV PATH="${PATH}:/home/vscode/.dotnet/tools"

# Set the working directory
WORKDIR /workspace