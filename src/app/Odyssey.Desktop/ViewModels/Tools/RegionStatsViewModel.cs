using Avalonia.Controls;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Prism.Events;
using System.Collections.ObjectModel;
using static Odyssey.Models.Documents.CRDocument;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using static Odyssey.Models.Documents.SimpleItemSelection;
using Odyssey.Models.Localization;
using System.Diagnostics;
using static Odyssey.ViewModels.Tools.RegionStatsViewModel;
using System.Reflection.Emit;
using Odyssey.Models;

namespace Odyssey.ViewModels.Tools;

/// <summary>
/// Factions information on the currently selected region.
/// </summary>
public partial class RegionStatsViewModel : DocumentToolViewModelBase
{
    public class FactionItem
    {
        public FactionItem(string label, int id)
        {
            Label = label;
            this.Id = id;
        }
        public string Label { get; set; }
        public int Id { get; set; }
        public override string ToString()
        {
            return Label;
        }
    }

    protected class Entry
    {
        // (UnitID, Number) or (shipId, Number)
        private List<Tuple<int, int>> _items;

        public List<Tuple<int, int>> Items
        {
            get => _items;
        }

        public Entry()
        {
            _items = [];
        }
    }

    public class StatItem
    {
        public required string Label { get; set; }
        public int Kind { get; set; }
    }

    [ObservableProperty]
    private int _selectedFactionIndex;

    // Existing factions on the current region
    private ObservableCollection<FactionItem> _factions;

    private ObservableCollection<string> _domains;

    private int SelectedFactionId { get; set; }

    private readonly Collection<StatItem> _statItems;

    public Collection<StatItem> StatItems { get { return _statItems; } }

    public ObservableCollection<FactionItem> Factions => _factions ??= [];

    public ObservableCollection<string> Domains => _domains ??= [];
    public RegionStatsViewModel() : this(null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This constructor should only be used in design mode.");
        }
    }
    public RegionStatsViewModel(IEventAggregator? eventAggregator): base(eventAggregator)
    {
        SelectedFactionId = -1;
        _statItems = [];
        _factions = [];
        _domains = [];
    }

    partial void OnSelectedFactionIndexChanged(int oldValue, int newValue)
    {
        SelectedFactionId = newValue >= 0 ? Factions[newValue].Id : 0;
        CollectFactionData();
    }

    protected override void OnSelectionChanged(ISelectionChange selectionChange)
    {
        // Exclude this selector from selection change events to avoid reentrancy issues
        List<string> selectorIdsExcludeFilter = [Id];
        List<string> selectorIdsIncludeFilter = [Ids.Explorer];
        if (ShouldIgnoreSelectionChangedEvent(selectionChange, selectorIdsIncludeFilter, selectorIdsExcludeFilter))
        {
            return;
        }

        ISelection sel = selectionChange.Selection;
        Debug.WriteLine($"[VM-REG-STAT] SELSTATE CHANGED {sel} -> BEGIN");
        if (sel.IsRegionSelected() && !IsSelectedRegion(sel))
        {
            SetSelection(sel);
            if (FillFactionsList(Selection.Region!))
            {
                Debug.WriteLine("[VM-REGIONSTATS-] OnSelectionChanged - CollectFactions returned true");
            }
        }
        Debug.WriteLine($"[VM-REG-STAT] SELSTATE CHANGED {sel} -> ENDED");
    }

    private bool FillFactionsList(DataBlock region)
    {
        Debug.WriteLine($"[VM-REG-STAT] Fill faction lists -> BEGIN");

        SortedSet<int> factions = [];
        Factions.Clear();
        CRDocument cr = GetDocument();
        if (cr == null)
        {
            return false;
        }
        Debug.WriteLine($"[VM-REG-STAT] Fill faction lists -> handling...");
        bool bFoundCurrentlySelectedFaction = false;
        DataBlock? startBlock = region.GetNextBlock();
        int regionDepth = region.GetDepth();
        for (DataBlock? unit = startBlock; unit != null && unit.GetDepth() > regionDepth; unit = unit.GetNextBlock())
        {
            if (unit.GetBlockType() == BlockType.UNIT)
            {
                string fac = unit.Value(KeyType.FACTION);
                int factionId = -1;
                if (!string.IsNullOrEmpty(fac))
                {
                    factionId = int.Parse(fac);
                }
                if (factionId < 0 || !factions.TryGetValue(factionId, out _))
                {
                    factions.Add(factionId);
                    string label;
                    //DataBlock? faction = null;
                    FactionModel? faction = null;
                    if (factionId <= 0)
                    {
                        label = Labels.Localize(Labels.FACTION_DISGUISED);
                    }
                    else if (cr.GetFaction(ref faction, factionId))
                    {
                        label = faction!.Name;
                        /*
                        string name = faction!.Value(KeyType.FACTIONNAME);
                        if (string.IsNullOrEmpty(name))
                        {
                            label = Labels.Localize(Labels.FACTION_DISGUISED);
                        }
                        else
                        {
                            string id = faction.IdToString();
                            label = $"{name} ({id})";
                        }
                        */
                    }
                    else
                    {
                        // missing PARTEI block in report? how?
                        string id = Utils.Converters.IdToString(factionId);
                        label = Labels.Localize(Labels.UNKNOWN_ID, [id]);
                    }
                    FactionItem factionItem = new FactionItem(label, factionId);
                    int index;
                    if (cr.IsActiveFaction(factionId))
                    {
                        Factions.Insert(0, factionItem);
                        index = 0;
                    }
                    else
                    {
                        Factions.Add(factionItem);
                        index = Factions.Count - 1;
                    }
                    // select previously selected faction again
                    if (SelectedFactionId == factionId)
                    {
                        SelectedFactionIndex = index;
                        bFoundCurrentlySelectedFaction = true;
                    }
                }
            }
        }

        Factions.Add(new FactionItem(Labels.Localize(Labels.FACTION_ALL), -1));
        Debug.WriteLine($"[VM-REG-STAT] Fill faction lists -> ENDED");
        return bFoundCurrentlySelectedFaction;
    }

    private void CollectFactionData(/*int factionId*/)
    {
        // clear old list
        //List.clearItems();
        //Entries.Clear();

        // generate new list
        SortedDictionary<string, Entry> persons = [];
        SortedDictionary<string, Entry> items = [];
        SortedDictionary<string, Entry> ships = [];
        SortedDictionary<string, Entry> buildings = [];
        SortedDictionary<Tuple<string, int>, Entry> talents = [];

        // collect data
        if (Selection.IsRegionSelected())
        {
            CollectData(persons, items, talents, ships, buildings, Selection.Region);
        }

        // Add statistics
        bool includePersons = persons.Count > 0;
        bool includeItems = items.Count > 0;
        bool includeSkills = talents.Count > 0;
        bool includeShips = ships.Count > 0;
        bool includeBuildings = buildings.Count > 0;
        /*
        bool list_persons = (select.filter & select.FILTER_PERSONS) != 0 && persons.Count > 0;
        bool includeItems = (select.filter & select.FILTER_ITEMS) != 0 && items.Count > 0;
        bool includeSkills = (select.filter & select.FILTER_TALENTS) != 0 && talents.Count > 0;
        bool includeShips = (select.filter & select.FILTER_SHIPS) != 0 && ships.Count > 0;
        bool includeBuildings = (select.filter & select.FILTER_BUILDINGS) != 0 && buildings.Count > 0;
        */

        // add persons to list if not filtered out
        if (includePersons)
        {
            for (SortedDictionary<string, Entry>.Enumerator itor = persons.GetEnumerator(); itor.MoveNext();)
            {
                //Entry entry = itor.Current.Value;

                // calculate sum of units persons
                int sum = 0;
                foreach (Tuple<int, int> unit in itor.Current.Value.Items)
                {
                    // TODO
                    //sum += unit.;
                }
                string label = $"{itor.Current.Key}: {sum}";
                // 0: unit, 1: building, 2: ship
                StatItems.Add(new StatItem { Label = label, Kind = 0 });
            }

            if (includeItems || includeSkills || includeBuildings || includeShips)
            {
                StatItems.Add(new StatItem { Label = "--------------------------", Kind = -1 });
            }
        }

        // add items (and aura) to list
        if (includeItems)
        {
            for (SortedDictionary<string, Entry>.Enumerator itor = items.GetEnumerator(); itor.MoveNext();)
            {
                Entry en = itor.Current.Value;

                // calculate sum of units items
                int sum = 0;
                foreach (Tuple<int, int> unit in itor.Current.Value.Items)
                {
                    // TODO
                    //sum += unit.second;
                }
                string label = $"{itor.Current.Key}: {sum}";
                // 0: unit, 1: building, 2: ship
                StatItems.Add(new StatItem { Label = label, Kind = 0 });
                // save additional data
                //Entries[idx].CopyFrom(en);
            }

            if (includeSkills || includeBuildings || includeShips)
            {
                StatItems.Add(new StatItem { Label = "--------------------------", Kind = -1 });
            }
        }

        // add talents to list
        if (includeSkills)
        {
            for (SortedDictionary<Tuple<string, int>, Entry>.Enumerator itor = talents.GetEnumerator(); itor.MoveNext();)
            {
                Entry en = itor.Current.Value;

                // calculate sum of units items
                int sum = 0;
                foreach (Tuple<int, int> unit in itor.Current.Value.Items)
                {
                    // TODO
                    //sum += unit.second;
                }

                // TODO
                string label = "";
                //label.format("%s %d: %d", itor.Current.Key.first., itor.Current.Key.second, sum);
                // 0: unit, 1: building, 2: ship
                StatItems.Add(new StatItem { Label = label, Kind = 0 });
                // save additional data
                //Entries[idx].CopyFrom(en);
            }

            if (includeBuildings || includeShips)
            {
                StatItems.Add(new StatItem { Label = "--------------------------", Kind = -1 });
            }
        }

        // add castles and buildings
        if (includeBuildings)
        {
            for (SortedDictionary<string, Entry>.Enumerator itor = buildings.GetEnumerator(); itor.MoveNext();)
            {
                Entry en = itor.Current.Value;

                // calculate sum of units items
                int sum = 0;
                int min_ = -1;
                int max_ = -1;
                foreach (Tuple<int, int> building in itor.Current.Value.Items)
                {
                    sum++;
                    // TODO
                    /*
                    if (min_ == -1 || building.second < min_)
                    {
                        min_ = building.second;
                    }
                    if (building.second > max_)
                    {
                        max_ = building.second;
                    }
                    */
                }

                string label = "";
                // TODO
                /*
                if (min_ == max_)
                {
                    label.format("%s: %d (%d)", itor.Current.Key, sum, min_);
                }
                else
                {
                    label.format("%s: %d (%d-%d)", itor.Current.Key, sum, min_, max_);
                }
                */
                // 0: unit, 1: building, 2: ship
                StatItems.Add(new StatItem { Label = label, Kind = 1 });
                // save additional data
                //Entries[idx].CopyFrom(en);
            }

            if (includeShips)
            {
                StatItems.Add(new StatItem { Label = "--------------------------", Kind = -1 });
            }
        }

        // add castles and buildings
        if (includeShips)
        {
            for (SortedDictionary<string, Entry>.Enumerator itor = ships.GetEnumerator(); itor.MoveNext();)
            {
                Entry en = itor.Current.Value;
                // TODO
                string label = "";
                //label.format("%s: %zd", itor.Current.Key, itor.Current.Value.Items.size());
                // 0: unit, 1: building, 2: ship
                StatItems.Add(new StatItem { Label = label, Kind = 2 });
                // save additional data
                //Entries[idx].CopyFrom(en);
            }
        }
    }

    protected void CollectData(SortedDictionary<string, Entry> persons, SortedDictionary<string, Entry> items, SortedDictionary<Tuple<string, int>, Entry> talents, SortedDictionary<string, Entry> ships, SortedDictionary<string, Entry> buildings, in DataBlock? region)
    {
        bool unitInFaction = false;
        int unitId = 0;
        int personsInUnit = 0;
        CRDocument cr = GetDocument();
        if (cr == null)
        {
            return;
        }

        // TODO: each faction has a name and an id (int)
        /*
        int item = SelectedFaction;
        FXival selectedFactionId = (FXival)factionBox.getItemData(new int(item));
        */

        DataBlock? firstBlockNode = region?.GetNextBlock();
        int depth = region!.GetDepth();

        // FIXME: debug this part
        // TMP : 
        return;

        for (DataBlock? block = firstBlockNode; block != null && block.GetDepth() > depth; block = block.GetNextBlock())
        {
            if (block.GetBlockType() == BlockType.SHIP)
            {
                string fac = block.Value(KeyType.FACTION);
                int faction = -1;
                if (!string.IsNullOrEmpty(fac))
                {
                    faction = int.Parse(fac);
                }

                if (faction == SelectedFactionId || SelectedFactionId == 0)
                {
                    int size = int.Parse(block.Value("Groesse"));

                    // <Shiptype>: <Size>
                    string type = block.Value(KeyType.TYPE);
                    if (!string.IsNullOrEmpty(type))
                    {
                        //  FIXME: Exception  here
                        ships[type].Items.Add(Tuple.Create(block.GetId(), size));
                    }
                }
            }
            else if (block.GetBlockType() == BlockType.BUILDING)
            {
                string fac = block.Value(KeyType.FACTION);
                int faction = -1;
                if (!string.IsNullOrEmpty(fac))
                {
                    faction = int.Parse(fac);
                }

                if (faction == SelectedFactionId || SelectedFactionId == 0)
                {
                    int size = int.Parse(block.Value("Groesse"));

                    // <Buildingtype>: <Size>
                    string type = block.Value(KeyType.TYPE);
                    if (!string.IsNullOrEmpty(type))
                    {
                        buildings[type].Items.Add(Tuple.Create(block.GetId(), size));
                    }
                }
            }
            else if (block.GetBlockType() == BlockType.UNIT)
            {
                unitId = block.GetId();
                string fac = block.Value(KeyType.FACTION);
                int faction = -1;
                if (!string.IsNullOrEmpty(fac))
                {
                    faction = int.Parse(fac);
                }

                if (faction == SelectedFactionId || SelectedFactionId == 0)
                {
                    unitInFaction = true;
                }
                else
                {
                    unitInFaction = false;
                }
            }

            if (!unitInFaction)
            {
                continue;
            }

            // collect statistics from this unit block
            if (block.GetBlockType() == BlockType.UNIT)
            {
                string val = string.Empty;
                string type = block.Value(KeyType.TYPE);

                // <Race>: #
                val = block.Value(KeyType.NUMBER);
                if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(val))
                {
                    personsInUnit = int.Parse(val);
                    if (!persons.TryGetValue(type, out Entry entry))
                    {
                        entry = new Entry();
                        persons[type] = entry;
                    }
                    // Now you can add data to entry.Items, e.g.:
                    entry.Items.Add(Tuple.Create(unitId, personsInUnit));
                    // Exception when type is "Orks"
                    //persons[type].Items.Add(Tuple.Create(unitId, personsInUnit));
                }
                else
                {
                    personsInUnit = 0;
                }

                val = block.Value(KeyType.HERO);
                if (!string.IsNullOrEmpty(val))
                {
                    // heroes
                    persons["Helden"].Items.Add(Tuple.Create(unitId, personsInUnit));
                }

                // Aura: #
                val = block.Value(KeyType.AURA);
                if (!string.IsNullOrEmpty(val))
                {
                    items["Aura"].Items.Add(Tuple.Create(unitId, int.Parse(val)));
                }
            }

            // collect statistics from this items block
            if (block.GetBlockType() == BlockType.ITEMS)
            {
                foreach (DataKey kb in block.GetData())
                {
                    if (!string.IsNullOrEmpty(kb.GetValue()))
                    {
                        items[kb.GetKey()].Items.Add(Tuple.Create(unitId, int.Parse(kb.GetValue())));
                    }
                }
            }

            // collect statistics from this skills block
            if (block.GetBlockType() == BlockType.TALENTS)
            {
                //stats_talents, numberInUnit
                // <Talent> <Level>: #
                foreach (DataKey kb in block.GetData())
                {
                    if (!string.IsNullOrEmpty(kb.GetValue()))
                    {
                        // TODO
                        string talent = kb.GetValue();//.after(' ');
                        int level = int.Parse(talent);
                        talents[Tuple.Create(kb.GetKey(), level)].Items.Add(Tuple.Create(unitId, personsInUnit));
                    }
                }
            }
        }
    }

}
