using MindLinc.EventBus;
using System;
using System.Configuration;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MindLinc.UI.StatusBar
{
    // Status bar at the bottom of the main window, used to display each log message for about 30s
    // This element is plugged into the event bus, via the StatusMessageBroker
    class StatusArea : ToolStripStatusLabel, IObserver<String>
    {
        IObservable<long> ticker = Observable.Interval(tickerInterval());
        public StatusArea()
        {
            AutoSize = false;
            Dock = DockStyle.Fill;
            TextAlign = ContentAlignment.MiddleLeft;
            GlobalEventBrokers.StatusMessageBroker.Subscribe(this);
            setupStatusClearPeriodicTask();
        }

        private DateTime _lastUpdateTimestamp = DateTime.Now;
        public void OnNext(string value)
        {
            Text = value;
            _lastUpdateTimestamp = DateTime.Now;
        }

        // Set up a job 30s from now, to clear the status bar.
        private static TimeSpan tickerInterval()
        {
            return TimeSpan.FromSeconds(Convert.ToInt32(ConfigurationManager.AppSettings["statusClearResolutionInSeconds"]));
        }

        // Clear the status bar, and start another task, so that the cycle never ends.
        private void setupStatusClearPeriodicTask()
        {
            ticker.Subscribe(x =>
            {
                Task task = new Task(() => { if (displayTimeElapsed()) Text = ""; });
                task.Start();
            });
        }

        // Check if the current status message has been displayed for at least 30s
        private bool displayTimeElapsed()
        {
            var limit = Convert.ToInt32(ConfigurationManager.AppSettings["statusDisplayIntervalInSeconds"]);
            return DateTime.Now.Subtract(_lastUpdateTimestamp).Seconds > limit;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }
}
