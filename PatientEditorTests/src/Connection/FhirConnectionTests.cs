using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindLinc.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;
using System.Configuration;
using MindLinc.Model;
using MindLinc.EventBus;

namespace MindLinc.Connection.Tests
{
    [TestClass()]
    public class FhirConnectionTests
    {
        [TestMethod()]
        public void FhirConnectionTest()
        {
            var fhirConnection = new FhirConnection(startFetchingData: false);
            var fhirUrl = ConfigurationManager.AppSettings["fhirUrl"];
            var client = new FhirClient(fhirUrl);
            fhirConnection.fetchPatient(client, "1");
            Assert.IsTrue(fhirConnection.cache.Count > 0);
        }

        [TestMethod()]
        public void OnNext_FinderUpdate()
        {
            FhirTableClear tableClearWitness = null;
            Patient tableUpdateWitness = null;
            GlobalEventBrokers.FhirTableClearBroker.Subscribe(value => tableClearWitness = value);
            GlobalEventBrokers.FhirPatientBroker.Subscribe(value => tableUpdateWitness = value);
            var fhirConnection = new FhirConnection(startFetchingData: false);
            var p = new Patient();
            p.id = "10";
            fhirConnection.cache[p.id] = p;
            var f = new FinderUpdated();
            var fp = new Patient();
            fp.id = "1";
            f.Patient = fp;
            fhirConnection.OnNext(f);
            Assert.IsFalse(tableClearWitness == null);
            Assert.AreEqual(p.id, tableUpdateWitness.id);
        }
    }
}