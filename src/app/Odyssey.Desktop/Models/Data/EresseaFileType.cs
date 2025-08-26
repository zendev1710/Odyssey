namespace Odyssey.Models.Data
{
    /// <summary>
    /// Type of Eressea file.
    /// </summary>
    public enum EresseaFileType
    {
        // Report file (.cr)
        REPORT,
        // Report file enbedded in zip (.zip)
        REPORT_FROM_ZIP,
        // Orders file (.txt)
        ORDERS,
        // Unknown type file
        UNKNOWN,
    }
    
}
