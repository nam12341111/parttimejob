FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["PTJ.API/PTJ.API.csproj", "PTJ.API/"]
COPY ["PTJ.Application/PTJ.Application.csproj", "PTJ.Application/"]
COPY ["PTJ.Infrastructure/PTJ.Infrastructure.csproj", "PTJ.Infrastructure/"]
COPY ["PTJ.Domain/PTJ.Domain.csproj", "PTJ.Domain/"]

RUN dotnet restore "PTJ.API/PTJ.API.csproj"
COPY . .
RUN dotnet publish "PTJ.API/PTJ.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "PTJ.API.dll"]
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the solution file and restore dependencies
COPY *.sln .
COPY PTJ.API/*.csproj ./PTJ.API/
COPY PTJ.Application/*.csproj ./PTJ.Application/
COPY PTJ.Domain/*.csproj ./PTJ.Domain/
COPY PTJ.Infrastructure/*.csproj ./PTJ.Infrastructure/
RUN dotnet restore PTJ.API/PTJ.API.csproj

# Copy the remaining source code
COPY . .

# Build the application
WORKDIR /app/PTJ.API
RUN dotnet publish -c Release -o /app/out

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set entry point
ENTRYPOINT ["dotnet", "PTJ.API.dll"]
