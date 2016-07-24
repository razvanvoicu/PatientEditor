using MindLinc.EventBus;
using MindLinc.Model;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows.Forms;

namespace MindLinc.UI.TabbedEditor
{
    // This is used to collect the filtering informtion from the user. Somewhat peculiarly, we managed to reuse this
    // to create the 'New Patient' form.
    // TODO: perhaps the responsibilities of filtering and patient creation should be segregated.
    class FinderForm : TableLayoutPanel, IObservable<FinderUpdated>, IObserver<Tuple<string, string, string>>,
        IObserver<EventArgs>
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // Styling settings. TODO: move to a separate class, or a config file.
        private const int ROW_HEIGHT = 20;
        private const int TITLE_WIDTH = 280;
        private const string TITLE_FONT_FAMILY = "arial";
        private const int TITLE_FONT_SIZE = 12;
        private const int LABEL_TOP_PADDING = 6;
        private const int TEXTBOX_WIDTH = 180;

        // We handle the text boxes in bulk, using reflection on the patient.
        private ISet<ObservableTextBox> _textBoxes = new HashSet<ObservableTextBox>();

        public string Title { get; set; }
        public FinderForm(string title, bool hasSubmitButton)
        {
            Dock = DockStyle.Fill;
            setRowStyle();
            mkTitle(title);
            setupGridLayout();
            if (hasSubmitButton) setupSubmitButton();
            foreach (var t in _textBoxes) t.Subscribe(this);
            GlobalEventBrokers.FinderUpdatedBroker.RegisterAsPublisher(this);
        }

        private void setupGridLayout()
        {
            foreach (Tuple<string, int> nameAndRow in nameAndRowEnum())
            {
                mkLabel(nameAndRow);
                mkTextBox(nameAndRow);
            }
        }

        IEnumerable<Tuple<string,int>> nameAndRowEnum()
        {
            var usedGridRows = Enumerable.Range(1, ColumnNames.AsArray.Length).Select(i => i * 2);
            return Enumerable.Zip(ColumnNames.AsArray, usedGridRows, (string a, int b) => new Tuple<string, int>(a, b));
        }

        private void setupSubmitButton()
        {
            var b = new Button();
            b.Text = "Submit";
            SetCellPosition(b, new TableLayoutPanelCellPosition(1, 2 * ColumnNames.AsArray.Length + 3));
            Controls.Add(b);
            b.Click += (o, e) =>
            {
                _finderState.ContainerTitle = Title;
                _finderState.Submit = true;
                _innerFinderUpdatedSubject.OnNext(_finderState);
                _finderState.Submit = false;
            };
        }

        private void mkTitle(string titleText)
        {
            Title = titleText;
            var title = new Label();
            title.Text = titleText;
            title.Width = TITLE_WIDTH;
            title.Font = new Font(TITLE_FONT_FAMILY, TITLE_FONT_SIZE);
            Controls.Add(title);
            SetCellPosition(title, new TableLayoutPanelCellPosition(0, 0));
            SetColumnSpan(title, 2);
        }

        private Label mkLabel(Tuple<string, int> nameAndRow)
        {
            var label = new Label();
            label.Text = nameAndRow.Item1;
            label.Padding = new Padding(0, LABEL_TOP_PADDING, 0, 0);
            SetCellPosition(label, new TableLayoutPanelCellPosition(0, nameAndRow.Item2));
            Controls.Add(label);
            return label;
        }

        private ObservableTextBox mkTextBox(Tuple<string, int> nameAndRow)
        {
            var textBox = new ObservableTextBox(Title);
            textBox.Name = nameAndRow.Item1;
            textBox.Width = TEXTBOX_WIDTH;
            SetCellPosition(textBox, new TableLayoutPanelCellPosition(1, nameAndRow.Item2));
            Controls.Add(textBox);
            _textBoxes.Add(textBox);
            return textBox;
        }

        private void setRowStyle()
        {
            ColumnCount = 2;
            RowCount = 2 * ColumnNames.AsArray.Length + 4;
            foreach (int i in Enumerable.Range(0, RowCount))
            {
                var rs = new RowStyle();
                rs.SizeType = SizeType.Absolute;
                rs.Height = ROW_HEIGHT;
                RowStyles.Add(rs);
            }
        }

        public ObservableTextBox getTextBoxByName(string name)
        {
            var searchResult = _textBoxes.Where(tb => tb.Name == name);
            return searchResult.Count() == 0 ? null : searchResult.First();
        }

        private FinderUpdated _finderState = new FinderUpdated(); // The current filter representation of this form.
                                                                  // Each keystroke updates this state by one character,
                                                                  // but then we send the entire object as an event, making it easy
                                                                  // for the grid to appear reactive.
        public void OnNext(Tuple<string, string, string> value)
        {
            _finderState.ContainerTitle = Title;
            var fieldName = value.Item2.ToLower().Replace(" ", "_");
            var fieldNewValue = value.Item3;
            Type patientType = typeof(Patient);
            PropertyInfo prop = patientType.GetProperty(fieldName);
            if (prop.PropertyType == typeof(DateTime?))
                setDateTimeValue(prop, fieldNewValue);
            else if (prop.PropertyType == typeof(bool))
                setBoolValue(prop, fieldNewValue);
            else
                prop.SetValue(_finderState.Patient, fieldNewValue);
            _innerFinderUpdatedSubject.OnNext(_finderState);
        }

        private void setDateTimeValue(PropertyInfo prop, string fieldNewValue)
        {
            DateTime dateResult;
            if (DateTime.TryParse(fieldNewValue, out dateResult))
                prop.SetValue(_finderState.Patient, dateResult);
        }

        private void setBoolValue(PropertyInfo prop, string fieldNewValue)
        {
            bool boolResult;
            if (Boolean.TryParse(fieldNewValue, out boolResult))
                prop.SetValue(_finderState.Patient, boolResult);
        }

        public void OnNext(EventArgs value)
        {
            _finderState.ContainerTitle = Title;
            _finderState.Submit = true;
            _innerFinderUpdatedSubject.OnNext(_finderState);
            _finderState.Submit = false;
            try { (Parent as Form).Close(); } catch { }
        }

        // Event bus boilerplate
        private ISubject<FinderUpdated> _innerFinderUpdatedSubject = new Subject<FinderUpdated>();
        public IDisposable Subscribe(IObserver<FinderUpdated> observer)
        {
            return _innerFinderUpdatedSubject.Subscribe(observer);
        }

        public void OnError(Exception error) { }

        public void OnCompleted() { }
    }
}

