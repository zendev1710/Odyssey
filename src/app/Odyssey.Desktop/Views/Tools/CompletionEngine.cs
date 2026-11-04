

using AvaloniaEdit.CodeCompletion;
using ExCSS;
using Odyssey.Models.Data;
using Odyssey.ViewModels.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Odyssey.Views.Tools.UnitOrdersView;

namespace Odyssey.Views.Tools;
public class CompletionEngine
{
    // TODO: à exposer dans les options, en plus du mode "flexible" qui proposerait TOUTES les options possibles
    // même si elles ne sont pas forcément réalisables dans le contexte actuel.
    // ex: proposer d'attaquer une unité alliée ou de vendre un produit de luxe même si on n'en a pas
    private bool strictProposalMode = true;

    private readonly UnitOrdersViewModel _vm;
    private readonly List<OrderDefinition> _registry;

    // public static List<ICompletionData> GetProposals(string currentLine, string typedSoFar)
    // {
    //     var parts = currentLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //     var lastWord = parts.Length > 0 ? parts[^1] : "";

    //     // Si on est au début de la ligne
    //     if (parts.Length == 1)
    //     {
    //         return new List<ICompletionData> {
    //             new EresseaCompletionData("GIVE", "Give item to unit", GetItemsList),
    //             new EresseaCompletionData("FOLLOW", "Follow unit", GetUnitsList)
    //         };
    //     }
    //     // Sinon, logique basée sur parts[0] (le mot-clé)
    //     // ...
    //     return new List<ICompletionData>();
    // }

    public CompletionEngine(UnitOrdersViewModel vm)
    {
        _vm = vm;
        _registry = new List<OrderDefinition>();
        InitializeRegistry();
    }

    private void InitializeRegistry()
    {
        // ;
        // //
        // PROMOTION, pas PROMOTE

        // === Navigation & Interaction ===
        //AddOrder("ATTACK", "Attaque une unité", GetAttackableUnits);
        //AddOrder("CONTACT", "Contacter les unités", GetVisibleUnits);
        //AddOrder("FOLLOW", "Suit une unité/bateau", GetVisibleUnitsOrShips);
        ////AddOrder("GIVE", "Transfère des biens", GetVisibleUnits, GetQuantities, GetItemsOrResources);
        ////AddOrder("ENTER", "Entre dans un bâtiment/bateau", GetBuildingsOrShips);
        //AddOrder("MOVE", "Déplacement", GetDirections);
        //AddOrder("ROUTE", "Déplacement automatisé", GetDirections);

        // === Construction & Production ===
        //AddOrder("MAKE", "Fabrique ou exploite", GetProductionOptions);
        //AddComplexOrder("MAKE", "Fabrique un objet", GetMakeParameters);
        //AddComplexOrder("CAST", "Lancer un sort", GetCastParameters);

        //AddOrder("DESTROY", "Démolir", GetDestroyables);
        //AddOrder("PLANT", "Plante des herbes/arbres", GetPlantables);
        //AddOrder("LEARN", "Apprend une compétence", GetAvailableSkills);
        //AddOrder("FORGET", "Oublie une compétence", GetKnownSkills);

        // === Magie & Combat ===
        //AddOrder("CAST", "Lancer un sort", GetRegions, GetSpellLevels, GetAvailableSpells);
        //AddOrder("COMBAT", "Comportement combat", GetCombatModes);
        //AddOrder("COMBATSPELL", "Sorts de combat", GetSpellLevels, GetAvailableSpells);
        //AddOrder("SHOW", "Description", GetShowableOptions);

        // === Gestion Faction & Unité ===
        //AddOrder("NAME", "Renommer", GetRenameOptions);
        //AddOrder("NUMBER", "Attribuer ID", GetNumberOptions);
        //AddOrder("HIDE", "Camouflage/Déguisement", GetHideOptions);
        //AddOrder("HELP", "Alliance faction", GetFactions, GetHelpTypes);
        //AddOrder("PAY", "Paie entretien", GetBuildings);

        AddOrder(Strings.EN_CMD_ATTACK, "", 1, GetAttackableUnits);
        AddOrder(Strings.EN_CMD_BANNER, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_BUY, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_CARRY, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_CAST, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_CLAIM, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_COMBAT, "Définit le comportement lors d'un combat", 1, GetCombatParameters);
        AddOrder(Strings.EN_CMD_COMBATSPELL, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_CONTACT, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_DEFAULT, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_DESCRIBE, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_DESTROY, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_EMAIL, "Définit l'adresse email pour envoyer les ordres et recevoir l'évaluation des tours suivants", 0, NoParametersAddQuotes);
        AddOrder(Strings.EN_CMD_ENTER, "", 1, GetBuildingsOrShips);
        AddOrder(Strings.EN_CMD_ENTERTAIN, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_FOLLOW, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_FORGET, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_GIVE, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_GROUP, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_GROW, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_GUARD, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_HELP, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_HIDE, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_LANGUAGE, "", 1, NoParameters); // auto add de or en according to the current language of the report
        AddOrder(Strings.EN_CMD_LEARN_AUTO, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_LEARN, "", 1, GetLearnableSkills);
        AddOrder(Strings.EN_CMD_LEAVE, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_MAKE, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_MESSAGE, "", 0, NoParametersAddQuotes);
        AddOrder(Strings.EN_CMD_MOVE, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_NAME, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_NUMBER, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_OPTION, "", 1, GetOptionParameters);
        AddOrder(Strings.EN_CMD_ORIGIN, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_PASSWORD, "Définit le mot de passe de votre faction", 0, NoParametersAddQuotes);
        AddOrder(Strings.EN_CMD_PAY_NOT, "Refuse d'honorer les frais d'entretien du bâtiment", 0, NoParameters); // only for building owner
        AddOrder(Strings.EN_CMD_PIRACY, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_PLANT, "", 1, NoParameters); // to do
        AddOrder(Strings.EN_CMD_PREFIX, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_PROMOTION, "Promeut l'unité en tacticien", 0, NoParameters);// to do
        AddOrder(Strings.EN_CMD_RECRUIT, "Embauche dans l'unité", 1, GetAmount);
        AddOrder(Strings.EN_CMD_RESEARCH, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_RESERVE, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_RIDING, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_ROUTE, "", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_SELL, "Vend un produit de luxe", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_SHOW, "Affiche la description d'un objet spécial, 'une potion ou d'un sort", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_SORT, "Modifie la position de l'unité dans le rapport", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_SPY, "Espionne une unité", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_STEAL, "Vole une unité", 1, GetStealableUnits);
        AddOrder(Strings.EN_CMD_TAX, "Collecte les impôts", 1, GetTaxMaxSilverAmount);
        // TODO: handle multiple students: number of parameters is "infinite" (number of units on the region)
        AddOrder(Strings.EN_CMD_TEACH, "Enseigne une compétence à une ou plusieurs unités", 1, GetTeachableUnits);
        AddOrder(Strings.EN_CMD_USE, "Utilise une potion ou un objet spécial", 1, NoParameters);// to do
        AddOrder(Strings.EN_CMD_WORK, "Travaille pour un salaire de misère", 0, NoParameters);// to do
    }

    // Pour les ordres simples qui n'ont pas besoin d'analyser tous les tokens
    public void AddOrder(string keyword, string description, int maxParameters, params Func<UnitOrdersViewModel, List<ICompletionData>>[] providers)
    {
        // On transforme l'ancien système en "Complex" pour uniformiser
        AddComplexOrder(keyword, description, (vm, tokens) =>
        {
            int paramIndex = tokens.Length - 2;
            if (paramIndex >= 0 && paramIndex < providers.Length)
            {
                return providers[paramIndex](vm);
            }
            return new List<ICompletionData>();
        }, maxParameters);
    }

    private static List<ICompletionData> NoParameters(UnitOrdersViewModel vm) => new List<ICompletionData>();

    // === Helper Mappers (À adapter) ===
    private static List<ICompletionData> GetAttackableUnits(UnitOrdersViewModel vm)
    {
        // TODO: handle strict or flexible/free mode 
        //    // IN strict mode:
        //    // - only not allied units are attackable
        //    // - attack only if unit has weapon (or spell?)
        //    // - attack only if combat status allows it (COMBAT, COMBAT AGGRESSIVE, COMBAT REAR or COMBAT DEFENSIVE)

        return vm.UnitsInRegion
             .Where(u => u.Id != vm.CurrentUnitId)
             .Select(u => new EresseaCompletionData(u.Id, u.Name))
             .Cast<ICompletionData>()
             .ToList();
        //return vm.UnitsInRegion.Select(u => new EresseaCompletionData(u.Id, u.Name)).ToList();
    }

    private static List<ICompletionData> GetStealableUnits(UnitOrdersViewModel vm)
    {
        // TODO: handle strict or flexible/free mode 
        //    // IN strict mode:
        //    // - only not allied units are stealable
        //    // - steal only if unit has stealth skill
        return vm.UnitsInRegion
             .Where(u => u.Id != vm.CurrentUnitId)
             .Select(u => new EresseaCompletionData(u.Id, u.Name))
             .Cast<ICompletionData>()
             .ToList();
    }

    
    private static List<ICompletionData> GetTeachableUnits(UnitOrdersViewModel vm)
    {
        // TODO: handle multiple students (unit ids already on the line should be removed from the proposals)
        return vm.UnitsInRegion
             .Where(u => u.Id != vm.CurrentUnitId)
             .Select(u => new EresseaCompletionData(u.Id, u.Name))
             .Cast<ICompletionData>()
             .ToList();
    }

    

    private static List<ICompletionData> GetLearnableSkills(UnitOrdersViewModel vm)
    {
        return new List<ICompletionData> {
            new EresseaCompletionData(Strings.EN_SKILL_ALCHEMY, ""),
            new EresseaCompletionData(Strings.EN_SKILL_ARMOURSMITHING, ""),
            new EresseaCompletionData(Strings.EN_SKILL_BOW, ""),
            new EresseaCompletionData(Strings.EN_SKILL_CARTMAKING, ""),
            new EresseaCompletionData(Strings.EN_SKILL_CATAPULT, ""),
            new EresseaCompletionData(Strings.EN_SKILL_CROSSBOW, ""),
            new EresseaCompletionData(Strings.EN_SKILL_ENDURANCE, ""),
            new EresseaCompletionData(Strings.EN_SKILL_ENTERTAINMENT, ""),
            new EresseaCompletionData(Strings.EN_SKILL_ESPIONAGE, ""),
            new EresseaCompletionData(Strings.EN_SKILL_FORESTRY, ""),
            new EresseaCompletionData(Strings.EN_SKILL_HERBALISM, ""),
            new EresseaCompletionData(Strings.EN_SKILL_MAGIC, ""),
            new EresseaCompletionData(Strings.EN_SKILL_MASONRY, ""),
            new EresseaCompletionData(Strings.EN_SKILL_MELEE, ""),
            new EresseaCompletionData(Strings.EN_SKILL_MINING, ""),
            new EresseaCompletionData(Strings.EN_SKILL_PERCEPTION, ""),
            new EresseaCompletionData(Strings.EN_SKILL_POLEARM, ""),
            new EresseaCompletionData(Strings.EN_SKILL_QUARRYING, ""),
            new EresseaCompletionData(Strings.EN_SKILL_RIDING, ""),
            new EresseaCompletionData(Strings.EN_SKILL_ROADWORK, ""),
            new EresseaCompletionData(Strings.EN_SKILL_SAILING, ""),
            new EresseaCompletionData(Strings.EN_SKILL_SHIPCRAFT, ""),
            new EresseaCompletionData(Strings.EN_SKILL_STEALTH, ""),
            new EresseaCompletionData(Strings.EN_SKILL_TACTICS, ""),
            new EresseaCompletionData(Strings.EN_SKILL_TAMING, ""),
            new EresseaCompletionData(Strings.EN_SKILL_TAXATION, ""),
            new EresseaCompletionData(Strings.EN_SKILL_TRADE, ""),
            new EresseaCompletionData(Strings.EN_SKILL_UNARMED_COMBAT, ""),
            new EresseaCompletionData(Strings.EN_SKILL_WEAPONSMITHING, ""),
        };
    }

    //private static List<ICompletionData> GetVisibleUnits(UnitOrdersViewModel vm) =>
    //    vm.UnitsInRegion.Select(u => new EresseaCompletionData(u.Id, u.Name)).ToList();

    //private static List<ICompletionData> GetVisibleUnitsOrShips(UnitOrdersViewModel vm) =>
    //    vm.UnitsAndShipsInRegion.Select(x => new EresseaCompletionData(x.Id, x.Name)).ToList();

    //private static List<ICompletionData> GetItemsOrResources(UnitOrdersViewModel vm) =>
    //    vm.CurrentUnitInventory.Select(i => new EresseaCompletionData(i.Name, i.Type)).ToList();

    //private static List<ICompletionData> GetDirections(UnitOrdersViewModel vm) =>
    //    new() { "NORTH", "SOUTH", "EAST", "WEST", "NORTHEAST", "NORTHWEST", "SOUTHEAST", "SOUTHWEST" }.Select(d => new EresseaCompletionData(d, "Direction")).ToList();

    //private static List<ICompletionData> GetAvailableSkills(UnitOrdersViewModel vm) =>
    //    vm.GlobalSkills.Select(s => new EresseaCompletionData(s.Name, "Skill")).ToList();

    public List<ICompletionData> GetProposals(string lineText, bool isManualTrigger)
    {
        Debug.WriteLine($"GetProposals called with: '{lineText}' isManualTrigger:{isManualTrigger}");
        lineText = lineText.TrimStart();
        // Remove leading markers "!@", "!" or "@" if present
        if (lineText.StartsWith("!@", StringComparison.Ordinal))
                lineText = lineText.Substring(2);
            else if (lineText.StartsWith("!", StringComparison.Ordinal))
                lineText = lineText.Substring(1);
            else if (lineText.StartsWith("@", StringComparison.Ordinal))
                lineText = lineText.Substring(1);

        Debug.WriteLine($"linetext after: '{lineText}'");
        var tokens = lineText.Split(' ', StringSplitOptions.None);
        var command = tokens[0].ToUpper();
        Debug.WriteLine($"command: {command}, tokens: [{string.Join(", ", tokens)}] tokens-length: {tokens.Length}");
        // TODO: not case sensitive comparison
        if (isManualTrigger && command == tokens.Last())
        {   // Si le curseur est juste après le mot-clé, on considère qu'on n'a rien tapé pour les paramètres
            tokens = [.. tokens, " "];
            isManualTrigger = false;
            Debug.WriteLine($"Whitespace zdded as last token");
        }

        var def = _registry.FirstOrDefault(o => o.Keyword.Equals(command, StringComparison.OrdinalIgnoreCase));

        // --- 1. Requête manuelle (Ctrl+Space) : on ignore le filtrage par caractères tapés ---
        // Si c'est un appel manuel, on veut TOUTES les options possibles pour le paramètre actuel
        string currentTypedToken = isManualTrigger ? "" : (tokens.Length > 0 ? tokens.Last() : "");
        Debug.WriteLine($"currentTypedToken: {currentTypedToken}");

        //--- 2. Mode Recherche de Commande (Début de ligne) ---
        if (tokens.Length <= 1)
        {
            var result = _registry
                .Where(o => isManualTrigger || o.Keyword.StartsWith(currentTypedToken, StringComparison.OrdinalIgnoreCase))
                .Select(o => new EresseaCompletionData(o.Keyword, o.Description))
                .Cast<ICompletionData>()
                .ToList();
            Debug.WriteLine($"Proposals for command: {string.Join(", ", result.Select(r => r.Text))}");
            return result;
        }

        // --- 3. Mode Saisie de Paramètres ---
        if (def != null)
        {
            int currentParamCount = tokens.Length - 1;

            if (currentParamCount > def.MaxParameters)
            {
                Debug.WriteLine($"Parameter count exceeds max allowed ({def.MaxParameters}) for this command");
                return new List<ICompletionData>();
            }

            // On appelle le provider
            var proposals = def.DynamicProvider(_vm, tokens);
            Debug.WriteLine($"Proposals from provider: {string.Join(", ", proposals.Select(p => p.Text))}");

            // Si on n'est pas en manuel, on filtre par ce qui a été tapé
            if (!isManualTrigger && !string.IsNullOrEmpty(currentTypedToken))
            {
               var result = proposals
                    .Where(p => p.Text.StartsWith(currentTypedToken, StringComparison.OrdinalIgnoreCase))
                    .ToList();
               Debug.WriteLine($"Filtered proposals: {string.Join(", ", result.Select(r => r.Text))}");
               return result;
            }

            Debug.WriteLine($"returned proposals {string.Join(", ", proposals.Select(p => p.Text))}");
            return proposals;
        }

        Debug.WriteLine($"returned EMPTY list for command '{command}'");
        return new List<ICompletionData>();
    }

    //private static List<ICompletionData> GetCastParameters(UnitOrdersViewModel vm, int paramIndex)
    //{
    //    // paramIndex 0: REGION x y (optionnel)
    //    // paramIndex 1: LEVEL n (optionnel)
    //    // paramIndex 2: Nom du sort

    //    // Logique : Si l'utilisateur a tapé "CAST REGION", on propose les coordonnées
    //    // Si il a tapé "CAST LEVEL", on propose les chiffres
    //    // Sinon, on propose la liste des sorts connus.
    //    return vm.AvailableSpells.Select(s => new EresseaCompletionData(s.Name, s.Description)).ToList();
    //}

    //private static List<ICompletionData> GetMakeParameters(UnitOrdersViewModel vm, string[] tokens)
    //{
    //    // tokens[0] est toujours "MAKE"
    //    // tokens[1] est le paramètre actuel ou en cours

    //    // Si on vient juste de taper "MAKE " (tokens.Length == 2)
    //    if (tokens.Length == 2)
    //    {
    //        return new List<ICompletionData> {
    //            new EresseaCompletionData("TEMP", "Crée une nouvelle unité"),
    //            new EresseaCompletionData("SHIP", "Construit un nouveau bateau"),
    //            new EresseaCompletionData("STREET", "Construit une route"),
    //            new EresseaCompletionData("HERBS", "Récolte des plantes"),
    //            new EresseaCompletionData("POTION", "Produit une potion alchimique"),
    //            // Ajoutez ici vos types de bâtiments courants si besoin
    //        };
    //    }

    //    // Analyse contextuelle selon le premier paramètre tapé
    //    string firstParam = tokens[1].ToUpper();

    //    return firstParam switch
    //    {
    //        "TEMP" => GetMakeTempContext(vm, tokens),
    //        "SHIP" => GetMakeShipContext(vm, tokens),
    //        "STREET" => GetMakeStreetContext(vm, tokens),
    //        "HERBS" => new List<ICompletionData>(), // Fin de la commande
    //        _ => new List<ICompletionData>() // Cas par défaut
    //    };
    //}

    private void AddComplexOrder(string keyword, string description, Func<UnitOrdersViewModel, string[], List<ICompletionData>> provider, int maxParameters = 1)
    {
        _registry.Add(new OrderDefinition
        {
            Keyword = keyword,
            Description = description,
            MaxParameters = maxParameters,
            DynamicProvider = provider
        });
    }

    private static List<ICompletionData> GetMakeTempContext(UnitOrdersViewModel vm, string[] tokens)
    {
        // MAKE TEMP <alias-id> ["<name>"]
        if (tokens.Length == 3) // On attend l'ID de l'alias
            return new List<ICompletionData> { new EresseaCompletionData("alias-id", "ID unique de l'unité temporaire") };

        if (tokens.Length == 4) // On attend le nom optionnel
            return new List<ICompletionData> { new EresseaCompletionData("\"Nom de l'unité\"", "Nom optionnel") };

        return new List<ICompletionData>();
    }

    private static List<ICompletionData> GetMakeShipContext(UnitOrdersViewModel vm, string[] tokens)
    {
        // MAKE SHIP <level> [id]
        if (tokens.Length == 3) // On attend le niveau
            return new List<ICompletionData> { new EresseaCompletionData("1", "Niveau de construction"), new EresseaCompletionData("2", "Niveau supérieur") };

        if (tokens.Length == 4) // On attend l'ID du bateau (optionnel)
            return new List<ICompletionData> { new EresseaCompletionData("ship-id", "ID optionnel du bateau") };

        return new List<ICompletionData>();
    }

    /*
    private static List<ICompletionData> GetMakeParameters(UnitOrdersViewModel vm, string[] tokens)
    {
        // tokens[0] est "MAKE"
        // tokens[1] est le paramètre actuel ou en cours

        // Si on vient de taper "MAKE", on regarde le contexte
        if (tokens.Length == 2)
        {
            return new List<ICompletionData> {
                new EresseaCompletionData("TEMP", "Crée une unité temporaire"),
                new EresseaCompletionData("SHIP", "Construit un bateau"),
                new EresseaCompletionData("STREET", "Construit une route"),
                // ... autres options basiques
            };
        }

        // Si on a tapé "MAKE TEMP"
        if (tokens[1].Equals("TEMP", StringComparison.OrdinalIgnoreCase))
        {
            return new List<ICompletionData> { new EresseaCompletionData("unit-alias-id", "ID de l'unité") };
        }

        // Si on a tapé "MAKE SHIP"
        if (tokens[1].Equals("SHIP", StringComparison.OrdinalIgnoreCase))
        {
            return vm.Report.GetAvailableShipTypes().Select(st => new EresseaCompletionData(st.Name, "Type bateau")).ToList();
        }

        return new List<ICompletionData>();
    }
    */

    // --- Helpers de Contexte (À implémenter selon votre modèle de données) ---
    //private static List<ICompletionData> GetUnitsInRegion(UnitOrdersViewModel vm)
    //{
    //    // Appeler la méthode de votre VM qui retourne les unités proches
    //    return vm.UnitsInRegion.Select(u => new EresseaCompletionData(u.Id, u.Name)).ToList();
    //}

    //private static List<ICompletionData> GetUnitItems(UnitOrdersViewModel vm)
    //{
    //    // Propose les objets possédés par l'unité sélectionnée
    //    return vm.Selection.Item.GetItems().Select(i => new EresseaCompletionData(i.Name, "Item")).ToList();
    //}

    // private static List<ICompletionData> GetUnitsInRegion(UnitOrdersViewModel vm)
    //     => vm.Report.GetUnitsInRegion().Select(u => new EresseaCompletionData(u.Id, u.Name)).ToList();

    
    private static List<ICompletionData> GetCombatParameters(UnitOrdersViewModel vm)
    {     return new List<ICompletionData> {
            new EresseaCompletionData("AGGRESSIVE", "En première ligne jusqu'à trépas"),
            new EresseaCompletionData("DEFENSIVE", "En deuxième ligne (adapté aux mages)"),
            new EresseaCompletionData("FLEE", "Fuit le combat (adapté aux civils)"),
            new EresseaCompletionData("FRONT", "En première ligne (adapté aux bons épéistes)"),
            new EresseaCompletionData("NOT", "Évite le combat (adapté aux occupants d'un bâtiment ou d'un bateau à terre)"),
            new EresseaCompletionData("REAR", "En deuxième ligne en acceptant plus de dommages (adapté aux tireurs)"),
        };
    }

    private static List<ICompletionData> GetOptionParameters(UnitOrdersViewModel vm)
    {
        return new List<ICompletionData> {
            new EresseaCompletionData("ADDRESSES", ""),
            new EresseaCompletionData("BZIP2", ""),
            new EresseaCompletionData("COMPUTER", ""),
            new EresseaCompletionData("PLAINTEXT", ""),
            new EresseaCompletionData("SCORE", ""),
            new EresseaCompletionData("SHOWSKCHANGE", ""),
            new EresseaCompletionData("STATISTICS", ""),
            new EresseaCompletionData("TEMPLATE", ""),
            new EresseaCompletionData("ZIPPED", ""),
        };
    }

    /*
    private static List<ICompletionData> GetParameters(UnitOrdersViewModel vm)
    {
        return new List<ICompletionData> {
            new EresseaCompletionData("", ""),
            new EresseaCompletionData("", ""),
            new EresseaCompletionData("", ""),
            new EresseaCompletionData("", ""),
            new EresseaCompletionData("", ""),
            new EresseaCompletionData("", ""),
        };
    }
    */

    private static List<ICompletionData> GetAmount(UnitOrdersViewModel vm)
    {
        return new List<ICompletionData> {
            new EresseaCompletionData("<amount>", "Nombre de personnes à recruter (1 par défaut)"),
        };
    }

    private static List<ICompletionData> GetTaxMaxSilverAmount(UnitOrdersViewModel vm)
    {
        // TODO: suggest dozens, hundreds, or thousands of silver based on the current region's wealth or the unit's tax skill level
        // TODO: in strict mode, the maximum should not exceed the amount of silver available in the region or the tax capacity of the unit
        return new List<ICompletionData> {
            new EresseaCompletionData("<amount>", "Montant maximum d'argent à collecter"),
        };
    }
  

    private static List<ICompletionData> GetBuildingsOrShips(UnitOrdersViewModel vm)
    {
        // TODO: Proposer les bâtiments et bateaux disponibles dans la région, au format <icone selon le type et l'appartenance de faction> Nom (id))
        // TODO: la sélection sur carctères tapés peut être basée sur le nom ou l'id
        // TODO: la liste sera triée par faction puis nom
        return new List<ICompletionData> {
            new EresseaCompletionData("b01", "mon bateau n° 1"),
            new EresseaCompletionData("b02", "mon bateau n° 2"),
            new EresseaCompletionData("c01", "mon château n° 1"),
            new EresseaCompletionData("c02", "mon château n° 2"),
            new EresseaCompletionData("eb01", "bateau ennemi n° 1"),
            new EresseaCompletionData("ec01", "château ennemi n° 1"),
        };
    }

    private static List<ICompletionData> NoParametersAddQuotes(UnitOrdersViewModel vm)
    {
        // TODO: " "" à ajouter à la fin de l'ordre pour permettre d'ajouter un message d'ordre
        return new List<ICompletionData>() { new EresseaCompletionData("", "", null) };
    }

    private static List<ICompletionData> GetItemsOrSilver(UnitOrdersViewModel vm)
        => new List<ICompletionData> { new EresseaCompletionData("SILVER", "Argent"), new EresseaCompletionData("MEN", "Personnes") };

    private static List<ICompletionData> GetRegions(UnitOrdersViewModel vm) => new();
    private static List<ICompletionData> GetSpellLevels(UnitOrdersViewModel vm) => new();
    private static List<ICompletionData> GetSpells(UnitOrdersViewModel vm) => new();
}

