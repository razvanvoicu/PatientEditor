using Microsoft.VisualStudio.TestTools.UnitTesting;
using MindLinc.EventBus;
using MindLinc.Model;
using Moq;
using System;
using System.Data.Entity;
using System.Reactive.Linq;

namespace MindLinc.Connection.Tests
{
    [TestClass()]
    public class SqlConnectionTests
    {
        string outerStatus = null;

        [TestMethod()]
        public void SqlConnectionTest_ConnectionToDatabaseSucceeds()
        {
            GlobalEventBrokers.StatusMessageBroker.Subscribe(value => { outerStatus = value; });
            var sqlConnection = new SqlConnection();
            Assert.IsTrue(sqlConnection.cache.Count > 0);
        }

        [TestMethod()]
        public void OnNext_CreatePatientInvokesAdd()
        {
            GlobalEventBrokers.StatusMessageBroker.Subscribe(value => { outerStatus = value; });
            var sqlConnection = new SqlConnection();
            var mockContext = new Mock<PatientDbContext>();
            var mockSet = new Mock<DbSet<Patient>>();
            mockContext.Setup(m => m.patients).Returns(mockSet.Object);
            sqlConnection.Db = mockContext.Object;
            var createRequest = new CreatePatient();
            createRequest.Patient = new Patient();
            createRequest.Patient.id = "ML0001";
            sqlConnection.OnNext(createRequest);
            mockSet.Verify(m => m.Add(It.IsAny<Patient>()), Times.Once());
        }

        [TestMethod()]
        public void OnNext_ModificationOfPatientFailsWithEmptyPatientSet()
        {
            GlobalEventBrokers.StatusMessageBroker.Subscribe(value => { outerStatus = value; });
            var sqlConnection = new SqlConnection();
            var mockContext = new Mock<PatientDbContext>();
            var mockSet = new Mock<DbSet<Patient>>();
            mockContext.Setup(m => m.patients).Returns(mockSet.Object);
            sqlConnection.Db = mockContext.Object;
            var patientChangeRequest = new PatientChange("1", "Family Name", "New Name");
            sqlConnection.OnNext(patientChangeRequest);
            var expected = "Modification failed for patient id [1] with field [Family Name], new value [New Name]";
            StringAssert.Contains(outerStatus, expected);
        }

        [TestMethod()]
        public void OnNext_ModificationOfPatientSucceedsOnExistingPatient()
        {
            GlobalEventBrokers.StatusMessageBroker.Subscribe(value => { outerStatus = value; });
            var sqlConnection = new SqlConnection();
            var patientChangeRequest = new PatientChange("ML0499", "Family Name", "New Name");
            sqlConnection.OnNext(patientChangeRequest);
            var expected = "Modification succeeded for patient id [ML0499]";
            StringAssert.Contains(outerStatus, expected);
        }

        [TestMethod()]
        public void modifyPatientField_ChangeFieldOfTypeString()
        {
            var newName = "New Name";
            var id = "ML0499";
            var patientChangeRequest = new PatientChange(id, "Family Name", newName);
            var patient = new Patient();
            patient.id = id;
            patient.family_name = "old name";
            var sqlConnection = new SqlConnection();
            sqlConnection.modifyPatientField(patientChangeRequest, patient);
            Assert.AreEqual(newName, patient.family_name);
        }

        [TestMethod()]
        public void modifyPatientField_ChangeFieldOfTypeDateTime()
        {
            var id = "ML0499";
            var newBirthDate = "1980-01-01";
            var patientChangeRequest = new PatientChange(id, "Birth Date", newBirthDate);
            var patient = new Patient();
            patient.id = id;
            patient.birth_date = DateTime.Parse("1970-12-12");
            var sqlConnection = new SqlConnection();
            sqlConnection.modifyPatientField(patientChangeRequest, patient);
            Assert.AreEqual(DateTime.Parse(newBirthDate), patient.birth_date);
        }

        [TestMethod()]
        public void modifyPatientField_ChangeFieldOfTypeBool()
        {
            var id = "ML0499";
            var newActive = "False";
            var patientChangeRequest = new PatientChange(id, "Active", newActive);
            var patient = new Patient();
            patient.id = id;
            patient.active = true;
            var sqlConnection = new SqlConnection();
            sqlConnection.modifyPatientField(patientChangeRequest, patient);
            Assert.IsTrue(! patient.active);
        }

        [TestMethod()]
        public void OnNext_CheckUniqueGeneratesIsUnique()
        {
            IsUnique witness = null;
            var sqlConnection = new SqlConnection();
            var checkUnique = new CheckUnique();
            checkUnique.Id = "abc";
            GlobalEventBrokers.IsUniqueBroker.Subscribe(value => witness = value);
            sqlConnection.OnNext(checkUnique);
            Assert.AreEqual(witness.Id, checkUnique.Id);
        }
    }
}
