using MindLinc.EventBus;
using MindLinc.Model;
using NLog;
using System;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace MindLinc.UI.TabbedEditor
{
    class FhirGridEditor: GridEditor, IObserver<FhirTableClear>, IObserver<ImportFhirRequest>, IObservable<CreatePatient>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public FhirGridEditor(): base(readOnly: true)
        {
            GlobalEventBrokers.FhirPatientBroker.Subscribe(this);
            GlobalEventBrokers.FhirTableClearBroker.Subscribe(this);
            GlobalEventBrokers.FhirImportRequestBroker.Subscribe(this);
            GlobalEventBrokers.CreatePatientBroker.RegisterAsPublisher(this);
        }

        public void OnNext(ImportFhirRequest value)
        {
            if (CurrentCellAddress.Y < 0) { warnRowSelectionNeeded(); return; }
            var cells = Rows[CurrentCellAddress.Y].Cells;
            var patient = new Patient();
            patient.id = cells[0].Value.ToString();
            patient.family_name = cells[1].Value.ToString();
            patient.given_name = cells[2].Value.ToString();
            DateTime date;
            patient.birth_date = DateTime.TryParse(cells[3].Value.ToString(), out date) ? date : (DateTime?)null ;
            patient.gender = cells[4].Value.ToString();
            patient.marital_status = cells[5].Value.ToString();
            patient.address = cells[6].Value.ToString();
            patient.telecom = cells[7].Value.ToString();
            patient.language = cells[8].Value.ToString();
            patient.managing_organization = cells[9].Value.ToString();
            Boolean b;
            patient.active = Boolean.TryParse(cells[10].Value.ToString(), out b) ? b : true;
            var createPatientRequest = new CreatePatient();
            createPatientRequest.Patient = patient;
            _innerCreatePatientSubject.OnNext(createPatientRequest);
            var statusMessage = String.Format("Attempt to import FHIR patient with id [{0}]", patient.id);
            logger.Info(statusMessage);
            _innerStatusSubject.OnNext(statusMessage);
        }

        private void warnRowSelectionNeeded()
        {
            MessageBox.Show(
                "You need to select a row on the FHIR tab in order to import a record", 
                "Error while importing patient", MessageBoxButtons.OK);
        }

        public void OnNext(FhirTableClear tableClearRequest)
        {
            Rows.Clear();
        }

        ISubject<CreatePatient> _innerCreatePatientSubject = new Subject<CreatePatient>();
        public IDisposable Subscribe(IObserver<CreatePatient> observer)
        {
            return _innerCreatePatientSubject.Subscribe(observer);
        }
    }
}
