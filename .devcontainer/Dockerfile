FROM mcr.microsoft.com/devcontainers/dotnet:8.0

# Install additional tools
RUN apt-get update && apt-get install -y \
    postgresql-client \
    curl \
    wget \
    git \
    && rm -rf /var/lib/apt/lists/*

# Install Entity Framework Core tools
RUN dotnet tool install --global dotnet-ef

# Install Claude Code (native single-binary installer, user-scoped)
USER vscode
RUN curl -fsSL https://claude.ai/install.sh | bash
USER root

# Make sure dotnet tools and Claude Code are in PATH
ENV PATH="${PATH}:/home/vscode/.dotnet/tools:/home/vscode/.local/bin"

# Set the working directory
WORKDIR /workspace
