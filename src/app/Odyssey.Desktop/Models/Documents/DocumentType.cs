namespace Odyssey.Models.Data
{
    /// <summary>
    /// Type of Eressea file.
    /// </summary>
    public enum DocumentType
    {
        // Report file (.cr)
        ERESSEA_REPORT,
        // Report file enbedded in zip (.zip)
        ERESSEA_REPORT_FROM_ZIP,
        // Orders file (.txt)
        ERESSEA_ORDERS,
        // Other kind of text file (.txt)
        TXT,
        //  Other kind of file (.doc, .pdf, .odt, ...)
        OTHER,
        // Unknown type file
        UNKNOWN,
    }
    
}
