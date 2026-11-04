using Odyssey.Models.Data;
using System;

namespace Odyssey.Extensions;

public static class DocumentTypeExtensions
{
    public static bool IsReportType(this DocumentType value)
    {
        return value == DocumentType.ERESSEA_REPORT || value == DocumentType.ERESSEA_REPORT_FROM_ZIP;
    }

    public static bool IsOrdersType(this DocumentType value)
    {
        return value == DocumentType.ERESSEA_ORDERS;
    }
}
