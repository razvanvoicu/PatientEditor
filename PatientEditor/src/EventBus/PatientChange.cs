using System;

namespace MindLinc.EventBus
{
    // This event is issued by a cell content change in the DB data grid. It is consumed by SqlConnection.
    public class PatientChange
    {
        public string Id { get; set; }
        public string Field { get; set; }
        public string NewValue { get; set; }
        public PatientChange(String id, String field, String newValue)
        {
            Id = id;
            Field = field;
            NewValue = newValue;
        }
    }
}
