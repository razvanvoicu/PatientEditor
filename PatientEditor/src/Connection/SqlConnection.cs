using MindLinc.EventBus;
using MindLinc.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;

namespace MindLinc.Connection
{
    using static Commons;
    public class SqlConnection : IObservable<Patient>, IObservable<string>, IObserver<PatientChange>, 
        IObserver<FinderUpdated>, IObserver<CheckUnique>, IObservable<IsUnique>, IObserver<CreatePatient>
    {
		static private Logger logger = LogManager.GetCurrentClassLogger();

        // TODO: Explore how to make this class testable without making its fields public

        // Model context for EntityFramework, used to persist/retrieve/search data in the local SQL server instance.
        public PatientDbContext Db;

        // Cache to speed up searches
        public IDictionary<string,Patient> cache = new Dictionary<string,Patient>();

        // Current filter, will be updated by FinderUpdated events (generated when user tweaks the FinderForm fields)
        private Patient filter = new Patient();

        // Filter out FinderUpdated events not destined for SqlConnection
        private IObservable<FinderUpdated> _finderUpdated = 
            GlobalEventBrokers.FinderUpdatedBroker.Where(f => f.ContainerTitle == "Filter DB Records");

        public SqlConnection()
        {
            setupDatabaseConnection();
            setupSubscriptions();
            SqlFetchData();
        }

        private void setupDatabaseConnection()
        {
            try
            {
                Db = new PatientDbContext();
            }
            catch
            {
                var statusMessage = "Database connection failed";
                logger.Error(statusMessage);
                _innerStatusSubject.OnNext(statusMessage);
            }
        }

        // Setup Message bus subscriptions and registrations
        private void setupSubscriptions()
        {
            GlobalEventBrokers.SqlPatientBroker.RegisterAsPublisher(this);
            GlobalEventBrokers.StatusMessageBroker.RegisterAsPublisher(this);
            GlobalEventBrokers.IsUniqueBroker.RegisterAsPublisher(this);
            GlobalEventBrokers.PatientChangeBroker.Subscribe(this);
            GlobalEventBrokers.CheckUniqueBroker.Subscribe(this);
            GlobalEventBrokers.CreatePatientBroker.Subscribe(this);
            _finderUpdated.Subscribe(this);
        }

        // Retrieve data from DB, and send to the grid the patients that pass the current filter
        public void SqlFetchData()
        {
            try
            {
                foreach (var patient in Db.patients)
                {
                    cache[patient.id.Trim()] = patient;
                    if (filterPass(patient, filter)) _innerPatientSubject.OnNext(patient);
                }
            }
            catch
            {
                var statusMessage = "Unable to fetch data from database";
                logger.Error(statusMessage);
                _innerStatusSubject.OnNext(statusMessage);
            }
        }

        // When a patient is edited in the grid, the PatientChange event is emitted, with an encoding of the edit. The edit
        // operation is propagated into the cache and database.
        public void OnNext(PatientChange patientChange)
        {
            try
            {
                var patient = Db.patients.Find(patientChange.Id);
                modifyPatientField(patientChange, patient);
                Db.SaveChanges();
                var statusMessage = String.Format(
                    "Modification succeeded for patient id [{0}] with field [{1}], new value [{2}]",
                    patientChange.Id, patientChange.Field, patientChange.NewValue);
                logger.Info(statusMessage);
                _innerStatusSubject.OnNext(statusMessage);
            }
            catch (Exception e)
            {
                var statusMessage = String.Format(
                    "Modification failed for patient id [{0}] with field [{1}], new value [{2}]\n[{3}]",
                    patientChange.Id, patientChange.Field, patientChange.NewValue, e);
                logger.Error(statusMessage);
                _innerStatusSubject.OnNext(statusMessage);
            }
        }

        // Modify the patient field specified by the PatientChange event. Use reflection
        // to decouple the modification logic from the actual structure of class Patient.
        public void modifyPatientField(PatientChange patientChange, Patient patient)
        {
            Type patientType = typeof(Patient);
            var fieldName = patientChange.Field.ToLower().Replace(" ", "_");
            PropertyInfo prop = patientType.GetProperty(fieldName);
            if (prop.PropertyType == typeof(DateTime?))
                prop.SetValue(patient, DateTime.Parse(patientChange.NewValue));
            else if (prop.PropertyType == typeof(bool))
                prop.SetValue(patient, Boolean.Parse(patientChange.NewValue));
            else
                prop.SetValue(patient, patientChange.NewValue);
        }

        // When the user types in the filtering form, the FinderUpdated event is emitted on each keystroke
        // Here we react to these events by updating the current filter, and requesting a rebuild of the grid
        // by sending 'TableClear' and 'Patient' events.
        public void OnNext(FinderUpdated value)
        {
            filter = value.Patient;
            GlobalEventBrokers.DbTableClearBroker.OnNext(new DbTableClear());
            foreach (string id in cache.Keys)
                if (filterPass(cache[id], filter))
                    _innerPatientSubject.OnNext(cache[id]);
        }

        // When a new patient is added, a check for unique id is requested via a CheckUnique request.
        // Here we check the uniqueness, and respond with a IsUnique event containing the answer.
        public void OnNext(CheckUnique request)
        {
            var response = new IsUnique();
            response.Id = request.Id.Trim();
            response.Unique = !cache.Keys.Contains(response.Id);

            _innerIsUniqueSubject.OnNext(response);
        }

        // When the New Patient form has its 'Submit' button clicked, a 'CreatePatient' event is emitted (after id uniqueness has been checked)
        // Here we react to this event by creating the user, and adding it to the cache and database.
        public void OnNext(CreatePatient createRequest)
        {
            Db.patients.Add(createRequest.Patient);
            try
            {
                Db.SaveChanges();
                cache[createRequest.Patient.id.Trim()] = createRequest.Patient;
                _innerPatientSubject.OnNext(createRequest.Patient);
                var statusMessage = String.Format("Creation of patient with id [{0}] succeeded.", createRequest.Patient.id);
                logger.Info(statusMessage);
                _innerStatusSubject.OnNext(statusMessage);
            }
            catch(DbEntityValidationException e)
            {
                var statusMessage = String.Format("Create patient failed for id [{0}]\n{1}\nEntityValidationErrors:\n",
                    createRequest.Patient.id, e.ToString(), e.EntityValidationErrors);
                logger.Error(statusMessage);
                _innerStatusSubject.OnNext(statusMessage);
                Db.patients.Remove(createRequest.Patient);
            }
        }

        // Message bus boilerplate
        public void OnError(Exception error) { }
        public void OnCompleted() { }

        ISubject<Patient> _innerPatientSubject = new Subject<Patient>();
        public IDisposable Subscribe(IObserver<Patient> observer) { return _innerPatientSubject.Subscribe(observer); }

        ISubject<string> _innerStatusSubject = new Subject<string>();
        public IDisposable Subscribe(IObserver<string> observer) { return _innerStatusSubject.Subscribe(observer); }

        private ISubject<IsUnique> _innerIsUniqueSubject = new Subject<IsUnique>();
        public IDisposable Subscribe(IObserver<IsUnique> observer) { return _innerIsUniqueSubject.Subscribe(observer); }
    }
}

