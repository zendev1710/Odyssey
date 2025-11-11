using Avalonia.Controls.Shapes;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Tools;
using DryIoc.ImTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Reactive;
using Tmds.DBus.Protocol;
using static System.Reflection.Metadata.BlobBuilder;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Odyssey.Models.Dal
{
    public static class OrdersFileService
    {
        /// <summary>
        /// Load orders file from the given pathname.
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="report"></param>
        /// <param name="outError"></param>
        /// <returns></returns>
        public static bool LoadFile(in string pathname, ref CRDocument report, out string errorMessage)
        {
            bool loaded = false;
            errorMessage = "";
            string password = "";
            if (string.IsNullOrEmpty(pathname))
            {
                errorMessage = "No pathname given!";
                return false;
            }

            using (var fileStream = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    // Check first line : ERESSEA <faction id> "password" or PARTEI <faction id> "password"
                    if (!CheckHeader(reader, out password, out errorMessage))
                    {
                        return false;
                    }
                    GameLanguage locale = report.Locale;
                    // TODO
                    //report.OrdersDocument.PrefixLines.Clear();
                    //report.OrdersDocument.RegionLines.Clear();
                    List<string> prefixLines;
                    string firstlineWithStatement = ReadPrefixLines(reader, ref locale, out prefixLines);
                    ReportOrdersParser parser = new ReportOrdersParser(report, locale);
                    // TODO
                    //report.OrdersDocument.PrefixLines.AddRange(prefixLines);

                    if (string.IsNullOrWhiteSpace(firstlineWithStatement))
                    {
                        errorMessage = "No order found!";
                        return false;
                    }

                    if (!parser.ProcessLine(firstlineWithStatement,out errorMessage))
                    {
                        return false;
                    }

                    // Read lines
                    string? line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!parser.ProcessLine(line, out errorMessage))
                        {
                            return false;
                        }
                        
                    }
                    loaded = true;
                }
            }
            return loaded;
        }

        /// <summary>
        /// Loads an Eressea orders file document from the specified file pathname.
        /// This file is not linked to a known report file.
        /// </summary>
        /// <remarks>This method attempts to read and parse the specified file as an Eressea document. If
        /// the file cannot be opened, read, or parsed, the method returns <see langword="false"/> and provides an error
        /// message in <paramref name="errorMessage"/>. The <paramref name="eresseaDocument"/> parameter will contain a
        /// default instance of <see cref="OrdersDocument"/> in such cases.</remarks>
        /// <param name="pathname">The full path to the file to be loaded. Must not be null or empty.</param>
        /// <param name="eresseaDocument">When this method returns, contains the loaded <see cref="EresseaDocument"/> if the operation succeeds;
        /// otherwise, contains a default instance of <see cref="OrdersDocument"/>.</param>
        /// <param name="errorMessage">When this method returns, contains an error message if the operation fails; otherwise, an empty string.</param>
        /// <returns><see langword="true"/> if the file was successfully loaded and parsed; otherwise, <see langword="false"/>.</returns>
        public static bool LoadFile(in string pathname, out EresseaDocument eresseaDocument, out string errorMessage)
        {
            bool loaded = false;
            errorMessage = "";
            eresseaDocument = new OrdersDocument();
            if (string.IsNullOrEmpty(pathname))
            {
                errorMessage = "No pathname given!";
                return false;
            }
            OrdersParser parser = new OrdersParser();
            using (var fileStream = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    if (parser.ReadOrders(reader, out errorMessage))
                    {
                        eresseaDocument = parser.OrdersDocument!;
                        loaded = true;
                    }
                }
            }
            return loaded;
        }

        /// <summary>
        /// Saves orders file.
        /// </summary>
        /// <param name="report"></param>
        /// <param name="pathname"></param>
        /// <param name="password"></param>
        /// <param name="stripped"></param>
        /// <returns></returns>
        public static bool Save(ref readonly CRDocument report, in string pathname, in string password, bool stripped)
        {
            if (string.IsNullOrEmpty(pathname))
            {
                return false;
            }
            if (report.IsEmpty())
            { 
                return false; 
            }
            if (!report.HasActiveFaction())
            {
                return false;
            }

            GameLanguage reportLocale = report.Locale;
            if (reportLocale == GameLanguage.UNKNOWN)
            {
                return false;
            }

            try
            {
                // TODO: be able to save orders for several active factions ?
                // open text file for writing
                using StreamWriter writer = new(pathname, false);

                // Everything ok so far
                if (!stripped)
                {
                    // command export doesn't change modified flag
                    // TODO
                    //report.SetOrdersModified(false);
                }

                // get recruitment costs
                int recruitment = report.ActiveFaction.Recruitment;
                // TODO: check if it's what that should be done
                int activeFactionId = report.ActiveFaction.Id;
                if (recruitment == 0)
                {
                    // TODO: check if CR Recruitement property update is needed later
                    recruitment = 100;
                }

                // save header and echeck information	// "Eressea";Spiel
                LinkedListNode<DataBlock>? firstBlockNode = report.FirstBlock;
                DataBlock firstBlock = firstBlockNode!.Value;

                // CSMAP_APP_TITLE_VERSION = // Title / Copyright / Version
                string headerLine = $"Odyssey 0.9"; 
                string spielValue = firstBlock.Value(Strings.DE_GAME);
                string firstWord = spielValue == "Eressea" || spielValue == "E3" ? "ERESSEA" : "PARTEI";
                string UnitKeyword = reportLocale == GameLanguage.EN ? "UNIT" : "EINHEIT";
                string NextKeyword = reportLocale == GameLanguage.EN ? "NEXT" : "NAECHSTER";
                string factionId = Utils.Converters.IdToString(activeFactionId);
                writer.Write($"{firstWord} {factionId} ");
                writer.WriteLine($"\" {password} \"");

                // output prefix lines
                List<string> prefixLines = report.OrdersDocument.PrefixLines;
                if (!stripped && prefixLines.Count > 0)
                {
                    foreach (string line in prefixLines)
                    {
                        writer.WriteLine(line);
                    }
                }

                if (stripped)
                {
                    writer.WriteLine(writer.NewLine);
                    writer.WriteLine($"; ECHECK -v4.7 -l -w3 -r{recruitment}");
                    writer.WriteLine(writer.NewLine);
                    writer.WriteLine($"; {headerLine}");
                    writer.WriteLine(writer.NewLine);
                }

                DataBlock? region = null;
                for (var node = firstBlockNode; node != null; node = node.Next)
                {
                    DataBlock block = node.Value;
                    var b = block.GetBlockType();
                    if (b == BlockType.REGION)
                    {
                        region = block;
                    }
                    else if (b == BlockType.UNIT)
                    {
                        if (block.ValueInt(KeyType.FACTION) != activeFactionId)
                        {
                            continue;
                        }

                        DataBlock? cmds = null;
                        if (!CRDocument.GetCommands(ref cmds, node))
                        {
                            writer.WriteLine($"  ; Unit {block.GetId()} has no command block!");
                            continue;
                        }

                        if (region != null)
                        {
                            Coordinates coord = new Coordinates(region.GetX(), region.GetY(), region.GetId());
                            // TODO
                            /*
                            var it = m_cmds.region_lines.find(coord);
                            if (it != m_cmds.region_lines.end())
                            {
                                att_commands cmds = it.second;
                                writeCmds(@out, cmds);
                            }
                            else
                            {
                                // did not write a REGION header yet
                                @out << regionCommand(*region) << std::endl;

                                int salary = region.valueInt("Lohn");
                                if (region.valueInt("Lohn"))
                                {
                                    writer.WriteLine($"; ECheck Lohn {salary}";  // << std::endl << std::endl;
                                }
                            }
                            */
                            // TODO
                            //region = report.Blocks.Last();
                        }

                        // unit has command block
                        //  UNIT wz5t;  Ambassador of the Council [$1.146245,Beqwx(1/3)] does not fight
                        // TODO
                        /*
                        CommandsAttachment cmdsAttachment = new cmds.attachment();
                        if (attcmds != null && !attcmds.header.empty())
                        {
                            writer.WriteLine($"{attcmds.header}");
                        }
                        else
                        {
                            // get amount of silver from GEGENSTAENDE block
                            int silver = 0;

                            DataBlock items = block;
                            for (items++; items != m_blocks.end() && items.depth() > block.depth(); items++)
                            {
                                if (items.GetType() == BlockType.ITEMS)
                                {
                                    silver = items.ValueInt(KeyType.SILVER);
                                    break;
                                }
                            }

                            // output unit header
                            int id = block.GetId();
                            // TODO: check if GetUILabel is same as getName() returns the name of the unit
                            string name = block.GetUILabel();
                            int people = block.ValueInt(KeyType.NUMBER);
                            writer.WriteLine($"EINHEIT {id}";
                            writer.Write($";  {name}");
                            writer.WriteLine($" [{people},{silver}$]";
                        }

                        // output attachment (changed) or default commands
                        if (attcmds != null)
                        {
                            if (isConfirmed(block))
                            {
                                writer.WriteLine("; bestaetigt");
                            }

                            // output prefix lines
                            foreach (string itor in attcmds.prefix_lines)
                            {
                                writer.WriteLine(itor);
                            }

                            {
                                // output changed commands
                                foreach (string itor in attcmds.commands)
                                {
                                    @out << "    " << itor.text() << "\n";
                                }
                            }

                            writer.WriteLine(writer.NewLine);
                            // output postfix lines
                            foreach (string itor in attcmds.postfix_lines)
                            {
                                writer.WriteLine(itor);
                            }
                        }
                        else
                        {
                            // output default commands
                            foreach (DataKey dk in cmdb.GetData())
                            {
                                writer.WriteLine($"    {dk.GetValue()}");
                            }
                        }
                        */
                    }
                }
                writer.WriteLine(writer.NewLine);
                writer.WriteLine(NextKeyword);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!! Failed to save {pathname}. An error occurred: {ex.Message}");
            }
            return true;
        }

        /// <summary>
        /// Check the header (first line) of the orders text file.
        /// Should follow this pattern: [ERESSEA|PARTEI] faction_id "password".
        /// Lines beginning by ';' are ignored.
        /// </summary>
        /// <param name="reader">Streamreader used to read lines</param>
        /// <param name="password">Returned extracted password from header</param>
        /// <param name="errorMessage">Error message when headeer was not as expected; otherwise ""</param>
        /// <returns>true if header was as expected; otherwise false</returns>
        private static bool CheckHeader(StreamReader reader, out string password, out string errorMessage)
        {
            string? line;
            password = "";
            errorMessage = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith(';'))
                {
                    continue;
                }
                line = line.Trim();
                string[] words = line.Split();
                if (words.Length < 3 || (words[0] != "ERESSEA" && words[0] != "PARTEI"))
                {
                    errorMessage = "Invalid header line!";
                    return false;
                }
                int factionId = Utils.Converters.DecodeBase36(words[1]);
                if (factionId == 0)
                {
                    errorMessage = "Invalid faction id!";
                    return false;
                }
                /*
                if (report != null && report.HasData() && factionId != report.GetActiveFactionId())
                {
                    errorMessage = $"The orders are for another faction ({words[1]})!";
                    return false;
                }
                */
                // TODO: use it
                password = words[2].Trim('"');
                break;
            }
            return true;
        }

        /// <summary>
        /// Read all lines before the first UNIT or REGION statement.
        /// </summary>
        /// <param name="reader">Stream reader used to read orders file</param>
        /// <param name="prefixLines">Lines read until first UNIT or REGION statement</param>
        /// <returns>the first line with a UNIT or REGION statement</returns>
        private static string ReadPrefixLines(StreamReader reader, ref GameLanguage locale, out List<string> prefixLines)
        {
            prefixLines = [];
            // TODO: use ;locale "en" or "de" information
            string UnitKeyWord = "UNIT";
            //string UnitKeyWord = report.IsEnglishLocale ? "UNIT" : "EINHEIT";
            string? line = string.Empty;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(' ', '\t');
                string[] words = line.Split(' ');
                string cmd = words[0].ToUpper();
                if (cmd == "LOCALE")
                {
                    locale = words[1].ToLocaleType();
                }
                if (cmd == UnitKeyWord || cmd == "REGION")
                {
                    break;
                }
                prefixLines.Add(line);
            }
            return line ?? "";
        }
    }
}

