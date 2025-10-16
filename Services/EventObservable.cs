using KafkaObserverService.Models;

namespace KafkaObserverService.Services
{
    public class EventObservable : IObservable<UserEvent>
    {
        private readonly List<IObserver<UserEvent>> _observers = new();
        private readonly ILogger<EventObservable> _logger;

        public EventObservable(ILogger<EventObservable> logger)
        {
            _logger = logger;
        }

        public IDisposable Subscribe(IObserver<UserEvent> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                _logger.LogInformation("New observer subscribed. Total observers: {Count}", _observers.Count);
            }

            return new Unsubscriber(_observers, observer, _logger);
        }

        public void NotifyObservers(UserEvent userEvent)
        {
            foreach (var observer in _observers.ToArray())
            {
                try
                {
                    observer.OnNext(userEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error notifying observer");
                    observer.OnError(ex);
                }
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly List<IObserver<UserEvent>> _observers;
            private readonly IObserver<UserEvent> _observer;
            private readonly ILogger<EventObservable> _logger;

            public Unsubscriber(List<IObserver<UserEvent>> observers, IObserver<UserEvent> observer, ILogger<EventObservable> logger)
            {
                _observers = observers;
                _observer = observer;
                _logger = logger;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                    _logger.LogInformation("Observer unsubscribed. Remaining observers: {Count}", _observers.Count);
                }
            }
        }
    }
}
