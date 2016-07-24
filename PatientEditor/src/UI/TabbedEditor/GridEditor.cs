using MindLinc.EventBus;
using MindLinc.Model;
using NLog;
using System;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace MindLinc.UI.TabbedEditor
{
    // Base class for DbEditor and FhirEditor; populates the columns, and handles 'Patient' events
    // for which the two editors have exactly the same reaction (adding the patient to the grid).
    class GridEditor: DataGridView, IObserver<Patient>, IObservable<string>
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();

        public GridEditor(bool readOnly)
        {
            GlobalEventBrokers.StatusMessageBroker.RegisterAsPublisher(this);
            AllowUserToAddRows = false;
            DataGridViewColumn[] columns = setupColumns(readOnly);
            Columns.AddRange(columns);

            Dock = DockStyle.Fill;
            BackgroundColor = Color.White;
            setupRowHeaders();
        }

        private DataGridViewColumn[] setupColumns(bool readOnly)
        {
            return ColumnNames.AsArray.Select(columName =>
            {
                var column = new DataGridViewTextBoxColumn();
                column.HeaderText = columName;
                if (columName == "Id" || columName == "Active")
                    column.ReadOnly = true;
                else
                    column.ReadOnly = readOnly;
                return (DataGridViewColumn)column;
            }).ToArray();
        }

        private void setupRowHeaders()
        {
            RowHeadersWidth = 50;
            RowPostPaint += new DataGridViewRowPostPaintEventHandler((s, e) => {
                using (SolidBrush b = new SolidBrush(RowHeadersDefaultCellStyle.ForeColor))
                {
                    var header = (e.RowIndex + 1).ToString("0000");
                    var x = e.RowBounds.Location.X + 15;
                    var y = e.RowBounds.Location.Y + 4;
                    e.Graphics.DrawString(header, e.InheritedRowStyle.Font, b, x, y);
                }
            });
        }

        // React to a Patient event (sent by either SqlConnection, or FhirConnection) by adding the patient to the grid.
        public void OnNext(Patient patient)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    Rows.Add(patient.ToDataGridViewRow());
                }));
            }
            else
                Rows.Add(patient.ToDataGridViewRow());
        }

        // Event bus boilerplate
        protected ISubject<string> _innerStatusSubject = new Subject<string>();
        public IDisposable Subscribe(IObserver<string> observer)
        {
            return _innerStatusSubject.Subscribe(observer);
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }
}
