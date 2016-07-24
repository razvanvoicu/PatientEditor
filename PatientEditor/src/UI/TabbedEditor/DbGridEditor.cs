using MindLinc.EventBus;
using NLog;
using System;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace MindLinc.UI.TabbedEditor
{
    class DbGridEditor : GridEditor, IObservable<PatientChange>, IObserver<DbTableClear>, IObserver<PatientDeactivate>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public DbGridEditor() : base(readOnly: false)
        {
            setupSubscriptions();
            setupObservables();
        }

        private void setupSubscriptions()
        {
            GlobalEventBrokers.SqlPatientBroker.Subscribe(this);
            GlobalEventBrokers.PatientDeactivateBroker.Subscribe(this);
            GlobalEventBrokers.DbTableClearBroker.Subscribe(this);
            GlobalEventBrokers.PatientChangeBroker.RegisterAsPublisher(this);
        }

        private void setupObservables()
        {
            setupCellValueChangedObservable();
            setupCurrentCellChangedObservable();
        }

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

        private void setupCurrentCellChangedObservable()
        {
            CurrentCellChanged += new EventHandler((object s, EventArgs e) =>
            {
                if (currentCellHasProperValue())
                    _focusedCellValue = Rows[CurrentCellAddress.Y].Cells[CurrentCellAddress.X].Value;
            });
        }

        private bool currentCellHasProperValue()
        {
            return CurrentCellAddress.Y >= 0
                && CurrentCellAddress.Y < Rows.Count
                && CurrentCellAddress.X < Rows[CurrentCellAddress.Y].Cells.Count;
        }

        private object _focusedCellValue = null;

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

        private bool cellValueAsBoolean(DataGridViewCell cell)
        {
            return cell.Value.ToString() == "True";
        }

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

        public void OnNext(DbTableClear value)
        {
            Rows.Clear();
        }

        private ISubject<PatientChange> _innerPatientChangeSubject = new Subject<PatientChange>();
        public IDisposable Subscribe(IObserver<PatientChange> observer)
        {
            return _innerPatientChangeSubject.Subscribe(observer);
        }
    }
}
