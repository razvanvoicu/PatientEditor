using System;

namespace MindLinc.EventBus
{
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
