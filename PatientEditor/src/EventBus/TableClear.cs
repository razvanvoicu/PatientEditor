namespace MindLinc.EventBus
{
    // This event is issued by SqlConnection when its filter has changed. It indicates to
    // the DB grid that it needs to be cleared, so that the new set of filtered patients can be sent.
    // Clearing and then redisplaying is really fast, and appears as atomic to the human eye.
    public class DbTableClear { }


    // Similar to the above, but for the FhirConnection and the FhirGrid
    public class FhirTableClear { }
}
