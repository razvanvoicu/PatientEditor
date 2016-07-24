using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindLinc.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace MindLinc.EventBus.Tests
{
    [TestClass()]
    public class EventBrokerTests
    {

        [TestMethod()]
        public void RegisterAsPublisherTest()
        {
            string witness = null;
            var s = new Subject<string>();
            GlobalEventBrokers.StatusMessageBroker.RegisterAsPublisher(s);
            GlobalEventBrokers.StatusMessageBroker.Subscribe(val => witness = val);
            s.OnNext("test");
            Assert.AreEqual("test", witness);
        }
    }
}