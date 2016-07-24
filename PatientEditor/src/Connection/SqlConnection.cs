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
        public PatientDbContext Db;
        public IDictionary<string,Patient> cache = new Dictionary<string,Patient>();
        private Patient filter = new Patient();
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

        public void OnNext(FinderUpdated value)
        {
            filter = value.Patient;
            GlobalEventBrokers.DbTableClearBroker.OnNext(new DbTableClear());
            foreach (string id in cache.Keys)
                if (filterPass(cache[id], filter))
                    _innerPatientSubject.OnNext(cache[id]);
        }

        public void OnNext(CheckUnique request)
        {
            var response = new IsUnique();
            response.Id = request.Id.Trim();
            response.Unique = !cache.Keys.Contains(response.Id);

            _innerIsUniqueSubject.OnNext(response);
        }

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

