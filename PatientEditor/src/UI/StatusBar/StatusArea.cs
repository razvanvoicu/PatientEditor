using MindLinc.EventBus;
using System;
using System.Configuration;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MindLinc.UI.StatusBar
{
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

        private static TimeSpan tickerInterval()
        {
            return TimeSpan.FromSeconds(Convert.ToInt32(ConfigurationManager.AppSettings["statusClearResolutionInSeconds"]));
        }

        private void setupStatusClearPeriodicTask()
        {
            ticker.Subscribe(x =>
            {
                Task task = new Task(() => { if (displayTimeElapsed()) Text = ""; });
                task.Start();
            });
        }

        private bool displayTimeElapsed()
        {
            var limit = Convert.ToInt32(ConfigurationManager.AppSettings["statusDisplayIntervalInSeconds"]);
            return DateTime.Now.Subtract(_lastUpdateTimestamp).Seconds > limit;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) { }
    }
}
