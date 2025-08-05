# استخدم صورة .NET 8 الرسمية
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# انسخ كل ملفات المشروع
COPY . .

# استرجع الحزم وابني المشروع
RUN dotnet restore "Mistria.API/Mistria.API.csproj"
RUN dotnet publish "Mistria.API/Mistria.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Mistria.API.dll"]
