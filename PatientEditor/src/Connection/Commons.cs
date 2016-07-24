using MindLinc.Model;
using System;
using System.Linq;
using System.Reflection;

namespace MindLinc.Connection
{
    // Code used by both connection for filtering records
    static class Commons
    {
        // If the patient passes the filter, then the patient will be displayed in the grid.
        // The filter is represented as a Patient whose fields, when converted to strings, must
        // be contained in the corresponding fields of the patient.
        public static bool filterPass(Patient patient, Patient filter)
        {
            Type patientType = typeof(Patient); // use reflection to iterate through the fields; no need to change here when adding/modifying a field.
            PropertyInfo[] props = patientType.GetProperties();
            var checkableProps = props.Where(p => filterCheckable(p, filter)).ToArray();
            if (checkableProps.Length == 0) return false;
            return patient.active && checkableProps.All(p => propertyMatches(p, patient, filter));
        }

        // Only fields that are non-null and non-empty take part in the filtering
        private static bool filterCheckable(PropertyInfo prop, Patient filter)
        {
            return prop.Name != "active"
                && prop.GetValue(filter) != null
                && prop.GetValue(filter).ToString().Length != 0;
        }

        // Check if, for a given field (represented by the reflected prop), a patient matches the filter
        private static bool propertyMatches(PropertyInfo prop, Patient patient, Patient filter)
        {
            var patientProp = prop.GetValue(patient);
            return patientProp != null && patientProp.ToString().ToLower()
                .Contains(prop.GetValue(filter).ToString().ToLower());
        }
    }
}
