using Avalonia.Controls.Shapes;
using Avalonia.Metadata;
using AvaloniaEdit.Editing;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace Odyssey.Models.Dal
{
    public class OrdersParser
    {
        private int UnitId { get; set; }
        private OrdersAttachment RegionOrders { get; set; }
        private OrdersAttachment? UnitOrders { get; set; }
        private string UnitKeyword { get; set; }
        private string NextKeyword { get; set; }
        private StringBuilder StringBuilder { get; set; }
        public string Password { get; private set; }
        public int LinesNumber { get; private set; }
        public OrdersDocument? OrdersDocument { get; private set; }
        public GameLanguage Locale { get; private set; }
        public string Text { get; private set; }
        public List<string> PrefixLines { get; private set; }

        public OrdersParser(GameLanguage locale = GameLanguage.UNKNOWN)
        {
            UnitId = 0;
            Text = "";
            Password = "";
            Locale = locale;
            RegionOrders = new OrdersAttachment();
            UnitOrders = new OrdersAttachment();
            StringBuilder = new StringBuilder();

            // English by default if not german
            UnitKeyword = locale == GameLanguage.DE ? "EINHEIT" : "UNIT";
            NextKeyword = locale == GameLanguage.DE ? "NAECHSTER" : "NEXT";

            LinesNumber = 0;
            PrefixLines = [];

        }

        public bool ReadOrders(StreamReader reader, out string errorMessage)
        {
            // Check first line : ERESSEA <faction id> "password" or PARTEI <faction id> "password"
            if (!CheckHeader(reader, out errorMessage))
            {
                return ReadError(errorMessage);
            }

            List<string> prefixLines;
            GameLanguage locale = Locale;
            string firstlineWithStatement = ReadPrefixLines(reader, ref locale, out prefixLines);
            if (Locale != GameLanguage.UNKNOWN)
            {
                if (locale != Locale)
                {
                    Debug.WriteLine($"[WARNING] Orders locale {locale} is not as expected ({Locale})");
                }
            }
            else
            {
                Locale = locale;
            }

            PrefixLines.AddRange(prefixLines);
            if (string.IsNullOrWhiteSpace(firstlineWithStatement))
            {
                errorMessage = "No order found!";
                return ReadError(errorMessage);
            }

            if (!ProcessLine(firstlineWithStatement, out errorMessage))
            {
                Debug.WriteLine($"First line with statement process failure. Line={firstlineWithStatement} LinesNumber=({LinesNumber})");
                return ReadError(errorMessage);
            }

            // Read lines
            string? line = string.Empty;
            while ((line = ReadLine(reader)) != null)
            {
                if (!ProcessLine(line, out errorMessage))
                {
                    Debug.WriteLine($"Line process failed. Line={line} LinesNumber=({LinesNumber})");
                    return ReadError(errorMessage);
                }
            }

            Text = StringBuilder.ToString();
            OrdersDocument = new OrdersDocument(this);

            Debug.WriteLine($"Orders read: locale={Locale} LinesNumber=({LinesNumber})");
            return true;
        }

        private bool ReadError(string errorMessage)
        {
            Debug.WriteLine($"Orders read failure: {errorMessage}");
            Debug.WriteLine($"locale={Locale} LinesNumber=({LinesNumber})");
            return false;
        }

        private bool CheckHeader(StreamReader reader, out string errorMessage)
        {
            string? line;
            errorMessage = "";
            while ((line = ReadLine(reader)) != null)
            {
                if (line.StartsWith(';'))
                {
                    continue;
                }
                line = line.Trim();
                string[] words = line.Split();
                if (words.Length < 3 || (words[0] != "ERESSEA" && words[0] != "PARTEI"))
                {
                    errorMessage = "This is not an orders text file!";
                    return false;
                }
                int factionId = Utils.Converters.DecodeBase36(words[1]);
                if (factionId == 0)
                {
                    errorMessage = "Invalid faction id!";
                    return false;
                }
                Password = words[2].Trim('"');
                break;
            }
            return true;
        }
        private string? ReadLine(StreamReader reader)
        {
            string? line = reader.ReadLine();
            if (line != null)
            {
                StringBuilder.Append(line);
                StringBuilder.Append(Environment.NewLine);
                LinesNumber++;
            }
            return line;
        }
        public bool ProcessLine(string fullLine, out string errorMessage)
        {
            LinesNumber++;
            errorMessage = "";
            if (fullLine.StartsWith(';'))
            {
                // nothing processed
                return true;
            }
            // TODO: decode not UTF-8 lines (ISO to UTF-8)

            //string line = fullLine.Replace('\t', ' ').Trim(' ', '\t');
            string line = fullLine. Trim(' ', '\t'/*, '\r', '\n', Environment.NewLine */);
            // TODO: handle empty (and with \r\n) lines
            string[] words = line.Split([' ', '\t', ';'], 10);
            string cmd = words[0].ToUpper();
            string param = words.Length >= 2 ? words[1] : "";
            // TODO: check EN and GERMAN if locale is unkown
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
                // TODO: keep region in a list
            }
            return true;
        }
        private bool ProcessUnitKeyword(string param, out string errorMessage)
        {
            DataBlock? unit = null;
            errorMessage = "";
            UnitId = Utils.Converters.StringIdToInt(param);

            // TODO: keep unit in a list
            /*
            Unit = unit!;
            if (Region != null)
            {
                DataBlock? region = Region;
                Coordinates coords = new(region.GetX(), region.GetY(), region.GetId());
                if (!CRDocument.HasChild(region, Unit))
                {
                    if (!CRDocument.GetParent(ref region, Unit))
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
            */
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

        /// <summary>
        /// Read all lines before the first UNIT or REGION statement.
        /// </summary>
        /// <param name="reader">Stream reader used to read orders file</param>
        /// <param name="prefixLines">Lines read until first UNIT or REGION statement</param>
        /// <returns>the first line with a UNIT or REGION statement</returns>
        private string ReadPrefixLines(StreamReader reader, ref GameLanguage locale, out List<string> prefixLines)
        {
            prefixLines = [];
            string? line = string.Empty;
            bool prefixLinesEnded = false;
            while ((line = ReadLine(reader)) != null)
            {
                line = line.Trim(' ', '\t');
                string[] words = line.Split(' ');
                string cmd = words[0].ToUpper();
                switch (cmd)
                {
                    case "LOCALE":
                        locale = words[1].ToLocaleType();
                        break;
                    case "REGION":
                    case "UNIT":
                    case "EINHEIT":
                        prefixLinesEnded = true;
                        break;
                }
                if (prefixLinesEnded)
                {
                    break;
                }
                prefixLines.Add(line);
            }
            return line ?? "";
        }
    }
}
