namespace MindLinc.UI.ToolBar
{
    // Symbolic constants used in building the buttons.
    class ButtonText
    {
        public const string DEACTIVATE = "Deactivate";
        public const string IMPORT_FHIR = "Import FHIR";
        public const string NEW_PATIENT = "New Patient";
        public const string SEPARATOR = "separator";

        public static string[] ButtonNames = {
            NEW_PATIENT,
            DEACTIVATE,
            SEPARATOR,
            IMPORT_FHIR,
        };
    }
}
