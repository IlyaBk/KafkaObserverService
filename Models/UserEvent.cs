namespace KafkaObserverService.Models
{
    public class UserEvent
    {
        public int UserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public class UserEventStats
    {
        public int UserId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
