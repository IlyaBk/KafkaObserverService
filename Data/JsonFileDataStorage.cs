using KafkaObserverService.Models;
using System.Text.Json;

namespace KafkaObserverService.Data
{
    public class JsonFileDataStorage : IDataStorage
    {
        private readonly string _filePath = "user_event_stats.json";
        private readonly ILogger<JsonFileDataStorage> _logger;
        private readonly object _fileLock = new();

        public JsonFileDataStorage(ILogger<JsonFileDataStorage> logger)
        {
            _logger = logger;
        }

        public Task SaveStatisticsAsync(List<UserEventStats> statistics)
        {
            try
            {
                lock (_fileLock)
                {
                    List<UserEventStats> existingStats = new();

                    if (File.Exists(_filePath))
                    {
                        var existingJson = File.ReadAllText(_filePath);
                        existingStats = JsonSerializer.Deserialize<List<UserEventStats>>(existingJson) ?? new List<UserEventStats>();
                    }

                    // Обновляем существующую статистику
                    foreach (var newStat in statistics)
                    {
                        var existingStat = existingStats.FirstOrDefault(s =>
                            s.UserId == newStat.UserId && s.EventType == newStat.EventType);

                        if (existingStat != null)
                        {
                            existingStat.Count += newStat.Count;
                        }
                        else
                        {
                            existingStats.Add(newStat);
                        }
                    }

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(existingStats, options);
                    File.WriteAllText(_filePath, json);

                    _logger.LogDebug("Saved {Count} statistics records to JSON file", statistics.Count);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save statistics to JSON file");
                throw;
            }
        }
    }
}
