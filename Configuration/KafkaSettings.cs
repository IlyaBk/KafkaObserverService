namespace KafkaObserverService.Configuration
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string Topic { get; set; } = "user-events";
        public string GroupId { get; set; } = "user-events-group";
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
