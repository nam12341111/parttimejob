FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["PTJ.API/PTJ.API.csproj", "PTJ.API/"]
COPY ["PTJ.Application/PTJ.Application.csproj", "PTJ.Application/"]
COPY ["PTJ.Infrastructure/PTJ.Infrastructure.csproj", "PTJ.Infrastructure/"]
COPY ["PTJ.Domain/PTJ.Domain.csproj", "PTJ.Domain/"]

RUN dotnet restore "PTJ.API/PTJ.API.csproj"
COPY . .
RUN dotnet publish "PTJ.API/PTJ.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5000
ENTRYPOINT ["dotnet", "PTJ.API.dll"]
