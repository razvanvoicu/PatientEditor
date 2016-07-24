using MindLinc.Model;

namespace MindLinc.EventBus
{
    // Event emitted when a patient is created by the 'New Patient' form.
    // The event is consumed by the SqlConnection class
    public class CreatePatient
    {
        public Patient Patient { get; set; }
    }
}