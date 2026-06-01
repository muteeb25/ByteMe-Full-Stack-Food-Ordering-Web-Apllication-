# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["ByteMe/ByteMe.csproj", "ByteMe/"]
RUN dotnet restore "ByteMe/ByteMe.csproj"

# Copy source code
COPY . .
WORKDIR "/src/ByteMe"
RUN dotnet build "ByteMe.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ByteMe.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "ByteMe.dll"]
