FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем и восстанавливаем зависимости проекта
COPY ["hr_director/hr_director.csproj", "hr_director/"]
RUN dotnet restore "hr_director/hr_director.csproj"

COPY . .
WORKDIR "/src/hr_director"
RUN dotnet build "hr_director.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Установка dotnet-ef для работы с миграциями
RUN dotnet tool install --global dotnet-ef --version 7.0.10
ENV PATH="$PATH:/root/.dotnet/tools"

FROM build AS publish
RUN dotnet publish "hr_director.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Выполняем миграции при запуске контейнера
ENTRYPOINT ["dotnet", "hr_director.dll"]
