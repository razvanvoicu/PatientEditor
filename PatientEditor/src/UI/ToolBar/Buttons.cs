using NLog;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace MindLinc.UI.ToolBar
{
    using static ButtonText;

    // ToolBar buttons, and their connection to corresponding actions.
    class Buttons: ToolStrip
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();

        private ObservableButton[] _buttons = null;
        public Buttons(ComponentResourceManager resources)
        {
            ToolStripItem[] buttons = ButtonNames.Select(text => mkButtonOrSeparator(resources, text)).ToArray();
            Items.AddRange(buttons);
            _buttons = buttons.Where(b => null != b as ObservableButton).Select(b => (ObservableButton)b).ToArray();
            addButtonActions();
        }

        private ToolStripItem mkButtonOrSeparator(ComponentResourceManager resources, string text)
        {
            ToolStripItem button = null;
            if (text == SEPARATOR)
                button = new ToolStripSeparator();
            else
                button = mkButton(resources, text);
            return button;
        }

        private ObservableButton mkButton(ComponentResourceManager resources, string text)
        {
            var button = new ObservableButton();
            button.Image = (Image)resources.GetObject(text);
            button.Text = text;
            return button;
        }

        private void addButtonActions()
        {
            getButtonByText("New Patient").Subscribe(NewPatientAction.MakeNewPatientAction());
            getButtonByText("Deactivate").Subscribe(DeletePatientAction.MakeDeletePatientAction());
            getButtonByText("Import FHIR").Subscribe(ImportFhirAction.MakeImportFhirAction());
        }

        public ObservableButton getButtonByText(string text)
        {
            var result = _buttons.Where(b => b.Text == text).ToList();
            result.Add(null); //Protect against "index out of bounds"
            return result[0];
        }
    }
}
