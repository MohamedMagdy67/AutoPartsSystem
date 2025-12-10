# ===== 1️⃣ Base image =====
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

# ===== 2️⃣ Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AutoPartsSystem.csproj", "./"]
RUN dotnet restore "./AutoPartsSystem.csproj"
COPY . .
RUN dotnet build "AutoPartsSystem.csproj" -c Release -o /app/build

# ===== 3️⃣ Publish stage =====
FROM build AS publish
RUN dotnet publish "AutoPartsSystem.csproj" -c Release -o /app/publish

# ===== 4️⃣ Final stage =====
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AutoPartsSystem.dll"]
