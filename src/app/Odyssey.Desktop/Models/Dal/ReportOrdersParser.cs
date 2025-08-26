using Avalonia.Controls.Shapes;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Models.Dal
{
    public class ReportOrdersParser
    {
        private DataBlock? Region { get; set; }
        private DataBlock? Unit { get; set; }
        private int UnitId { get; set; }
        private OrdersAttachment RegionOrders { get; set; }
        private OrdersAttachment? UnitOrders { get; set; }
        private CRDocument Report { get; set; }
        private string UnitKeyword { get; set; }
        private string NextKeyword { get; set; }

        private GameLanguage Locale { get; set; }


        public ReportOrdersParser(CRDocument report, GameLanguage locale)
        {
            Report = report;
            Region = null;
            Unit = null;
            UnitId = 0;
            RegionOrders = new OrdersAttachment();
            UnitOrders = new OrdersAttachment();
            Locale = Report.Locale;
            if (Locale == GameLanguage.UNKNOWN)
            {
                Locale = locale;
            }
            UnitKeyword = Locale == GameLanguage.DE ? "EINHEIT" : "UNIT";
            NextKeyword = Locale == GameLanguage.DE ? "NAECHSTER" : "NEXT";
        }

        public bool ProcessLine(string fullLine, out string errorMessage)
        {
            errorMessage = "";
            if (fullLine.StartsWith(';'))
            {
                // nothing processed
                return true;
            }
            // LATER: decode not UTF-8 lines (ISO to UTF-8)
            string line = fullLine.Trim(' ', '\t'/*, '\r', '\n', Environment.NewLine */);
            // TODO: handle empty (and with \r\n) lines
            string[] words = line.Split(' ', 2);
            string cmd = words[0].ToUpper();
            string param = words.Length == 2 ? words[1] : "";
            if (cmd == UnitKeyword || cmd == "REGION")
            {
                if (cmd == UnitKeyword)
                {
                    if (!ProcessUnitKeyword(param, out errorMessage))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!ProcessRegionKeyword(param, out errorMessage))
                    {
                        return false;
                    }
                }
            }
            else if (cmd == NextKeyword)
            {
                return true;
            }
            else
            {
                // Process each line
                ProcessOrdersLine(line);

            }
            return true;
        }
        private bool ProcessRegionKeyword(string param, out string errorMessage)
        {
            UnitId = 0;
            errorMessage = "";
            if (Utils.Converters.ExtractCoordinates(param, out int x, out int y, out int z))
            {
                DataBlock? region = null;
                // TODO: check if region can be an unknown one in orders
                if (!Report.GetRegion(ref region, x, y, z))
                {
                    errorMessage = $"Region not found: {param}";
                    return false;
                }
                UnitOrders = RegionOrders;
            }
            return true;
        }
        private bool ProcessUnitKeyword(string param, out string errorMessage)
        {
            DataBlock? unit = null;
            errorMessage = "";
            UnitId = Utils.Converters.StringIdToInt(param);
            if (!Report.GetUnit(ref unit, UnitId))
            {
                errorMessage = $"Unit not found: {param}";
                return false;
            }
            Unit = unit!;
            if (Region != null)
            {
                DataBlock? region = Region;
                Coordinates coords = new(region.GetX(), region.GetY(), region.GetId());
                if (!CRDocument.HasKnownChild(region, Unit))
                {
                    if (!CRDocument.GetKnownParent(ref region, Unit))
                    //if (!CRDocument.GetParent(ref region, Unit))
                    {
                        errorMessage = $"Unit in wrong region: {param}";
                        return false;
                    }
                    RegionOrders.Header = GetRegionStatement(region);
                }
                coords = new(region.GetX(), region.GetY(), region.GetId());
                Report.OrdersDocument.RegionLines[coords] = RegionOrders;
                RegionOrders?.Clear();
                Region = null;
            }
            // TODO: cumbersome, but reading orders without a `; bestaetigt` comment must do this.
            Report.SetConfirmed(ref unit, false);
            DataBlock? commandsBlock = null;
            if (!CRDocument.GetCommands(ref commandsBlock, unit.Node))
            {
                errorMessage = $"Unit has no command block: {param}";
                return false;
            }
            UnitOrders = commandsBlock!.GetAttachment() as OrdersAttachment;
            return true;
        }
        private void ProcessOrdersLine(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                /*
                if (str.left(1) == ";")
					{
						cmd = str.section(' ', 1);
						if (unitId != 0 && flatten(cmd.lower()) == "bestaetigt")
						{
							// don't add "; bestaetigt" comment, just set confirmed flag
							setConfirmed(block, true);
						}
						else if (indent > headerindent)
						{
							cmds_list.addCommand(str);
						}
						else if (cmds_list.commands.Count == 0)
						{
							cmds_list.prefix_lines.Add(str);
						}
						else
						{
							// add to postfix if it follows some commands
							cmds_list.postfix_lines.Add(str);
						}
					}
					else
					{
						cmds_list.addCommand(str);
					}
                */
            }
        }
        /// <summary>
        /// orginally named regionCommand.
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        private static string GetRegionStatement(in DataBlock region)
        {
            int id = region.GetId();
            int x = region.GetX();
            int y = region.GetY();
            string name = region.Value(KeyType.NAME);
            int terrainType = region.GetTerrain();
            string terrainTypeName = Terrains.GetLabel(terrainType);
            string suffix = id == 0 ? "" : $", {id}";
            return $"REGION {x}, {y}{suffix} ; {name} ({terrainTypeName})";
        }
    }
}
