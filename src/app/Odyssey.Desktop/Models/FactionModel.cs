using Markdig.Syntax;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Odyssey.Models.Tools;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Odyssey.Models;

public class FactionModel
{
    public DataBlock Data { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public FactionRace Race { get; private set; }
    public UnitModel? RepresentativeUnit { get; private set; }

    public int Id { get; private set; }
    public bool IsActive { get; private set; } = false;
    public int Recruitment { get; private set; } = 0;
    public string MagicRealm { get; private set; }
    public string PeopleNumber { get; private set; }
    public string PeopleNumberIncludingHeroes { get; private set; }
    public string Heroes { get; private set; }
    public string MaxHeroes { get; private set; }

    public Dictionary<int, DataBlock> Alliances { get; private set; } = [];

    // Player specific data
    public string Email { get; private set; }
    public int ScorePoints { get; private set; }
    public int ScoreAverage { get; private set; }
    public int ScorePercent { get; private set; }
    public int Age { get; private set; }

    public FactionModel(DataBlock data)
    {
        Data = data;
        Recruitment = data.ValueInt(KeyType.RECRUITMENTCOST);
        Email = data.Value(KeyType.EMAIL);
        MagicRealm = data.Value(Strings.DE_FACTION_MAGIC_REALM);
        PeopleNumber = data.Value(Strings.DE_FACTION_PEOPLE_NUMBER);
        Heroes = data.Value(Strings.EN_FACTION_HEROES);
        MaxHeroes = data.Value(Strings.EN_FACTION_MAX_HEROES);

        // // TODO: move into FactionViewModel and localize
        string extendedName = "{PeopleNumber} people";
        if (!string.IsNullOrEmpty(Heroes) || !string.IsNullOrEmpty(MaxHeroes))
        {
            if (string.IsNullOrEmpty(Heroes)) Heroes = "0";
            extendedName += $", of it {Heroes} Heroes (max. {MaxHeroes})";
        }
        PeopleNumberIncludingHeroes = extendedName;

        ScorePoints = data.ValueInt(Strings.DE_FACTION_SCORE_POINTS);
        ScoreAverage = data.ValueInt(Strings.DE_FACTION_SCORE_AVERAGE);
        if (ScorePoints > 0 && ScoreAverage > 0)
        {
            float f_points = (float)ScorePoints;
            float f_average = (float)ScoreAverage;
            ScorePercent = (int)Math.Round(f_points * 100 / f_average);
            // TODO: move into FactionViewModel and localize
            //string score = $"Points: {points} ({percent}% from {average})";
        }

        Age = data.ValueInt(Strings.EN_FACTION_AGE); // age in turns number, not in years

        // TODO: handle pools
        // list faction pool
        //bool itemsinpool = false;
        //++block;
        /*
        if (block.GetType() == BlockType.ITEMS)
        {

            DataKey.list_type itemlist = block.GetData();

            for (datakey itor = itemlist.begin(); itor != itemlist.end(); itor++)
            {
                string label; label.format("%s %s", itor.Value().text(), itor.GetKeyFromType().text());

                MenuCommand* cmd = new MenuCommand(menu.factionpool, label);
                cmd.create();

                itemsinpool = true;
            }
        }
        */


        string name = data.Value(KeyType.FACTIONNAME);
        Name = string.IsNullOrEmpty(name) ? Labels.Localize(Labels.DISGUISED) : $"{name} ({data.IdToString()})";
        Race = FactionRaceParser.Parse(data.Value(KeyType.TYPE));

        IsActive = CRDocument.FactionIsActive(data);
        Id = data.GetId();
    }

    public bool TrySetRaceFromUnit(UnitModel? unitModel)
    {
        if (unitModel is null || string.IsNullOrWhiteSpace(unitModel.Race))
        {
            return false;
        }

        if (Race != FactionRace.Unknown)
        {
            return false;
        }

        FactionRace inferredRace = FactionRaceParser.Parse(unitModel.Race);
        if (inferredRace == FactionRace.Unknown)
        {
            return false;
        }

        Race = inferredRace;
        RepresentativeUnit ??= unitModel;
        return true;
    }

    public void AddAlliance(DataBlock ally, int blockId)
    {
        Alliances[blockId] = ally;
    }

    public override string ToString()
    {
        return Name;
    }
}
