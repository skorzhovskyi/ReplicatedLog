FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 2100

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ReplicatedLogMaster.csproj", ""]
RUN dotnet restore "./ReplicatedLogMaster.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ReplicatedLogMaster.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReplicatedLogMaster.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
