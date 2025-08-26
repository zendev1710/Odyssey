using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.ViewModels
{
    internal static class Ids
    {
        public const string MainWindowVmId = "main";


        // These ids are used to identify the view models for the various docked views in the application.
        // Their values are also used as translation key.

        // Docked views view model ids
        public const string Map = "map";
        public const string MiniMap = "minimap";
        public const string Explorer = "explorer";
        public const string Bookmarks = "bookmarks";
        public const string History = "history";
        public const string RegionProperties = "region_properties";
        public const string RegionStatistics = "region_statistics";
        public const string Details = "details";
        public const string UnitOrders = "unit_orders";
        public const string RegionInfo = "region_info";
        public const string Battles = "battles";
        public const string SearchResults = "search_results";
        public const string ReportInfo = "report_info";
        public const string ErrorList = "error_list";

        // Navigation views view model ids
        public const string Home = "Home";
        public const string Search = "Search";
        public const string Configuration = "Configuration";

        // other views view model ids
        public const string ShipDetails = "ship_details";
        public const string BuildingDetails = "building_details";
        public const string UnitDetails = "unit_details";
        public const string NoDetails = "no_details";

    }
}
