FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем проект и восстанавливаем зависимости
COPY ["KafkaObserverService.csproj", "."]
RUN dotnet restore "KafkaObserverService.csproj"

# Копируем все файлы и собираем приложение
COPY . .
WORKDIR /src
RUN dotnet build "KafkaObserverService.csproj" -c Release -o /app/build
RUN dotnet publish "KafkaObserverService.csproj" -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

ENTRYPOINT ["dotnet", "KafkaObserverService.dll"]