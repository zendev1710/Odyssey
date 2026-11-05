using CommunityToolkit.Mvvm.ComponentModel;
using ConfigFactory.Models;
using Odyssey.Core.Services;
using Odyssey.Models.Localization;
using Odyssey.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Odyssey.ViewModels
{
    public partial class ConfigurationViewModel : ClientAreaDockBase
    {
        private static readonly string[] SkillKeys =
        [
            "Alchemie", "Armbrustschießen", "Ausdauer", "Bergbau", "Bogenschießen", "Burgenbau",
            "Handeln", "Hiebwaffen", "Holzfällen", "Katapultbedienung", "Kräuterkunde", "Magie",
            "Pferdedressur", "Reiten", "Rüstungsbau", "Schiffbau", "Segeln", "Spionage",
            "Stangenwaffen", "Steinbau", "Steuereintreiben", "Straßenbau", "Taktik", "Tarnung",
            "Unterhaltung", "Waffenbau", "WaffenloserKampf", "Wagenbau", "Wahrnehmung"
        ];

        [ObservableProperty]
        private ConfigPageModel _configPageModel;

        public GlobalSettings Settings { get; }
        public ObservableCollection<string> SkillSortModes { get; } = new();
        public ObservableCollection<SkillOrderItemViewModel> SkillOrderItems { get; } = new();

        public ConfigurationViewModel(IDocumentService? documentService = null) : base(documentService)
        {
            Settings = GlobalSettings.Shared;
            _configPageModel = ConfigFactory.ConfigFactory.Build<GlobalSettings>();
            _configPageModel.PropertyChanged += _configPageModelPropertyChanged;

            SkillSortModes.Add(Labels.Localize("skill_sort_mode_level"));
            SkillSortModes.Add(Labels.Localize("skill_sort_mode_alphabetical"));
            SkillSortModes.Add(Labels.Localize("skill_sort_mode_custom"));
            LoadSkillOrder();
        }

        private void LoadSkillOrder()
        {
            string[] savedKeys = Settings.UnitSkillCustomOrder
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            HashSet<string> knownKeys = new(SkillKeys, StringComparer.Ordinal);
            List<string> orderedKeys = savedKeys
                .Where(knownKeys.Contains)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            orderedKeys.AddRange(SkillKeys.Where(key => !orderedKeys.Contains(key, StringComparer.Ordinal)));

            foreach (string key in orderedKeys)
            {
                SkillOrderItems.Add(new SkillOrderItemViewModel(key));
            }

            if (savedKeys.Length > 0)
            {
                LoadSkillOrder(savedKeys);
            }
            else
            {
                SortSkillOrderItemsAlphabetically();
            }
            SaveSkillOrder();
        }

        private void LoadSkillOrder(string[] orderedKeys)
        {
            Dictionary<string, int> orderIndexes = orderedKeys
                .Distinct(StringComparer.Ordinal)
                .Select((key, index) => new { key, index })
                .ToDictionary(item => item.key, item => item.index, StringComparer.Ordinal);
            SkillOrderItemViewModel[] orderedItems = SkillOrderItems
                .OrderBy(item => orderIndexes.TryGetValue(item.Key, out int index) ? index : int.MaxValue)
                .ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
            SkillOrderItems.Clear();
            foreach (SkillOrderItemViewModel item in orderedItems)
            {
                SkillOrderItems.Add(item);
            }
        }

        public void MoveSkillUp(SkillOrderItemViewModel? item) => MoveSkill(item, -1);

        public void MoveSkillDown(SkillOrderItemViewModel? item) => MoveSkill(item, 1);

        private void MoveSkill(SkillOrderItemViewModel? item, int offset)
        {
            int currentIndex = item is null ? -1 : SkillOrderItems.IndexOf(item);
            int targetIndex = currentIndex + offset;
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= SkillOrderItems.Count)
            {
                return;
            }

            SkillOrderItems.Move(currentIndex, targetIndex);
            SaveSkillOrder();
        }

        public void SortSkillsAlphabetically()
        {
            SortSkillOrderItemsAlphabetically();
            if (SkillOrderItems.Count > 0)
            {
                SaveSkillOrder();
            }
        }

        private void SortSkillOrderItemsAlphabetically()
        {
            SkillOrderItemViewModel[] orderedItems = SkillOrderItems
                .OrderBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(item => item.Key, StringComparer.Ordinal)
                .ToArray();
            SkillOrderItems.Clear();
            foreach (SkillOrderItemViewModel item in orderedItems)
            {
                SkillOrderItems.Add(item);
            }
        }

        private void SaveSkillOrder()
        {
            Settings.UnitSkillCustomOrder = string.Join('|', SkillOrderItems.Select(item => item.Key));
        }

        public void SaveAndClose()
        {
            Settings.Save();
        }

        public void CancelAndClose()
        {
            Settings.Reset();
            GoBackToWorkspace();
        }

        private void _configPageModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO
        }

        public void GoBackToWorkspace()
        {
            // TODO: handle Esc key to go back to workspace
            base.GoBackToWorkspaceCommand.Execute(null);
        }
    }
}
