using KafkaObserverService.Models;

namespace KafkaObserverService.Data
{
    public interface IDataStorage
    {
        Task SaveStatisticsAsync(List<UserEventStats> statistics);
    }
}
