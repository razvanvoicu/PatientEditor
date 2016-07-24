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
    // Action invoked by the 'New Patient' button. Builds a form that takes input from the user, and then requests creation of a new patient (after checking for uniqueness of id).
    // The flow is a bit convoluted here. This object contains a FinderForm (coincidentally, creation and filtering need forms with the same format), but does not have direct 
    // access to the form's content. It can only find that content by subscribing to the FinderUpdate events issued by the form. These events need to be filtered by destination,
    // which results in the 'formFills' observable below. We only want FormFills where the 'Submit' button has been pressed. When that happens, the Action issues a request for
    // uniqueness check to SqlConnection. Upon receiving positive response, it can then issue CreatePatient. This back and forth is necessary because SqlConnection has no UI concernes,
    // and should not have the responsibility, in the case of a non-unique id, of creating a MessageBox to notify the user.
    class NewPatientAction : IObserver<EventArgs>, IObserver<FinderUpdated>, IObservable<CheckUnique>, IObserver<IsUnique>,
        IObservable<CreatePatient>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // Filter FormUpdate stream of events for events only destined for this form.
        IObservable<FinderUpdated> formFills = GlobalEventBrokers.FinderUpdatedBroker.Where(f => f.ContainerTitle == "Add New Patient");

        private static NewPatientAction _singletonInstance = null;
        public static NewPatientAction MakeNewPatientAction()
        {
            if (_singletonInstance == null) _singletonInstance = new NewPatientAction();
            return _singletonInstance;
        }

        // Receives FormUpdated events from its inner FinderForm
        // Issues CheckUnique events to SqlConnection
        // Receives IsUnique events from SqlConnection
        // Issues CreatePatient events to SqlConnection
        private NewPatientAction()
        {
            formFills.Subscribe(this);
            GlobalEventBrokers.CheckUniqueBroker.RegisterAsPublisher(this);
            GlobalEventBrokers.IsUniqueBroker.Subscribe(this);
            GlobalEventBrokers.CreatePatientBroker.RegisterAsPublisher(this);
        }

        private Form _form;

        // Show 'New Patient' dialog
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

        private FinderUpdated submittedForm; // cache the form, so we can build the CreatePatient later

        // Wait for a form with 'Submit' turned on, and then send a uniqueness check request
        public void OnNext(FinderUpdated form)
        {
            if (!form.Submit) return;
            submittedForm = form;
            var checkUniqueRequest = new CheckUnique();
            checkUniqueRequest.Id = form.Patient.id;
            _innerCheckUniqueSubject.OnNext(checkUniqueRequest);
        }

        // Upon receiving the uniqueness response, either send the CreatePatient event, or notify the user of the negative result.
        public void OnNext(IsUnique response)
        {
            if (response.Unique)
                sendCreatePatientEvent();
            else
                MessageBox.Show("The new patient's Id already exists in the database.",
                    "Error while creating new patient", MessageBoxButtons.OK);
        }

        // Build a CreatePatient event and send it to SqlConnection
        private void sendCreatePatientEvent()
        {
            var createPatient = new CreatePatient();
            createPatient.Patient = submittedForm.Patient;
            _innerCreatePatientSubject.OnNext(createPatient);
            try { _form.Close(); } catch { }
        }

        // Event bus boilerplate
        private ISubject<CreatePatient> _innerCreatePatientSubject = new Subject<CreatePatient>();
        public IDisposable Subscribe(IObserver<CreatePatient> observer)
        {
            return _innerCreatePatientSubject.Subscribe(observer);
        }

        private ISubject<CheckUnique> _innerCheckUniqueSubject = new Subject<CheckUnique>();
        public IDisposable Subscribe(IObserver<CheckUnique> observer)
        {
            return _innerCheckUniqueSubject.Subscribe(observer);
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }
}
