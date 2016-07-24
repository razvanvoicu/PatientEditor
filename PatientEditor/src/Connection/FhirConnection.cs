using Hl7.Fhir.Rest;
using MindLinc.EventBus;
using MindLinc.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FhirModel = Hl7.Fhir.Model;

namespace MindLinc.Connection
{
    using static Commons;
    public class FhirConnection : IObservable<Patient>, IObservable<string>, IObserver<FinderUpdated>
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();
        public IDictionary<string, Patient> cache = new Dictionary<string, Patient>();
        public Patient filter = new Patient();
        public IObservable<FinderUpdated> finderUpdated =
            GlobalEventBrokers.FinderUpdatedBroker.Where(f => f.ContainerTitle == "Filter FHIR Records");

        public FhirConnection(bool startFetchingData)
        {
            GlobalEventBrokers.FhirPatientBroker.RegisterAsPublisher(this);
            GlobalEventBrokers.StatusMessageBroker.RegisterAsPublisher(this);
            finderUpdated.Subscribe(this);
            if (startFetchingData) FhirFetchData();
        }

        public void FhirFetchData()
        {
            fetchPatients(patientUrls());
        }

        static ISubject<Patient> _innerPatientSubject = new Subject<Patient>();
        public IDisposable Subscribe(IObserver<Patient> subscriber)
        {
            return _innerPatientSubject.Subscribe(subscriber);
        }

        static ISubject<string> _innerStatusSubject = new Subject<string>();
        public IDisposable Subscribe(IObserver<string> observer)
        {
            return _innerStatusSubject.Subscribe(observer);
        }

        public Patient fetchPatient(FhirClient client, string url)
        {
            var id = ResourceIdentity.Build("Patient", url);
            var patient = new Patient(client.Read<FhirModel.Patient>(id));
            cache[patient.id.Trim()] = patient;
            return patient;
        }

        public string[] patientUrls()
        {
            var restClient = new RestClient(ConfigurationManager.AppSettings["allFhirPatientsUrl"]);
            var resp = restClient.Execute(new RestRequest("", Method.POST));
            dynamic patientsJson = JsonConvert.DeserializeObject(resp.Content);
            return ((JArray)patientsJson.entry).Select((dynamic e) => ((string)e.resource.id.Value)).ToArray();
        }

        private void fetchPatients(string[] urls)
        {
            var fhirUrl = ConfigurationManager.AppSettings["fhirUrl"];
            var statusMessage = String.Format("Caching patient data from [{0}]", fhirUrl);
            logger.Info(statusMessage);
            _innerStatusSubject.OnNext(statusMessage);
            var client = new FhirClient(fhirUrl);
            foreach (string url in urls)
            {
                var patient = fetchPatient(client, url);
                if (filterPass(patient, filter)) _innerPatientSubject.OnNext(patient);
            }
        }

        public void OnNext(FinderUpdated value)
        {
            filter = value.Patient;
            GlobalEventBrokers.FhirTableClearBroker.OnNext(new FhirTableClear());
            foreach (string id in cache.Keys)
                if (filterPass(cache[id], filter)) _innerPatientSubject.OnNext(cache[id]);
        }

        public void OnError(Exception error) { }

        public void OnCompleted() { }
    }
}
