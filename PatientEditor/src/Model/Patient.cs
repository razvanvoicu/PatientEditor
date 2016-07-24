using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Forms;
using FhirModel = Hl7.Fhir.Model;

namespace MindLinc.Model
{
    public class Patient
    {
        public Patient(): base() { }
        public Patient(FhirModel.Patient fhirPatient)
        {
            family_name = fhirPatient.FamilyName();
            given_name = fhirPatient.GivenName();
            id = fhirPatient.Id;
            birth_date = fhirPatient.BirthDate();
            gender = fhirPatient.Gender();
            marital_status = fhirPatient.MaritalStatus();
            address = fhirPatient.Address();
            telecom = fhirPatient.Telecom();
            language = fhirPatient.GetLanguage();
            managing_organization = fhirPatient.ManagingOrganization();
            active = fhirPatient.Active();
        }

        [StringLength(80)]
        public string id { get; set; }

        [StringLength(50)]
        public string family_name { get; set; }

        [StringLength(50)]
        public string given_name { get; set; }

        [Column(TypeName = "date")]
        public DateTime? birth_date { get; set; }

        [StringLength(10)]
        public string gender { get; set; }

        [StringLength(15)]
        public string marital_status { get; set; }

        [StringLength(150)]
        public string address { get; set; }

        [StringLength(50)]
        public string telecom { get; set; }

        [StringLength(30)]
        public string language { get; set; }

        [StringLength(30)]
        public string managing_organization { get; set; }

        public bool active { get; set; } = true;

        public DataGridViewRow ToDataGridViewRow()
        {
            var row = new DataGridViewRow();
            string dateString = birth_date == null ? null : birth_date.ToString();
            row.Cells.AddRange(new DataGridViewCell[]
                {
                    mkCell(id), mkCell(family_name), mkCell(given_name), mkCell(dateString),
                    mkCell(gender), mkCell(marital_status), mkCell(address), mkCell(telecom), mkCell(language),
                    mkCell(managing_organization), mkCell(active.ToString())
                });
            return row;
        }

        private DataGridViewCell mkCell(string content)
        {
            var cell = new DataGridViewTextBoxCell();
            cell.Value = content == null ? content : content.TrimEnd();
            return cell;
        }
    }
}
