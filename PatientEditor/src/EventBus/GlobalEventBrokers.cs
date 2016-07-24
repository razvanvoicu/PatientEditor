using MindLinc.Model;

namespace MindLinc.EventBus
{
    public static class GlobalEventBrokers
    {
        public static EventBroker<Patient> FhirPatientBroker = new EventBroker<Patient>();
        public static EventBroker<Patient> SqlPatientBroker = new EventBroker<Patient>();
        public static EventBroker<string> StatusMessageBroker = new EventBroker<string>();
        public static EventBroker<PatientChange> PatientChangeBroker = new EventBroker<PatientChange>();
        public static EventBroker<PatientDeactivate> PatientDeactivateBroker = new EventBroker<PatientDeactivate>();
        public static EventBroker<FinderUpdated> FinderUpdatedBroker = new EventBroker<FinderUpdated>();
        public static EventBroker<DbTableClear> DbTableClearBroker = new EventBroker<DbTableClear>();
        public static EventBroker<FhirTableClear> FhirTableClearBroker = new EventBroker<FhirTableClear>();
        public static EventBroker<CheckUnique> CheckUniqueBroker = new EventBroker<CheckUnique>();
        public static EventBroker<IsUnique> IsUniqueBroker = new EventBroker<IsUnique>();
        public static EventBroker<CreatePatient> CreatePatientBroker = new EventBroker<CreatePatient>();
        public static EventBroker<ImportFhirRequest> FhirImportRequestBroker = new EventBroker<ImportFhirRequest>();
    }
}
