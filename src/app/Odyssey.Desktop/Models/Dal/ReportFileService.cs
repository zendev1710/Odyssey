using Avalonia.Controls.Shapes;
using Avalonia.Svg.Commands;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Odyssey.Models.Dal
{
    public static class ReportFileService
    {
        private static bool IsIdenticalToMagellanContent = true;

        /// <summary>
        /// Loads a report file from the given pathname.
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="report"></param>
        /// <param name="outError"></param>
        /// <returns></returns>
        public static bool LoadFile(string pathname, out EresseaDocument report, out string outError)
        {
            bool loaded; 
            outError = "";
            string reportName = System.IO.Path.GetFileNameWithoutExtension(pathname);
            using (var fileStream = new FileStream(pathname, FileMode.Open, FileAccess.Read))
            {
                loaded = LoadStream(reportName, fileStream, out report, out outError);
            }
            return loaded;
        }

        /// <summary>
        /// Loads a report from the specified stream and parses its content into an <see cref="EresseaDocument"/>.
        /// </summary>
        /// <remarks>This method reads and parses the content of the provided stream to extract data
        /// blocks and construct an <see cref="EresseaDocument"/>. The stream must contain data in a supported format.
        /// If the format is invalid or the content cannot be read, the method returns <see langword="false"/>  and
        /// provides an error message in <paramref name="outError"/>.</remarks>
        /// <param name="reportName">The name of the report to associate with the parsed document.</param>
        /// <param name="stream">The input stream containing the report data to be parsed. Must be readable and positioned at the beginning
        /// of the content.</param>
        /// <param name="report">When this method returns, contains the parsed <see cref="EresseaDocument"/> if the operation succeeds;
        /// otherwise, <see langword="null"/>.</param>
        /// <param name="outError">When this method returns, contains an error message if the operation fails; otherwise, an empty string.</param>
        /// <returns><see langword="true"/> if the report was successfully loaded and parsed; otherwise, <see langword="false"/>.</returns>
        public static bool LoadStream(string reportName, Stream stream, out EresseaDocument report, out string outError)
        {
            bool result = true;
            outError = "";
            LinkedList<DataBlock> blocks = [];
            using (var reader = new StreamReader(stream))
            {
                DataBlock? currentBlock = null;
                //DataBlock? newblock = new();
                var utf8 = true;

                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.EndsWith('\r'))
                    {
                        line = line[..^1];
                    }

                    var str = line;

                    // first try: does the line contain a DataKey?
                    var key = new DataKey();
                    if (key.Parse(str, currentBlock is not null ? currentBlock.GetBlockType() : BlockType.UNKNOWN, utf8))
                    {
                        if (currentBlock is not null)
                        {
                            switch (key.GetKeyType())
                            {
                                case KeyType.CHARSET:
                                    if (utf8 && key.GetValue() != "UTF-8")
                                    {
                                        utf8 = false;
                                    }
                                    break;
                                case KeyType.TERRAIN:
                                    int terrain = DataBlock.ParseTerrain(key.GetValue());
                                    if (terrain == Terrains.UNKNOWN)
                                    {
                                        currentBlock.AddKey(key);
                                        // need textual representation of terrain type
                                        terrain = DataBlock.ParseSpecialTerrain(key.GetValue());
                                    }
                                    currentBlock.SetTerrain(terrain);
                                    break;
                                default:
                                    currentBlock.AddKey(key); // appends "Value";Key - tags
                                    break;
                            }
                        }
                    }
                    // second attempt: does the line contain a data block header?
                    else if (ParseBlocHeader(str, out DataBlock? parsedBlock))
                    {
                        // appends BLOCK Info - tags
                        parsedBlock!.SetNode(blocks.AddLast(parsedBlock));
                        currentBlock = parsedBlock;
                    }
                    else
                    {
                        // fix for a bug introduced by version 1.2.7
                        var semi = str.IndexOf(';');
                        if (semi >= 0)
                        {
                            currentBlock!.AddKey(new DataKey(str[(semi + 1)..], str[..semi]));
                        }
                    }
                }

                if (blocks.Count == 0)
                {
                    outError = "The content could not be read.\nThe format may not be supported.";
                    result = false;
                }
                else if (blocks.First?.Value.GetBlockType() != BlockType.VERSION)
                {
                    outError = "The content is in the wrong format.";
                    blocks.Clear();
                    result = false;
                }
            }
            Debug.WriteLine($"Blocks number : {blocks.Count} ");
            report = new CRDocument(reportName, blocks);
            return result;
        }

        /// <summary>
        /// Saves the specified document to a file at the given path, applying the provided map filter and selection
        /// criteria.
        /// </summary>
        /// <remarks>This method writes the content of the provided document to a file, applying the
        /// specified map filter and optionally limiting the output to the blocks included in the <paramref
        /// name="selection"/> set. The method ensures that only relevant data is written based on the filter and
        /// selection criteria. If the file cannot be written due to an error, the method logs the exception and returns
        /// the count of processed blocks.</remarks>
        /// <param name="pathname">The file path where the document will be saved. Cannot be null or empty.</param>
        /// <param name="cr">The document to be saved, represented as a <see cref="CRDocument"/> object.</param>
        /// <param name="mapFilter">The filter specifying which types of data blocks to include in the saved file.</param>
        /// <param name="selection">A set of <see cref="DataBlock"/> objects to include in the saved file. If null or empty, all applicable
        /// blocks are included based on the filter.</param>
        /// <returns>true if saving was successfull; otherwise false</returns>
        public static bool Save(string pathname, CRDocument cr, MapType mapFilter, HashSet<DataBlock> selection)
        {
            if (string.IsNullOrEmpty(pathname))
            {
                return false;
            }
            LinkedListNode<DataBlock>? firstBlockNode = cr.FirstBlock;
            int maxDepth = 0;
            bool inTranslationBlock = false;
            string terrainName = string.Empty;
            try
            {
                // open text file for writing
                using StreamWriter writer = new(pathname, false);
                for (var node = firstBlockNode; node != null; node = node.Next)
                {
                    var block = node.Value;
                    bool hideKeys = false;
                    BlockType blockType = block.GetBlockType();
                    if (block.GetId() < 0)
                    {
                        // GAME -1 prevent
                        continue;
                    }
                    if (maxDepth > 0)
                    {
                        if (block.GetDepth() <= maxDepth)
                        {
                            // reset the skip-child behavior
                            maxDepth = 0;
                        }
                        else
                        {
                            // do not print child-blocks now
                            continue;
                        }
                    }

                    if (mapFilter == MapType.MINIMAL)
                    {
                        // skip over anything except version or region
                        if (blockType == BlockType.REGION)
                        {
                            if (selection != null && selection.Count > 0)
                            {
                                DataBlock region = block;
                                if (!selection.Contains(region))
                                {
                                    // skip region if not in selection
                                    continue;
                                }
                            }
                        }
                        else if (blockType != BlockType.VERSION)
                        {
                            continue;
                        }
                    }
                    else if (mapFilter != MapType.FULL)
                    {
                        // do not include these blocks at all
                        if (blockType == BlockType.EFFECTS
                            || blockType == BlockType.MESSAGE
                            || blockType == BlockType.MESSAGETYPE
                            || blockType == BlockType.DURCHREISE
                            || blockType == BlockType.DURCHSCHIFFUNG)
                        {
                            continue;
                        }

                        // skip these blocks with all their children
                        if (blockType == BlockType.UNIT
                            || blockType == BlockType.BATTLE
                            || blockType == BlockType.SHIP)
                        {
                            maxDepth = block.GetDepth();
                            continue;
                        }
                    }

                    // Output block names + ID numbers
                    writer.Write(block.GetTypeLabel());

                    if (blockType == BlockType.REGION || blockType == BlockType.BATTLE)
                    {
                        ILocation? location = block as ILocation;
                        if (location is not null)
                        {
                            // TODO: what about if X and Y are 0 ? what &bout Plane ?
                            // if location.X != 0 && location.Y != 0)
                            writer.Write($" {location.X} {location.Y}");
                        }
                    }

                    if (blockType == BlockType.TRANSLATION)
                    {
                        inTranslationBlock = true;
                    }

                    if (blockType == BlockType.COMBATSPELL || blockType == BlockType.ISLAND || blockType == BlockType.FACTION || block.GetId() > 0)
                    {
                        writer.Write($" {block.GetId()}");
                    }

                    writer.Write(writer.NewLine);

                    if (blockType == BlockType.VERSION)
                    {
                        writer.WriteLine("\"UTF-8\";charset");
                    }

                    if (blockType == BlockType.REGION)
                    {
                        // the same info is written after with Data traversing => two lines in file)
                        terrainName = block.GetTerrainName();
                    }
                    else if (blockType == BlockType.VERSION)
                    {
                        // LATER: This KONFIGURATION block seems to be unused.
                        // It 's only here to say that the saved file is coming from this program.
                        // Adjust configuration block and set charset to ISO-8859-1
                        block.SetKey(KeyType.KONFIGURATION, GetConfigurationName(mapFilter));
                    }
                    else if (blockType == BlockType.UNIT)
                    {
                        if (!IsIdenticalToMagellanContent)
                        {
                            // CsMap specific property
                            if (cr.IsConfirmed(block))
                            {
                                writer.WriteLine("1;ejcOrdersConfirmed");
                            }
                        }

                    }
                    else if (blockType == BlockType.COMMANDS)
                    {
                        // commands are stored in attachment for the active faction(s)
                        OrdersAttachment? cmds = block.GetAttachment() as OrdersAttachment;
                        if (cmds?.Commands.Count > 0)
                        {
                            // hide original commands as they are present here in the attachment data
                            hideKeys = true;
                            foreach (var cmd in cmds.Commands)
                            {
                                writer.Write('"');
                                string value = cmd;
                                // \\ is a \, \" is a ", single " are the beginning and end 
                                // of strings, \n is a line break, (\x is x if no special rule)
                                for (int i = 0; i < value.Length; i++)
                                {
                                    char c = value[i];
                                    if (c == '\\' || c == '\"')
                                    {
                                        writer.Write('\\');
                                    }
                                    if (c == '\n')
                                    {
                                        writer.Write("\\n");
                                    }
                                    else
                                    {
                                        writer.Write(c);
                                    }
                                }
                                writer.WriteLine('"');
                            }
                        }
                    }

                    // Issue data keys - Save keys of this block only if desired
                    if (!hideKeys)
                    {
                        // tags are DataKeys
                        foreach (var data in block.GetData())
                        {
                            KeyType keyType = data.GetKeyType();
                            if (blockType == BlockType.FACTION)
                            {
                                if (mapFilter != MapType.FULL)
                                {
                                    if (keyType != KeyType.BANNER
                                        && keyType != KeyType.LOCALE
                                        && keyType != KeyType.FACTIONNAME
                                        && keyType != KeyType.EMAIL)
                                    {
                                        continue;
                                    }
                                }
                            }
                            else if (blockType == BlockType.REGION)
                            {
                                if (mapFilter != MapType.FULL)
                                {
                                    if (keyType == KeyType.VISIBILITY)
                                    {
                                        continue;
                                    }
                                    if (mapFilter == MapType.MINIMAL)
                                    {
                                        if (keyType != KeyType.NAME && keyType != KeyType.ISLAND && keyType != KeyType.ID)
                                        {
                                            continue;
                                        }
                                    }
                                }

                                // To have the exact same content as Magellan CR, data properties are sorted like the following:
                                // - Id (not defined if terrain in unknown )
                                // - Name : undefined for some region terrains (e.g. oceans and firewalls have no name)
                                // - Terrain
                                if (blockType == BlockType.REGION && !string.IsNullOrEmpty(terrainName))
                                {
                                    // Add terrain information if not defined as a data key value
                                    if (keyType != KeyType.NAME && keyType != KeyType.ID && keyType != KeyType.TERRAIN)
                                    {
                                        writer.WriteLine($"\"{terrainName}\";Terrain");
                                        terrainName = string.Empty;
                                    }
                                }

                            }
                            else if (blockType == BlockType.UNIT)
                            {
                                if (keyType == KeyType.ORDERS_CONFIRMED)
                                {
                                    continue;       // will be set above
                                }
                            }

                            string value = data.GetValue();
                            if (data.IsInt())
                            {
                                // When it's not an integer (though data key is considered as it), value is surrounded by double quotes
                                writer.Write(StringUtils.IsFirstWordNumeric(value) ? value : $"\"{value}\"");
                            }
                            else
                            {
                                // value is surrounded by double quotes 
                                writer.Write('"');

                                // \\ is a \, \" is a ", single " are the beginning and end 
                                // of strings, \n is a line break, (\x is x if no special rule)
                                for (int i = 0; i < value.Length; i++)
                                {
                                    char c = value[i];
                                    if (c == '\\' || c == '\"')
                                    {
                                        writer.Write('\\');
                                    }
                                    if (c == '\n')
                                    {
                                        writer.Write("\\n");
                                    }
                                    else
                                    {
                                        writer.Write(c);
                                    }
                                }
                                writer.Write('"');
                            }

                            string key = data.GetKeyFromType();

                            if (!string.IsNullOrEmpty(key))
                            {
                                if (inTranslationBlock)
                                {
                                    // In TRANSLATION section, some values (keys) have to be with German characters (not with digraphs)
                                    LanguageUtils.ConvertInGermans(ref key);
                                }
                                writer.Write($";{key}");
                            }
                            writer.Write(writer.NewLine);

                            if (keyType == KeyType.TERRAIN)
                            {
                                // If terrain is defined as a data key,like for a "unbekannt" (unknown) terrain, it is not written again
                                terrainName = string.Empty;
                            }
                        }

                        // Write terrain information if not already done
                        if (blockType == BlockType.REGION && !string.IsNullOrEmpty(terrainName))
                        {
                            writer.WriteLine($"\"{terrainName}\";Terrain");
                            terrainName = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VM-MAIN-WIN] !!! An error occurred: {ex.Message}");
                return false; 
            }
            return true;
        }

        // Try to parse str as a datablock header
        private static bool ParseBlocHeader(string brutLine, out DataBlock? parsedBlock)
        {
            parsedBlock = null;
            if (string.IsNullOrEmpty(brutLine))
            {
                return false;
            }

            // Skip potential indentation
            string line = brutLine.TrimStart();

            // must start with an uppercase letter
            if (string.IsNullOrEmpty(line) || !char.IsLetter(line[0]) || !char.IsUpper(line[0]))
            {
                return false;
            }

            // block headers cannot contain '\"' or ';'
            if (line.Contains('\"') || line.Contains(';'))
            {
                return false;
            }

            if (string.IsNullOrEmpty(line))
            {
                return false;
            }

            int spaceIndex = line.IndexOf(' ');

            if (spaceIndex != -1)
            {
                ReadOnlySpan<char> span = line.AsSpan();
                string word = span[..spaceIndex].ToString();
                string id = span[(spaceIndex + 1)..].TrimStart().ToString();

                // region first checked first for performance (moved to top)
                if (word == "REGION")
                {
                    parsedBlock = new RegionDataBlock(id);
                }
                else
                {
                    BlockType bt = BlockType.UNKNOWN;
                    // if BLOCKNAMES is defined as static, null exception here
                    foreach (var b in DataBlock.BLOCKNAMES)
                    {
                        if (b.Value == word)
                        {
                            bt = b.Key;
                            break;
                        }
                    }

                    if (bt == BlockType.UNIT)
                    {
                        parsedBlock = new UnitDataBlock(id);
                    }
                    else if (bt == BlockType.BUILDING)
                    {
                        parsedBlock = new BuildingDataBlock(id);
                    }

                    else if (bt == BlockType.SHIP)
                    {
                        parsedBlock = new ShipDataBlock(id);
                    }

                    else if (bt == BlockType.BATTLE)
                    {
                        parsedBlock = new BattleDataBlock(id);
                    }
                    else if (bt == BlockType.ISLAND)
                    {
                        parsedBlock = new IslandDataBlock(id);
                    }
                    else
                    {
                        // unknown block type, but still create a DataBlock with the given word as type label
                        parsedBlock = new DataBlock(bt, id, bt == BlockType.UNKNOWN ? word : string.Empty);
                    }
                    //SetAttachment(null);
                }
            }
            else
            {
                BlockType bt = BlockType.UNKNOWN;
                // if BLOCKNAMES is defined as static, null exception here
                foreach (var b in DataBlock.BLOCKNAMES)
                {
                    if (b.Value == line)
                    {
                        bt = b.Key;
                        break;
                    }
                }
                parsedBlock = new DataBlock(bt, string.Empty, line);
                // unset flags
                //SetFlags((int)Flag.None);
                //SetTypeLabel(str);
                //SetId(string.Empty);

                // unset all other states
                //SetTerrain(Terrains.UNKNOWN);
                //SetAttachment(null);
                return true;
            }

            // unset all other states
            //SetTerrain(Terrains.UNKNOWN);
            ////SetAttachment(null);

            return true;
        }

        private static string GetConfigurationName(MapType type)
        {
            if (type == MapType.NORMAL)
            {
                return "CSMapFX:Export";
            }
            if (type == MapType.MINIMAL)
            {
                return "CSMapFX:Map";
            }
            return "CSMapFX";
        }
    }
}

