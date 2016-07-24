using System;
using System.Reactive.Subjects;

namespace MindLinc.EventBus
{
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
