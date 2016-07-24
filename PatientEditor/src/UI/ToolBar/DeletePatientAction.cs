using MindLinc.EventBus;
using System;
using System.Reactive.Subjects;

namespace MindLinc.UI.ToolBar
{
    // Action to delete/deactivate a patient, as a reactive element.
    class DeletePatientAction : IObserver<EventArgs>, IObservable<PatientDeactivate>
    {
        private static DeletePatientAction _singleton = null;

        public static DeletePatientAction MakeDeletePatientAction()
        {
            if (_singleton == null) _singleton = new DeletePatientAction();
            return _singleton;
        }
        private DeletePatientAction()
        {
            GlobalEventBrokers.PatientDeactivateBroker.RegisterAsPublisher(this);
        }
        public void OnNext(EventArgs value)
        {
            _innerPatientDeactivateSubject.OnNext(new PatientDeactivate());
        }

        ISubject<PatientDeactivate> _innerPatientDeactivateSubject = new Subject<PatientDeactivate>();
        public IDisposable Subscribe(IObserver<PatientDeactivate> observer)
        {
            return _innerPatientDeactivateSubject.Subscribe(observer);
        }
        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }
}
