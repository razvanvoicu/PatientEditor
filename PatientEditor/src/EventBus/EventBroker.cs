using System;
using System.Reactive.Subjects;

namespace MindLinc.EventBus
{
    // Message bus simulator. We simply use a 'Subject' to connect publishers with subscribers.
    // We use Reactive Extension rather than a specialized message bus API, since using Rx was a requirement.
    public class EventBroker<T>: ISubject<T>, IObserver<T>, IObservable<T>
    {
        Subject<T> _innerSubject = new Subject<T>();

        public void OnNext(T value)
        {
            _innerSubject.OnNext(value);
        }

        public void OnError(Exception error)
        {
            _innerSubject.OnError(error);
        }

        public void OnCompleted()
        {
            _innerSubject.OnCompleted();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _innerSubject.Subscribe(observer);
        }

        public IDisposable RegisterAsPublisher(IObservable<T> publisher)
        {
            return publisher.Subscribe(_innerSubject);
        }
    }
}
