using MindLinc.Connection;
using MindLinc.UI.Menu;
using MindLinc.UI.StatusBar;
using MindLinc.UI.TabbedEditor;
using MindLinc.UI.ToolBar;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MindLinc.UI
{
    public class MainWindow : Form
    {
        private MenuBar _menuBar = new MenuBar();
        private TabControl _editor = new EditorContainer();
        private ToolStrip _toolBar = new Buttons(new ComponentResourceManager(typeof(MainWindow)));
        private StatusStrip _statusStrip = new StatusStrip();
        private TableLayoutPanel _layout = new TableLayoutPanel();
        public MainWindow()
        {
            Size = new Size(1600, 800);
            Text = "Patient Editor";
            _statusStrip.Dock = DockStyle.Bottom;
            setLayout();
            var bw = new BackgroundWorker();
            bw.DoWork += (e, a) => new FhirConnection(startFetchingData: true);
            bw.RunWorkerAsync();
            var bw1 = new BackgroundWorker();
            bw1.DoWork += (e, a) => new SqlConnection();
            bw1.RunWorkerAsync();
        }

        private void setLayout()
        {
            Controls.Add(_layout);
            _layout.Dock = DockStyle.Fill;
            _layout.Controls.AddRange(new Control[] { _menuBar, _toolBar, _editor});
            Controls.Add(_statusStrip);
            _statusStrip.Items.Add(new StatusArea());
        }
    }
}
