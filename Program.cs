using KafkaObserverService.Configuration;
using KafkaObserverService.Data;
using KafkaObserverService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Конфигурация для Docker
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Сервисы
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton<EventObservable>();
builder.Services.AddSingleton<EventProcessingObserver>();
builder.Services.AddSingleton<IDataStorage, PostgresDataStorage>();
builder.Services.AddHostedService<KafkaConsumerService>();

// Логирование
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var host = builder.Build();

// Инициализация
using (var scope = host.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var maxRetries = 5;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var eventObservable = scope.ServiceProvider.GetRequiredService<EventObservable>();
            var eventObserver = scope.ServiceProvider.GetRequiredService<EventProcessingObserver>();

            eventObservable.Subscribe(eventObserver);

            logger.LogInformation("Application services initialized successfully");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to initialize services (attempt {Attempt}/{MaxRetries})", i + 1, maxRetries);

            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "All initialization attempts failed");
                throw;
            }

            await Task.Delay(5000); // Ждем 5 секунд перед повторной попыткой запуска докера
        }
    }
}

await host.RunAsync();