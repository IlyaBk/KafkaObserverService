using KafkaObserverService.Data;
using KafkaObserverService.Models;

namespace KafkaObserverService.Services
{
    public class EventProcessingObserver : IObserver<UserEvent>
    {
        private readonly Dictionary<(int userId, string eventType), int> _eventCounts = new();
        private readonly IDataStorage _dataStorage;
        private readonly ILogger<EventProcessingObserver> _logger;
        private readonly Timer _flushTimer;
        private readonly object _lockObject = new();

        public EventProcessingObserver(IDataStorage dataStorage, ILogger<EventProcessingObserver> logger)
        {
            _dataStorage = dataStorage;
            _logger = logger;

            // Сохраняем статистику (сделал по 30 сек, можно вынести в конфиг)
            _flushTimer = new Timer(FlushStatistics, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public void OnNext(UserEvent userEvent)
        {
            try
            {
                var key = (userEvent.UserId, userEvent.EventType);

                lock (_lockObject)
                {
                    if (_eventCounts.ContainsKey(key))
                    {
                        _eventCounts[key]++;
                    }
                    else
                    {
                        _eventCounts[key] = 1;
                    }
                }

                _logger.LogDebug("Processed event - UserId: {UserId}, Type: {EventType}, Current Count: {Count}",
                    userEvent.UserId, userEvent.EventType, _eventCounts[key]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event for user {UserId}", userEvent.UserId);
            }
        }

        public void OnError(Exception error)
        {
            _logger.LogError(error, "Error in event processing observer");
        }

        public void OnCompleted()
        {
            _logger.LogInformation("Event processing observer completed");
            FlushStatistics(null); // Сохраняем оставшиеся данные
            _flushTimer?.Dispose();
        }

        private void FlushStatistics(object? state)
        {
            try
            {
                List<UserEventStats> statsToSave;

                lock (_lockObject)
                {
                    if (!_eventCounts.Any())
                    {
                        _logger.LogDebug("No statistics to flush");
                        return;
                    }

                    statsToSave = _eventCounts.Select(kvp => new UserEventStats
                    {
                        UserId = kvp.Key.userId,
                        EventType = kvp.Key.eventType,
                        Count = kvp.Value
                    }).ToList();

                    _eventCounts.Clear();
                }

                _dataStorage.SaveStatisticsAsync(statsToSave).Wait();
                _logger.LogInformation("Flushed {Count} statistics records to storage", statsToSave.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing statistics to storage");
            }
        }
    }
}
