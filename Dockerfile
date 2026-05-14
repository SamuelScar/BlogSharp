FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY ./src/BlogSharp.Api/BlogSharp.Api.csproj ./

RUN dotnet restore

EXPOSE 8080