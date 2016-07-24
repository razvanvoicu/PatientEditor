using NLog;
using System.Windows.Forms;

namespace MindLinc.UI.TabbedEditor
{
    class EditorContainer: TabControl
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();

        private const int GRID_WITH_PERCENTAGE = 80;
        private const int FINDER_WIDTH_ABSOLUTE = 300;
        public EditorContainer()
        {
            Dock = DockStyle.Fill;
            Controls.Add(
                mkTabPage("Patients", 
                    mkLayout(new DbGridEditor(), 
                             new FinderForm("Filter DB Records", hasSubmitButton: false))));
            Controls.Add(
                mkTabPage("FHIR", 
                    mkLayout(new FhirGridEditor(), 
                             new FinderForm("Filter FHIR Records", hasSubmitButton: false))));
        }

        private ColumnStyle mkGridColumnStyle()
        {
            var gridColumnStyle = new ColumnStyle();
            gridColumnStyle.SizeType = SizeType.Percent;
            gridColumnStyle.Width = GRID_WITH_PERCENTAGE;
            return gridColumnStyle;
        }

        private ColumnStyle mkFinderColumnStyle()
        {
            var finderColumnStyle = new ColumnStyle();
            finderColumnStyle.SizeType = SizeType.Absolute;
            finderColumnStyle.Width = FINDER_WIDTH_ABSOLUTE;
            return finderColumnStyle;
        }

        private TableLayoutPanel mkLayout(DataGridView grid, FinderForm finderForm)
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(mkGridColumnStyle());
            layout.ColumnStyles.Add(mkFinderColumnStyle());
            layout.Controls.Add(grid);
            layout.Controls.Add(finderForm);

            return layout;
        }

        private TabPage mkTabPage(string title, TableLayoutPanel layout)
        {
            TabPage tabPage = new TabPage();
            tabPage.TabIndex = 0;
            tabPage.Text = title;
            tabPage.Controls.Add(layout);
            return tabPage;
        }
    }
}
