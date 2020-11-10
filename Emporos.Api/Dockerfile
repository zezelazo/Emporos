#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Emporos.Api/Emporos.Api.csproj", "Emporos.Api/"]
COPY ["Emporos.Core/Emporos.Core.csproj", "Emporos.Core/"]
COPY ["Emporos.Data/Emporos.Data.csproj", "Emporos.Data/"]
COPY ["Emporos.Model/Emporos.Model.csproj", "Emporos.Model/"]
COPY ["Emporos.Services/Emporos.Services.csproj", "Emporos.Services/"]
RUN dotnet restore "Emporos.Api/Emporos.Api.csproj"
COPY . .
WORKDIR "/src/Emporos.Api"
RUN dotnet build "Emporos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Emporos.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Emporos.Api.dll"]