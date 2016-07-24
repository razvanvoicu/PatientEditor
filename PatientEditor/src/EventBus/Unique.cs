namespace MindLinc.EventBus
{
    public class CheckUnique
    {
        public string Id { get; set; }
    }

    public class IsUnique
    {
        public string Id { get; set; }
        public bool Unique { get; set; }
    }
}
