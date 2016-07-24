namespace MindLinc.EventBus
{
    // Event issued by the 'New Patient' form, as a request to check for uniqueness of the new id.
    // It is consumed by SqlConnection.
    public class CheckUnique
    {
        public string Id { get; set; }
    }

    // Event issued by SqlConnection in response to CheckUnique. It is consumed by the 'New Patient' form;
    // if unique, 'New Form' will continue with a 'CreatePatient' request; if not, it will abort the creation.
    public class IsUnique
    {
        public string Id { get; set; }
        public bool Unique { get; set; }
    }
}
