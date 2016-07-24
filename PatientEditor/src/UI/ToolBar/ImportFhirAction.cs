using MindLinc.EventBus;
using NLog;
using System;
using System.Reactive.Subjects;

namespace MindLinc.UI.ToolBar
{
    class ImportFhirAction : IObserver<EventArgs>, IObservable<ImportFhirRequest>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static ImportFhirAction _singleton = null;

        public static ImportFhirAction MakeImportFhirAction()
        {
            if (_singleton == null) _singleton = new ImportFhirAction();
            return _singleton;
        }

        private ImportFhirAction()
        {
            GlobalEventBrokers.FhirImportRequestBroker.RegisterAsPublisher(this);
        }

        public void OnNext(EventArgs value)
        {
            logger.Info("Import requested");
            _innerImportFhirRequestSubject.OnNext(new ImportFhirRequest());
        }

        private ISubject<ImportFhirRequest> _innerImportFhirRequestSubject = new Subject<ImportFhirRequest>();
        public IDisposable Subscribe(IObserver<ImportFhirRequest> observer)
        {
            return _innerImportFhirRequestSubject.Subscribe(observer);
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }
}
