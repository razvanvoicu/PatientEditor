using NLog;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace MindLinc.UI.Menu
{
    class ObservableMenuItem : ToolStripMenuItem, IObservable<EventArgs>
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();
        ObservableMenuItem[] _submenus;

        public ObservableMenuItem(string text, Keys shortcut, ObservableMenuItem[] submenus = null)
        {
            Text = text;
            _submenus = submenus;
            ShortcutKeys = shortcut;
            if (null != submenus)  DropDownItems.AddRange(submenus);
        }

        private IObservable<EventArgs> _innerObservable = null;

        public IDisposable Subscribe(IObserver<EventArgs> observer)
        {
            if (null == _innerObservable)
                _innerObservable = Observable.FromEventPattern<EventHandler,EventArgs>(
                        handler => Click += handler,
                        handler => Click -= handler
                    ).Select(ep => ep.EventArgs);
            return _innerObservable.Subscribe(observer);
        }

        public ObservableMenuItem[] getObservablesByText(string text)
        {
            return _submenus.Where(o => o.Text == text).ToArray();
        }
    }
}
