using KafkaObserverService.Configuration;
using KafkaObserverService.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace KafkaObserverService.Data
{
    public class PostgresDataStorage : IDataStorage
    {
        private readonly string _connectionString;
        private readonly ILogger<PostgresDataStorage> _logger;

        public PostgresDataStorage(IOptions<DatabaseSettings> dbSettings, ILogger<PostgresDataStorage> logger)
        {
            _connectionString = dbSettings.Value.ConnectionString;
            _logger = logger;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                var createTableSql = """
                CREATE TABLE IF NOT EXISTS user_event_stats (
                    user_id INT NOT NULL,
                    event_type VARCHAR(50) NOT NULL,
                    count INT NOT NULL,
                    last_updated TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
                    PRIMARY KEY (user_id, event_type)
                );
                """;

                using var command = new NpgsqlCommand(createTableSql, connection);
                command.ExecuteNonQuery();

                _logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database");
                throw;
            }
        }

        public async Task SaveStatisticsAsync(List<UserEventStats> statistics)
        {
            if (!statistics.Any()) return;

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = """
                INSERT INTO user_event_stats (user_id, event_type, count, last_updated)
                VALUES (@userId, @eventType, @count, NOW())
                ON CONFLICT (user_id, event_type)
                DO UPDATE SET 
                    count = user_event_stats.count + EXCLUDED.count,
                    last_updated = NOW();
                """;

                foreach (var stat in statistics)
                {
                    using var command = new NpgsqlCommand(sql, connection);
                    command.Parameters.AddWithValue("userId", stat.UserId);
                    command.Parameters.AddWithValue("eventType", stat.EventType);
                    command.Parameters.AddWithValue("count", stat.Count);

                    await command.ExecuteNonQueryAsync();
                }

                _logger.LogDebug("Saved {Count} statistics records to PostgreSQL", statistics.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save statistics to PostgreSQL");
                throw;
            }
        }
    }
}
