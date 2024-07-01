# Use the official ASP.NET Core runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["hoistmt.csproj", "./"]
RUN dotnet restore "./hoistmt.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "hoistmt.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "hoistmt.csproj" -c Release -o /app/publish

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ensure wwwroot directory exists


# Set environment variable to distinguish environments
ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ENVIRONMENT

ENTRYPOINT ["dotnet", "hoistmt.dll"]
