using System;


namespace Odyssey.Models.Data
{
    /// <summary>
    /// Contains all the english and german string values coming from reports.
    /// </summary>
    public static class Strings
    {
        ///////////////////
        // Names as entries key (and some entries value) coming from CR

        public const string CR_VERSION_LOCALE = "locale";

        // German language
        public const string DE_HORSE = "Pferd";
        public const string DE_ELFEN_HORSE = "Elfenpferd";
        public const string DE_CART = "Wagen";
        public const string DE_CATAPULT = "Katapult";

        public const string DE_BORDER_TYPE = "typ";
        public const string DE_FACTION_PEOPLE_NUMBER = "Anzahl Personen";
        public const string DE_GAME = "Spiel";
        public const string DE_GOBLINS = "Goblins";
        public const string DE_GUARDING = "bewacht";
        public const string DE_HORSES = "Pferde";

        public const string DE_MONSTER_TYPE_VALUE_SKELETTE = "Skelette";
        public const string DE_MONSTER_TYPE_VALUE_SKELETTHERREN = "Skelettherren";
        public const string DE_MONSTER_TYPE_VALUE_ZOMBIES = "Zombies";
        public const string DE_MONSTER_TYPE_VALUE_JUJU_ZOMBIES = "Juju-Zombies";
        public const string DE_MONSTER_TYPE_VALUE_GHOULE = "Ghoule";
        public const string DE_MONSTER_TYPE_VALUE_GHASTE = "Ghaste";
        public const string DE_MONSTER_TYPE_VALUE_ENTS = "Ents";
        public const string DE_MONSTER_TYPE_VALUE_BAUERN = "Bauern";
        public const string DE_MONSTER_TYPE_VALUE_HIRNTOTER = "Hirntöter";

        public const string DE_OWNER = "Besitzer";
        public const string DE_REGION_BORDER_TYPE_VALUE_ROAD = "Straße";
        public const string DE_REGION_BORDER_TYPE_VALUE_ROAD_DIGRAPH = "Strasse";
        public const string DE_REGION_BUILDING_VALUE_WORMHOLE = "Wurmloch";
        public const string DE_REGION_ROAD_DIRECTION = "richtung";
        public const string DE_RIDING = "Reiten";
        public const string DE_ROAD_PERCENT = "prozent";
        public const string DE_TROLLS = "Trolle";
        public const string DE_TRUE_RACE_TYPE = "wahrerTyp";

        public const string DE_UNIT_FACTION_ID = "Partei";
        public const string DE_UNIT_IS_GUARDING = DE_GUARDING;
        public const string DE_UNIT_TYPE_NAME_VALUE_SEA_SNAKE = "Seeschlange";
        public const string DE_UNIT_TYPE_NAME_VALUE_DRAGON = "Drachen";
        public const string DE_UNIT_TYPE_NAME_VALUE_DRAGON_WYRM = "Wyrme";
        public const string DE_UNIT_TYPE_NAME_VALUE_YOUNG_DRAGON = "Jungdrachen";

        // German skills names
        public const string DE_SKILL_ALCHEMY = "Alchemie";
        public const string DE_SKILL_ARMOURSMITHING = "Rüstungsbau";
        public const string DE_SKILL_BOW = "Bogenschießen";
        public const string DE_SKILL_CARTMAKING = "Wagenbau";
        public const string DE_SKILL_CATAPULT = "Katapultbedienung";
        public const string DE_SKILL_CROSSBOW = "Armbrustschießen";
        public const string DE_SKILL_ENDURANCE = "Ausdauer";
        public const string DE_SKILL_ENTERTAINMENT = "Unterhaltung";
        public const string DE_SKILL_ESPIONAGE = "Spionage";
        public const string DE_SKILL_FORESTRY = "Holzfällen";
        public const string DE_SKILL_HERBALISM = "Kräuterkunde";
        public const string DE_SKILL_MAGIC = "Magie";
        public const string DE_SKILL_MASONRY = "Burgenbau";
        public const string DE_SKILL_MELEE = "Hiebwaffen";
        public const string DE_SKILL_MINING = "Bergbau";
        public const string DE_SKILL_PERCEPTION = "Wahrnehmung";
        public const string DE_SKILL_POLEARM = "Stangenwaffen";
        public const string DE_SKILL_QUARRYING = "Steinbau";
        public const string DE_SKILL_RIDING = "Reiten";
        public const string DE_SKILL_ROADWORK = "Straßenbau";
        public const string DE_SKILL_SAILING = "Segeln";
        public const string DE_SKILL_SHIPCRAFT = "Schiffbau";
        public const string DE_SKILL_STEALTH = "Tarnung";
        public const string DE_SKILL_TACTICS = "Taktik";
        public const string DE_SKILL_TAMING = "Pferdedressur";
        public const string DE_SKILL_TAXATION = "Steuereintreiben";
        public const string DE_SKILL_TRADE = "Handeln";
        public const string DE_SKILL_UNARMED_COMBAT= "Waffenloser Kampf";
        public const string DE_SKILL_WEAPONSMITHING= "Waffenbau";

        // German orders (commands)
        public const string DE_CMD_ATTACK = "ATTACKIERE";
        public const string DE_CMD_BUY = "KAUFE";
        public const string DE_CMD_CARRY = "TRANSPORTIERE";
        public const string DE_CMD_CAST = "ZAUBERE";
        public const string DE_CMD_CLAIM = "BEANSPRUCHE";
        public const string DE_CMD_COMBAT = "KÄMPFE";
        public const string DE_CMD_COMBATSPELL = "KAMPFZAUBER";
        public const string DE_CMD_CONTACT = "KONTAKTIERE";
        public const string DE_CMD_DEFAULT = "DEFAULT";
        public const string DE_CMD_DESCRIBE = "BESCHREIBE";
        public const string DE_CMD_DESTROY = "ZERSTÖRE";
        public const string DE_CMD_EMAIL = "EMAIL";
        public const string DE_CMD_ENTER = "BETRETE";
        public const string DE_CMD_ENTERTAIN = "UNTERHALTE";
        public const string DE_CMD_FOLLOW = "FOLGE";
        public const string DE_CMD_FORGET = "VERGISS";
        public const string DE_CMD_GIVE = "GIB";
        public const string DE_CMD_GROUP = "GRUPPE";
        public const string DE_CMD_GROW = "ZÜCHTE";
        public const string DE_CMD_GUARD = "BEWACHE";
        public const string DE_CMD_HELP = "HELFE";
        public const string DE_CMD_HIDE = "TARNE";
        public const string DE_CMD_LANGUAGE = "SPRACHE";
        public const string DE_CMD_LEARN_AUTO = "LERNE AUTO";
        public const string DE_CMD_LEARN = "LERNE";
        public const string DE_CMD_LEAVE = "VERLASSE";
        public const string DE_CMD_MAKE = "MACHE";
        public const string DE_CMD_MESSAGE = "BOTSCHAFT";
        public const string DE_CMD_MOVE = "NACH";
        public const string DE_CMD_NUMBER = "NUMMER";
        public const string DE_CMD_OPTION = "OPTION";
        public const string DE_CMD_ORIGIN = "URSPRUNG";
        public const string DE_CMD_PASSWORD = "PASSWORT";
        public const string DE_CMD_PAY_NOT = "BEZAHLE NICHT";
        public const string DE_CMD_PIRACY = "PIRATERIE";
        public const string DE_CMD_PLANT = "PFLANZE";
        public const string DE_CMD_PREFIX = "PRÄFIX";
        public const string DE_CMD_PROMOTION = "BEFÖRDERE";
        public const string DE_CMD_RECRUIT = "REKRUTIERE";
        public const string DE_CMD_RESEARCH = "FORSCHE";
        public const string DE_CMD_RESERVE = "RESERVIERE";
        public const string DE_CMD_RIDING = "FAHRE";
        public const string DE_CMD_ROUTE = "ROUTE";
        public const string DE_CMD_SELL = "VERKAUFE";
        public const string DE_CMD_SHOW = "ZEIGE";
        public const string DE_CMD_SORT = "SORTIERE";
        public const string DE_CMD_SPY = "SPIONIERE";
        public const string DE_CMD_STEAL = "BEKLAUE";
        public const string DE_CMD_TAX = "TREIBE";
        public const string DE_CMD_TEACH = "LEHRE";
        public const string DE_CMD_USE = "BENUTZE";
        public const string DE_CMD_WORK = "ARBEITE";

        // English language
        public const string EN_ALLIANCE_STATUS = "Status";
        public const string EN_MESSAGE_REGION = "region";
        public const string EN_MESSAGE_RENDERED = "rendered";
        public const string EN_MESSAGE_SHIP = "ship";
        public const string EN_MESSAGE_FROM = "from";
        public const string EN_MESSAGE_TO = "to";
        public const string EN_MESSAGE_PASSWORD = "value";
        public const string EN_MESSAGE_SECTION = "section";
        public const string EN_MESSAGE_UNIT = "unit";
        public const string EN_REGION_OWNER = "owner";
        public const string EN_REGION_VISIBILITY_VALUE_NEIGHBOUR = "neighbour";
        public const string EN_REGION_VISIBILITY_VALUE_LIGHTHOUSE = "lighthouse";
        public const string EN_REGION_VISIBILITY_VALUE_TRAVEL = "travel";
        public const string EN_UNIT_TRAITOR = "Traitor";
        public const string EN_VERSION_BUILD = "Build";
        public const string EN_VERSION_TURN = "Turn";

        // English skills names
        public const string EN_SKILL_ALCHEMY = "Alchemy";
        public const string EN_SKILL_ARMOURSMITHING = "Armoursmithing";
        public const string EN_SKILL_BOW = "Bow";
        public const string EN_SKILL_CARTMAKING = "Cartmaking";
        public const string EN_SKILL_CATAPULT = "Catapult";
        public const string EN_SKILL_CROSSBOW = "Crossbow";
        public const string EN_SKILL_ENDURANCE = "Endurance";
        public const string EN_SKILL_ENTERTAINMENT = "Entertainment";
        public const string EN_SKILL_ESPIONAGE = "Espionage";
        public const string EN_SKILL_FORESTRY = "Forestry";
        public const string EN_SKILL_HERBALISM = "Herbalism";
        public const string EN_SKILL_MAGIC = "Magic";
        public const string EN_SKILL_MASONRY = "Masonry";
        public const string EN_SKILL_MELEE = "Melee";
        public const string EN_SKILL_MINING = "Mining";
        public const string EN_SKILL_PERCEPTION = "Perception";
        public const string EN_SKILL_POLEARM = "Polearm";
        public const string EN_SKILL_QUARRYING = "Quarrying";
        public const string EN_SKILL_RIDING = "Riding";
        public const string EN_SKILL_ROADWORK = "Roadwork";
        public const string EN_SKILL_SAILING = "Sailing";
        public const string EN_SKILL_SHIPCRAFT = "Shipcraft";
        public const string EN_SKILL_STEALTH = "Stealth";
        public const string EN_SKILL_TACTICS = "Tactics";
        public const string EN_SKILL_TAMING = "Taming";
        public const string EN_SKILL_TAXATION = "Taxation";
        public const string EN_SKILL_TRADE = "Trade";
        public const string EN_SKILL_UNARMED_COMBAT = "Unarmed combat";
        public const string EN_SKILL_WEAPONSMITHING = "Weaponsmithing";

        // English orders (commands)
        public const string EN_CMD_ATTACK = "ATTACK";
        public const string EN_CMD_BANNER = "BANNER";
        public const string EN_CMD_BUY = "BUY";
        public const string EN_CMD_CARRY = "CARRY";
        public const string EN_CMD_CAST = "CAST";
        public const string EN_CMD_CLAIM = "CLAIM";
        public const string EN_CMD_COMBAT = "COMBAT";
        public const string EN_CMD_COMBATSPELL = "COMBATSPELL";
        public const string EN_CMD_CONTACT = "CONTACT";
        public const string EN_CMD_DEFAULT = "DEFAULT";
        public const string EN_CMD_DESCRIBE = "DESCRIBE";
        public const string EN_CMD_DESTROY = "DESTROY";
        public const string EN_CMD_EMAIL = "EMAIL";
        public const string EN_CMD_ENTER = "ENTER";
        public const string EN_CMD_ENTERTAIN = "ENTERTAIN";
        public const string EN_CMD_FOLLOW = "FOLLOW";
        public const string EN_CMD_FORGET = "FORGET";
        public const string EN_CMD_GIVE = "GIVE";
        public const string EN_CMD_GROUP = "GROUP";
        public const string EN_CMD_GROW = "GROW";
        public const string EN_CMD_GUARD = "GUARD";
        public const string EN_CMD_HELP = "HELP";
        public const string EN_CMD_HIDE = "HIDE";
        public const string EN_CMD_LANGUAGE = "LANGUAGE";
        public const string EN_CMD_LEARN_AUTO = "LEARN AUTO";
        public const string EN_CMD_LEARN = "LEARN";
        public const string EN_CMD_LEAVE = "LEAVE";
        public const string EN_CMD_MAKE = "MAKE";
        public const string EN_CMD_MESSAGE = "MESSAGE";
        public const string EN_CMD_MOVE = "MOVE";
        public const string EN_CMD_NAME = "NAME";
        public const string EN_CMD_NUMBER = "NUMBER";
        public const string EN_CMD_OPTION = "OPTION";
        public const string EN_CMD_ORIGIN = "ORIGIN";
        public const string EN_CMD_PASSWORD = "PASSWORD";
        public const string EN_CMD_PAY_NOT = "PAY NOT";
        public const string EN_CMD_PIRACY = "PIRACY";
        public const string EN_CMD_PLANT = "PLANT";
        public const string EN_CMD_PREFIX = "PREFIX";
        public const string EN_CMD_PROMOTION = "PROMOTION";
        public const string EN_CMD_RECRUIT = "RECRUIT";
        public const string EN_CMD_RESEARCH = "RESEARCH";
        public const string EN_CMD_RESERVE = "RESERVE";
        public const string EN_CMD_RIDING = "RIDING";
        public const string EN_CMD_ROUTE = "ROUTE";
        public const string EN_CMD_SELL = "SELL";
        public const string EN_CMD_SHOW = "SHOW";
        public const string EN_CMD_SORT = "SORT";
        public const string EN_CMD_SPY = "SPY";
        public const string EN_CMD_STEAL = "STEAL";
        public const string EN_CMD_TAX = "TAX";
        public const string EN_CMD_TEACH = "TEACH";
        public const string EN_CMD_USE = "USE";
        public const string EN_CMD_WORK = "WORK";

        // Not a specific language
        public const string NEUTRAL_ALIAS = "alias";
        public const string NEUTRAL_TEMP = "temp";



    }
}
