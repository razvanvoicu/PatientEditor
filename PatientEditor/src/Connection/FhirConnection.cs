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

    // Fetches patients from a FHIR REST service
    public class FhirConnection : IObservable<Patient>, IObservable<string>, IObserver<FinderUpdated>
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();
        
        // TODO: explore how to make this class testable without making properties below public

        // cache for fetched data to speed up searches
        public IDictionary<string, Patient> cache = new Dictionary<string, Patient>(); 

        // current filter, will be updated by FinderChanged events
        public Patient filter = new Patient();

        // filter out FinderUpdated events that are not destined for FhirConnection
        public IObservable<FinderUpdated> finderUpdated =
            GlobalEventBrokers.FinderUpdatedBroker.Where(f => f.ContainerTitle == "Filter FHIR Records");

        public FhirConnection(bool startFetchingData)
        {
            GlobalEventBrokers.FhirPatientBroker.RegisterAsPublisher(this);  // Register with the message bus to generate new Patient and Status events
            GlobalEventBrokers.StatusMessageBroker.RegisterAsPublisher(this);
            finderUpdated.Subscribe(this); // Subscribe to receive events when the corresponding filter has its fields updated by user input
            if (startFetchingData) FhirFetchData(); // In tests, we want to control when data is being fetched
        }

        public void FhirFetchData() // TODO: explore how to make testable without making it public
        {
            fetchPatients(patientUrls()); // fetch FHIR patient data asynchronously; this will run in the background for a while.
        }

        public Patient fetchPatient(FhirClient client, string url) // TODO: explore how to make testable without making it public
        {
            var id = ResourceIdentity.Build("Patient", url);
            var patient = new Patient(client.Read<FhirModel.Patient>(id));
            cache[patient.id.Trim()] = patient;
            return patient;
        }

        // Get a list the list of all patient urls from FHIR service.
        public string[] patientUrls() // TODO: explore how to make testable without making it public
        {
            var restClient = new RestClient(ConfigurationManager.AppSettings["allFhirPatientsUrl"]);
            var resp = restClient.Execute(new RestRequest("", Method.POST));
            dynamic patientsJson = JsonConvert.DeserializeObject(resp.Content);
            return ((JArray)patientsJson.entry).Select((dynamic e) => ((string)e.resource.id.Value)).ToArray();
        }

        // Given a set of patient urls, fetch patients from FHIR REST service, and send them as events to be displayed by the grid (if they pass the current filter)
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

        // When receiving a FinderUpdated event (i.e. the filter has been updated by user input)
        // send request to clear the grid, then send the set of Patients passing the filter, one by one.
        public void OnNext(FinderUpdated value) // TODO: explore how to make testable without making it public
        {
            filter = value.Patient;
            GlobalEventBrokers.FhirTableClearBroker.OnNext(new FhirTableClear());
            foreach (string id in cache.Keys)
                if (filterPass(cache[id], filter)) _innerPatientSubject.OnNext(cache[id]);
        }

        // Message bus boilerplate. Route messages to their intended topics
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

        public void OnError(Exception error) { }

        public void OnCompleted() { }
    }
}
