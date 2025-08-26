using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Odyssey.Models.Data;
using Odyssey.Models.Documents;
using Odyssey.Models.Localization;
using Dock.Model.Controls;
using Dock.Model.Mvvm.Core;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Odyssey.Search;
using static Odyssey.Models.Tools.DataProperty;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Odyssey.ViewModels
{
    // search functions
    public class BlockContext
    {
        public CRDocument Report;
        public readonly DataBlock Region;
        public readonly DataBlock Building;
        public readonly DataBlock Ship;
        public readonly DataBlock Unit;
        public readonly Predicate<string> Compare;
        public readonly Predicate<string> CompareIgnoreCase;
        public readonly bool SearchDescriptions;
        public readonly bool SearchFactions;

        public BlockContext(ref CRDocument report, in DataBlock region, in DataBlock building, in DataBlock ship, in DataBlock unit, in Predicate<string> compare, in Predicate<string> compareIgnoreCase, bool searchDescriptions, bool searchFactions)
        {
            this.Report = report;
            this.Region = region;
            this.Building = building;
            this.Ship = ship;
            this.Unit = unit;
            this.Compare = compare;
            this.CompareIgnoreCase = compareIgnoreCase;
            this.SearchDescriptions = searchDescriptions;
            this.SearchFactions = searchFactions;
        }
    }
    public partial class SearchViewModel: ClientAreaDockBase
    {
        // Search domains (everything, regions, units, buildings, ships, commands)
        private const int DOMAIN_EVERYTHING = 0;
        private const int DOMAIN_REGION = 1;
        private const int DOMAIN_UNIT = 2;
        private const int DOMAIN_BLDG = 3;
        private const int DOMAIN_SHIP = 4;
        private const int DOMAIN_CMDS = 5;

        public ObservableCollection<SearchResult> Results { get; }

        private ObservableCollection<string> _domains;

        [ObservableProperty]
        private int _selectedDomainIndex;

        [ObservableProperty]
        private bool _isCaseSensitive;

        [ObservableProperty]
        private bool _isMatchingWholeWord;

        [ObservableProperty]
        private bool _isUsingRegularExpression;

        [ObservableProperty]
        private bool _includeDescriptions;

        [ObservableProperty]
        private bool _includeFactions;

        public ObservableCollection<string> Domains => _domains ??= [];
        public SearchViewModel() : this(null)
        {
            if (!Design.IsDesignMode)
            {
                throw new InvalidOperationException("This constructor should only be used in design mode.");
            }
        }

        public SearchViewModel(IEventAggregator? eventAggregator): base(eventAggregator)
        {
            _isCaseSensitive = true;
            _isMatchingWholeWord = false;
            _isUsingRegularExpression = false;
            _includeDescriptions = false;
            _includeFactions = false;

            _domains = [];

            FillDomains();
            _selectedDomainIndex = 0;
            Results = [];

            //CloseMeCommand.NotifyCanExecuteChanged();
            this.CanClose = true;
        }

        private void FillDomains()
        {
            Domains.Clear();
            Domains.Add(Labels.Localize(Labels.SEARCH_EVERYTHING));
            Domains.Add(Labels.Localize(Labels.SEARCH_REGIONS));
            Domains.Add(Labels.Localize(Labels.SEARCH_UNITS));
            Domains.Add(Labels.Localize(Labels.SEARCH_BUILDINGS));
            Domains.Add(Labels.Localize(Labels.SEARCH_SHIPS));
            Domains.Add(Labels.Localize(Labels.SEARCH_ORDERS));
        }

        [RelayCommand(CanExecute = nameof(CanFindPrevious))]
        private void FindPrevious()
        {
            // TODO
        }

        [RelayCommand(CanExecute = nameof(CanFindNext))]
        private void FindNext()
        {
            // TODO
        }

        [RelayCommand(CanExecute = nameof(CanFindAll))]
        private void FindAll()
        {
            // TODO
        }

        /// <summary>
        /// Indicates if FindPrevious command can be executed.
        /// </summary>
        /// <returns>true if FindPrevious can be executed; otherwise false.</returns>
        private bool CanFindPrevious()
        {
            return HasDocument;
        }

        /// <summary>
        /// Indicates if FindNext command can be executed.
        /// </summary>
        /// <returns>true if FindNext can be executed; otherwise false.</returns>
        private bool CanFindNext()
        {
            return HasDocument;
        }

        /// <summary>
        /// Indicates if FindAll command can be executed.
        /// </summary>
        /// <returns>true if FindAll can be executed; otherwise false.</returns>
        private bool CanFindAll()
        {
            return HasDocument;
        }

    }
}