using MindLinc.UI;
using NLog;
using System;
using System.Windows.Forms;

namespace MindLinc
{
    // Boilerplate startup lifted from WinForms examples on the web
    static class Startup
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();
        [STAThread]
        static void Main()
        {
            logger.Info("Application started");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
            logger.Info("Application exited");
        }
    }
}
