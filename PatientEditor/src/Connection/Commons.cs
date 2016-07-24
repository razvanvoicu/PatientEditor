using MindLinc.Model;
using System;
using System.Linq;
using System.Reflection;

namespace MindLinc.Connection
{
    static class Commons
    {
        public static bool filterPass(Patient patient, Patient filter)
        {
            Type patientType = typeof(Patient);
            PropertyInfo[] props = patientType.GetProperties();
            var checkableProps = props.Where(p => filterCheckable(p, filter)).ToArray();
            if (checkableProps.Length == 0) return false;
            return patient.active && checkableProps.All(p => propertyMatches(p, patient, filter));
        }

        private static bool filterCheckable(PropertyInfo prop, Patient filter)
        {
            return prop.Name != "active"
                && prop.GetValue(filter) != null
                && prop.GetValue(filter).ToString().Length != 0;
        }

        private static bool propertyMatches(PropertyInfo prop, Patient patient, Patient filter)
        {
            return prop.GetValue(patient).ToString().ToLower()
                .Contains(prop.GetValue(filter).ToString().ToLower());
        }
    }
}
