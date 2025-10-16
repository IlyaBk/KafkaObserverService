# KafkaObserverService
Микросервис для обработки событий пользователей из Apache Kafka с использованием паттерна Observer и сохранением статистики в PostgreSQL.

## Архитектура
### Сервис реализует:
* Consumer Apache Kafka - подписка на топик user-events
* Паттерн Observer - обработка событий через IObservable/IObserver
* PostgreSQL - сохранение статистики
* Docker - запуск приложения и окружения для него в контейнере

## Запуск приложения / окружений
1. Клонируйте репозиторий:
   
```bash
git clone <repository-url>
cd KafkaObserverService
```

2. Запустите сервисы:

```bash
docker-compose up -d
```

3. Проверка статусов сервисов:

```bash
docker-compose ps
```

4. Логи приложения:

```bash
docker-compose logs -f kafka-observer-service
```

##  Проверка работы
1. Создадим топик Kafka:

```bash
./scripts/kafka-commands.sh create-topic
```
2. Отправим тестовое событие:

```bash
./scripts/kafka-commands.sh produce-test
```

3. Проверьте статистику в PostgreSQL:

```bash
docker-compose exec postgres psql -U postgres -d user_events_db -c "SELECT * FROM user_event_stats;"
```
