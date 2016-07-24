using MindLinc.UI.ToolBar;
using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace MindLinc.UI.Menu
{
    // The Menu bar at the top of our app
    class MenuBar: MenuStrip
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // Menu structure expressed as a tree
        static private ObservableMenuItem[] _menuStructure = new ObservableMenuItem[]
        {
            new ObservableMenuItem(MenuText.FILE, Keys.None, new ObservableMenuItem[]
            {
                new ObservableMenuItem(MenuText.IMPORT_FHIR, Keys.Control | Keys.I),
                new ObservableMenuItem(MenuText.EXIT, Keys.Control | Keys.Q)
            }),
            new ObservableMenuItem(MenuText.EDIT, Keys.None, new ObservableMenuItem[]
            {
                new ObservableMenuItem(MenuText.NEW_PATIENT, Keys.Control | Keys.N),
                new ObservableMenuItem(MenuText.DEACTIVATE_PATIENT, Keys.Delete),
            }),
            new ObservableMenuItem(MenuText.HELP, Keys.None, new ObservableMenuItem[]
            {
                new ObservableMenuItem(MenuText.SHORT_USER_MANUAL, Keys.F1),
                new ObservableMenuItem(MenuText.ABOUT, Keys.Control | Keys.Alt | Keys.F1)
            })
        };

        public MenuBar()
        {
            Items.AddRange(_menuStructure);
            setupMenuActions();
        }

        // Create the necessary subscriptions to menu actions
        private void setupMenuActions()
        {
            getObservableByText(MenuText.NEW_PATIENT).Subscribe(NewPatientAction.MakeNewPatientAction());
            getObservableByText(MenuText.DEACTIVATE_PATIENT).Subscribe(DeletePatientAction.MakeDeletePatientAction());
            getObservableByText(MenuText.IMPORT_FHIR).Subscribe(ImportFhirAction.MakeImportFhirAction());
            getObservableByText(MenuText.EXIT).Subscribe(new ExitObserver());
        }

        // Convenient way of accessing the menus inside the structure
        public ObservableMenuItem getObservableByText(string text)
        {
            var menuItems = _menuStructure.SelectMany<ObservableMenuItem, ObservableMenuItem>(i => i.getObservablesByText(text)).Where(o => o.Text == text).ToList();
            if (menuItems.Count != 1) logger.Error("Menu item text selector returns {0} items instead of 1", menuItems.Count);
            menuItems.Add(null); // Protect against index out of bound exception
            return menuItems[0];
        }
    }

    // The simplest menu action: Exit
    class ExitObserver : IObserver<EventArgs>
    {
        public void OnNext(EventArgs value)
        {
            Application.Exit();
        }
        public void OnCompleted() {}

        public void OnError(Exception error) {}
    }
}
