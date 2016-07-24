using MindLinc.Model;
using System;

namespace MindLinc.EventBus
{
    public class FinderUpdated
    {
        public string ContainerTitle { get; set; }

        public Patient Patient { get; set; } = new Patient();

        public bool Submit { get; set; }

        public override string ToString()
        {
            return String.Format(
                "Issued by: {0}; id={1} family_name={2} given_name={3} " +
                "birth_date={4} gender={5} marital_status={6} address={7} telecom={8} language={9} " +
                "managing_organization={10} active={11} submit={12}",
                ContainerTitle, Patient.id, Patient.family_name, Patient.given_name, Patient.birth_date,
                Patient.gender, Patient.marital_status, Patient.address, Patient.telecom, Patient.language,
                Patient.managing_organization, Patient.active, Submit
                );
        }
    }
}
