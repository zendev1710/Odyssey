using Avalonia.Controls;
using ConfigFactory.Models;
using ConfigFactory;
using ConfigFactory.Avalonia;
using System;
using Odyssey.ViewModels;
using Odyssey.Settings;
using Odyssey.Models.Localization;
using Avalonia.Interactivity;
using System.Linq;
using System.Threading.Tasks;

namespace Odyssey.Views;
public partial class ConfigurationView : UserControl
{
    private readonly SkillsSettingsView _skillsSettingsView;

    public ConfigurationView()
    {
        InitializeComponent();
        _skillsSettingsView = new SkillsSettingsView();
        if (ConfigPage.DataContext is ConfigPageModel model)
        {
            model.Append<GlobalSettings>();
            CustomizeButtons(model);
            AddSkillsSettingsPage(model);
            model.SecondaryButtonCompletedEvent += () =>
            {
                if (DataContext is ConfigurationViewModel vm)
                {
                    vm.CancelAndClose();
                }

                return Task.CompletedTask;
            };
        }
    }

    private void AddSkillsSettingsPage(ConfigPageModel model)
    {
        ConfigCategory? generalCategory = model.Categories.FirstOrDefault(category => category.Id == "parm_cat_general");
        ConfigGroup? behavioursGroup = generalCategory?.Groups.FirstOrDefault(group => group.Id == "parm_grp_behaviours");
        if (generalCategory is null || behavioursGroup is null)
        {
            throw new InvalidOperationException("The General settings category must contain the Behaviours group.");
        }

        ConfigGroup skillsGroup = new("parm_grp_skills", Labels.Localize("settings_skills"));
        skillsGroup.Items.Add(new ConfigFactory.Models.ConfigItem
        {
            Header = Labels.Localize("skills_display_title"),
            Description = Labels.Localize("skills_display_description"),
            Content = _skillsSettingsView.SkillSortPanelContent
        });
        skillsGroup.Items.Add(new ConfigFactory.Models.ConfigItem
        {
            Header = Labels.Localize("skill_category_order_title"),
            Description = Labels.Localize("skill_category_order_description"),
            Content = _skillsSettingsView.CategoryOrderPanelContent
        });
        skillsGroup.Items.Add(new ConfigFactory.Models.ConfigItem
        {
            Header = Labels.Localize("skill_order_title"),
            Content = _skillsSettingsView.SkillOrderPanelContent
        });

        generalCategory.Groups.Insert(generalCategory.Groups.IndexOf(behavioursGroup) + 1, skillsGroup);
    }

    /// <summary>
    /// Handles settings properties changes and save events.
    /// </summary>
    /// <param name="e">Data context changed event</param>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (this.DataContext is ConfigurationViewModel) {
            _skillsSettingsView.SetSettingsDataContext(DataContext);
            GlobalSettings instance = GlobalSettings.Shared;
            instance.OnSave += Settings_OnSave;
            instance.PropertyChanged += Settings_PropertyChanged;
        }
    }

    /// <summary>
    /// Translates the configuration page button labels to the current language.
    /// </summary>
    /// <param name="model"></param>
    private static void CustomizeButtons(ConfigPageModel model)
    {
        // button width should change according to the content, see https://github.com/ArchLeaders/ConfigFactory/issues/6
        model.PrimaryButtonContent = Labels.Localize("parm_btn_save");
        model.SecondaryButtonContent = Labels.Localize("parm_btn_cancel");
    }

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // occurs when a configuration option is modified
    }

    /// <summary>
    /// Handles the save event for the current Configuration instance.
    /// ConfigurationView is closed when the user clicks the save button, the home default view is displayed.
    /// </summary>
    private void Settings_OnSave()
    {
        if (this.DataContext is ConfigurationViewModel vm)
        {
            vm.GoBackToWorkspace();
        }
    }

}
