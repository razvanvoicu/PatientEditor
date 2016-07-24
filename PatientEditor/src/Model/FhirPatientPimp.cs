using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using FhirModel = Hl7.Fhir.Model;

namespace MindLinc.Model
{
    public static class FhirPatientPimp
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static void checkValid(FhirModel.Patient fhirPatient)
        {
            if (fhirPatient == null)
            {
                logger.Error("FHIR Patient is null: {0}", new StackTrace());
                throw new ArgumentException("Received null as FHIR Patient");
            }
            if (fhirPatient.Id == null)
            {
                logger.Error("FHIR Patient Id is null: {0}", new StackTrace());
                throw new ArgumentException("Received null as FHIR Patient Id");
            }
        }

        public static string FamilyName(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.Name != null &&
                fhirPatient.Name.Count != 0 &&
                fhirPatient.Name[0] != null &&
                fhirPatient.Name[0].Family != null &&
                fhirPatient.Name[0].Family.Count() != 0)
            {
                return fhirPatient.Name[0].Family.First();
            }
            else
            {
                return "";
            }
        }

        public static string GivenName(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.Name != null &&
                fhirPatient.Name.Count != 0 &&
                fhirPatient.Name[0] != null &&
                fhirPatient.Name[0].Given != null &&
                fhirPatient.Name[0].Given.Count() != 0)
            {
                return fhirPatient.Name[0].Given.First();
            }
            else
            {
                return "";
            }
        }

        public static DateTime BirthDate(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.BirthDate != null)
                return DateTime.Parse(fhirPatient.BirthDate);
            else
                return DateTime.Parse("1/1/1980");
        }

        public static string Gender(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.Gender.HasValue)
                return fhirPatient.Gender.Value.ToString();
            else
                return "";
        }

        public static string MaritalStatus(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.MaritalStatus != null &&
                fhirPatient.MaritalStatus.Text != null)
            {
                return fhirPatient.MaritalStatus.Text;
            } 
            else
            {
                return "";
            }
        }

        public static string Address(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.Address != null &&
                fhirPatient.Address.Count() != 0 &&
                fhirPatient.Address.First() != null &&
                fhirPatient.Address.First().Line != null &&
                fhirPatient.Address.First().Line.First() != null)
            {
                return fhirPatient.Address.First().Line.First();
            }
            else
            {
                return "";
            }
        }

        public static string Telecom(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.Telecom != null &&
                fhirPatient.Telecom.Count != 0 &&
                fhirPatient.Telecom[0] != null &&
                fhirPatient.Telecom[0].Value != null)
            {
                return fhirPatient.Telecom[0].Value;
            }
            else
            {
                return "";
            }
        }

        public static string GetLanguage(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.Language != null)
            {
                return fhirPatient.Language;
            }
            else
            {
                return "";
            }
        }

        public static string ManagingOrganization(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.ManagingOrganization != null &&
                fhirPatient.ManagingOrganization.Display != null)
            {
                return fhirPatient.ManagingOrganization.Display;
            }
            else
            {
                return "";
            }
        }

        public static bool Active(this FhirModel.Patient fhirPatient)
        {
            checkValid(fhirPatient);
            if (fhirPatient.Active.HasValue)
                return fhirPatient.Active.Value;
            else
                return true;
        }
    }
}
