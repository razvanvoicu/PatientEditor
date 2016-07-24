using MindLinc.EventBus;
using NLog;
using System;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace MindLinc.UI.TabbedEditor
{
    // The DB patient editor. Allows filtering and modifying patients. If a modification is invalid, the original value is restored.
    class DbGridEditor : GridEditor, IObservable<PatientChange>, IObserver<DbTableClear>, IObserver<PatientDeactivate>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public DbGridEditor() : base(readOnly: false)
        {
            setupSubscriptions();
            setupObservables();
        }

        // Issues PatientChange when the field of a patient has been edited; consumed by SqlConnection
        // Responds to 'Patient' messages, by displaying the patient; issued by SqlConnection
        // Responds to TableClear event, by clearing all rows in the table; issued by SqlConnection (when its filter was updated)
        // Responds to PatientDeactivate, by turning the active flag to false (this change will automatically trigger PatientChange by the underlying DataGridView).
        // -- PatientDeactivate is issued by the 'Deactivate' button.
        private void setupSubscriptions()
        {
            GlobalEventBrokers.SqlPatientBroker.Subscribe(this);
            GlobalEventBrokers.PatientDeactivateBroker.Subscribe(this);
            GlobalEventBrokers.DbTableClearBroker.Subscribe(this);
            GlobalEventBrokers.PatientChangeBroker.RegisterAsPublisher(this);
        }

        // Turn UI events into Reactive Observables
        private void setupObservables()
        {
            setupCellValueChangedObservable();
            setupCurrentCellChangedObservable();
        }

        // When the user edits a cell, WinForms issues the CellValueChanged event. We turn this event into an observable
        private void setupCellValueChangedObservable()
        {
            CellValueChanged += new DataGridViewCellEventHandler((object s, DataGridViewCellEventArgs e) =>
            {
                var columnName = Columns[e.ColumnIndex].HeaderText;
                var id = Rows[e.RowIndex].Cells[0].Value.ToString();
                var newCellValue = Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                if (fieldContainsDate(columnName, newCellValue))
                    restoreOriginalValue(e, newCellValue);
                else
                    changeCellValue(columnName, id, newCellValue);

            });
        }

        // Date formatting requires special attention
        private bool fieldContainsDate(string columnName, string dateString)
        {
            DateTime _dummyDate = DateTime.Now;
            return columnName.Contains("Date") && !DateTime.TryParse(dateString, out _dummyDate);
        }

        private void restoreOriginalValue(DataGridViewCellEventArgs e, string newCellValue)
        {
            MessageBox.Show("Date format error in string: " + newCellValue, "Error");
            Rows[e.RowIndex].Cells[e.ColumnIndex].Value = _focusedCellValue;
        }

        private void changeCellValue(string columnName, string id, string newCellValue)
        {
            _innerPatientChangeSubject.OnNext(new PatientChange(id, columnName, newCellValue));
        }

        // Restoring the original value (in case a change was invalid) requires extra attention, since the underlying
        // WinForms layer does not cache the previous value of a cell, and the CellValueChanged event is issued only
        // after the content of the cell has already been modified. To alleviate that, we snatch the content of the
        // currently focussed cell when the focus moves. When the content changes, the old content is the snatched content.
        private void setupCurrentCellChangedObservable()
        {
            CurrentCellChanged += new EventHandler((object s, EventArgs e) =>
            {
                if (currentCellHasProperValue())
                    _focusedCellValue = Rows[CurrentCellAddress.Y].Cells[CurrentCellAddress.X].Value;
            });
        }

        // Snatch the content only when there is a proper focus. That is indicated by the coordinates
        // of the current position being non-negative.
        private bool currentCellHasProperValue()
        {
            return CurrentCellAddress.Y >= 0
                && CurrentCellAddress.Y < Rows.Count
                && CurrentCellAddress.X < Rows[CurrentCellAddress.Y].Cells.Count;
        }

        private object _focusedCellValue = null;

        // Respond to the PatientDeactivate event, issued by the 'Deactivate' button.
        public void OnNext(PatientDeactivate value)
        {
            var row = CurrentCellAddress.Y;
            var col = 10;
            var cell = Rows[row].Cells[col];
            var id = Rows[row].Cells[0].Value;
            if (cellValueAsBoolean(cell))
                deactivatePatient(cell, id);
            else
            {
                MessageBox.Show(this,
                    "Patient " + id + " is already deactivated!",
                    "Patient deactivation", MessageBoxButtons.OK);
                var statusMessage = String.Format("Igored attempt to deactivate id [{0}] which is already deactivated", id);
                logger.Warn(statusMessage);
                _innerStatusSubject.OnNext(statusMessage);
            }
        }

        // Handle boolean cell formatting. This is necessary since the content of the cell has type 'object', and a
        // value type is wrapped into a nullable type to be storable in a cell.
        private bool cellValueAsBoolean(DataGridViewCell cell)
        {
            return cell.Value.ToString() == "True";
        }

        // Perform the deactivation by first asking permission from the user
        private void deactivatePatient(DataGridViewCell cell, object id)
        {
            var dialogResult = MessageBox.Show(this, 
                "Are you sure you want to deactivate patient " + id, 
                "Patient deactivation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes) cell.Value = false;
            var statusMessage = String.Format("Deactivated patient id [{0}]", id);
            logger.Info(statusMessage);
            _innerStatusSubject.OnNext(statusMessage);
        }

        // Respond to TableClear request by wiping all the data in the grid.
        public void OnNext(DbTableClear value)
        {
            Rows.Clear();
        }

        // Event bus boilerplate
        private ISubject<PatientChange> _innerPatientChangeSubject = new Subject<PatientChange>();
        public IDisposable Subscribe(IObserver<PatientChange> observer)
        {
            return _innerPatientChangeSubject.Subscribe(observer);
        }
    }
}
