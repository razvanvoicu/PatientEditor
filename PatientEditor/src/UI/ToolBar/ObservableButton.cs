using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace MindLinc.UI.ToolBar
{
    // Buttons converted into Reactive Observables
    class ObservableButton : ToolStripButton, IObservable<EventArgs>
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();
        private IObservable<EventArgs> _innerObservable = null;
        public IDisposable Subscribe(IObserver<EventArgs> observer)
        {
            if (null == _innerObservable)
                _innerObservable = Observable.FromEventPattern<EventHandler, EventArgs>(
                    handler => Click += handler,
                    handler => Click -= handler
                    ).Select(ep => ep.EventArgs);
            return _innerObservable.Subscribe(observer);
        }
    }
}
