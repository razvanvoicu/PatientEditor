using NLog;
using System;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace MindLinc.UI.TabbedEditor
{
    class ObservableTextBox : TextBox, IObservable<Tuple<string,string,string>>
    {
        static private Logger logger = LogManager.GetCurrentClassLogger();

        string ContainerTitle { get; set; }

        public ObservableTextBox(string containerTitle)
        {
            ContainerTitle = containerTitle;
            TextChanged += (o, e) =>
            {
                _innerObservable.OnNext(Tuple.Create(ContainerTitle, Name, Text));
            };
        }

        private ISubject<Tuple<string,string,string>> _innerObservable = new Subject<Tuple<string,string,string>>();
        public IDisposable Subscribe(IObserver<Tuple<string,string,string>> observer)
        {
            return _innerObservable.Subscribe(observer);
        }
    }
}
