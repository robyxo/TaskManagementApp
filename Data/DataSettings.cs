namespace TaskManagementApp.Data;

/// <summary>
/// 1. I create the settings class bound to the "DataSettings" section of appsettings.json
/// With UseDatabase I decide at startup where the data comes from
/// </summary>
public class DataSettings
{
    // 2. Name of the section in appsettings.json
    public const string SectionName = "DataSettings";

    // 3. true = PostgreSQL, false = in-memory dictionaries loaded from the JSON seed file
    public bool UseDatabase { get; set; }

    // 4. Label I show in the navbar so it is always clear which mode is running
    public string ModeLabel => UseDatabase ? "PostgreSQL" : "Demo data (in-memory)";
}
