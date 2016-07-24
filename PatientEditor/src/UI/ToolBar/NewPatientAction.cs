using MindLinc.EventBus;
using MindLinc.UI.TabbedEditor;
using NLog;
using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace MindLinc.UI.ToolBar
{
    class NewPatientAction : IObserver<EventArgs>, IObserver<FinderUpdated>, IObservable<CheckUnique>, IObserver<IsUnique>,
        IObservable<CreatePatient>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        IObservable<FinderUpdated> formFills = GlobalEventBrokers.FinderUpdatedBroker.Where(f => f.ContainerTitle == "Add New Patient");

        private static NewPatientAction _singletonInstance = null;
        public static NewPatientAction MakeNewPatientAction()
        {
            if (_singletonInstance == null) _singletonInstance = new NewPatientAction();
            return _singletonInstance;
        }

        private NewPatientAction()
        {
            formFills.Subscribe(this);
            GlobalEventBrokers.CheckUniqueBroker.RegisterAsPublisher(this);
            GlobalEventBrokers.IsUniqueBroker.Subscribe(this);
            GlobalEventBrokers.CreatePatientBroker.RegisterAsPublisher(this);
        }

        private Form _form;

        public void OnNext(EventArgs value)
        {
            _form = new Form();
            _form.Height = 620;
            _form.Width = 400;
            var finder = new FinderForm("Add New Patient", true);
            finder.Padding = new Padding(10);
            finder.BackColor = Color.Beige;
            _form.Controls.Add(finder);
            _form.ShowDialog();
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }

        private FinderUpdated submittedForm;
        public void OnNext(FinderUpdated form)
        {
            if (!form.Submit) return;
            submittedForm = form;
            var checkUniqueRequest = new CheckUnique();
            checkUniqueRequest.Id = form.Patient.id;
            _innerCheckUniqueSubject.OnNext(checkUniqueRequest);
        }

        private ISubject<CheckUnique> _innerCheckUniqueSubject = new Subject<CheckUnique>();
        public IDisposable Subscribe(IObserver<CheckUnique> observer)
        {
            return _innerCheckUniqueSubject.Subscribe(observer);
        }

        public void OnNext(IsUnique response)
        {
            if (response.Unique)
                sendCreatePatientEvent();
            else
                MessageBox.Show("The new patient's Id already exists in the database.",
                    "Error while creating new patient", MessageBoxButtons.OK);
        }

        private void sendCreatePatientEvent()
        {
            var createPatient = new CreatePatient();
            createPatient.Patient = submittedForm.Patient;
            _innerCreatePatientSubject.OnNext(createPatient);
            try { _form.Close(); } catch { }
        }

        private ISubject<CreatePatient> _innerCreatePatientSubject = new Subject<CreatePatient>();
        public IDisposable Subscribe(IObserver<CreatePatient> observer)
        {
            return _innerCreatePatientSubject.Subscribe(observer);
        }
    }
}
