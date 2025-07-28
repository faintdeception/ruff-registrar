# Dockerfile for running E2E tests
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim

# Install Chrome dependencies
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    unzip \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install Chrome
RUN wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list \
    && apt-get update \
    && apt-get install -y google-chrome-stable \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy solution and project files
COPY . .

# Restore dependencies
RUN dotnet restore tests/StudentRegistrar.E2E.Tests/

# Build the test project
RUN dotnet build tests/StudentRegistrar.E2E.Tests/ --no-restore

# Set environment variables for headless mode
ENV SeleniumSettings__Headless=true
ENV SeleniumSettings__BaseUrl=http://host.docker.internal:3001

# Run tests
ENTRYPOINT ["dotnet", "test", "tests/StudentRegistrar.E2E.Tests/", "--no-build", "--logger", "console;verbosity=normal"]
