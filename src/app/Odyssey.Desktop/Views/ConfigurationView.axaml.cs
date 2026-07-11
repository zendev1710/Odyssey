using Avalonia.Controls;
using ConfigFactory.Models;
using ConfigFactory;
using ConfigFactory.Avalonia;
using System;
using Odyssey.ViewModels;
using Odyssey.Settings;
using Odyssey.Models.Localization;

namespace Odyssey.Views;
public partial class ConfigurationView : UserControl
{
    public ConfigurationView()
    {
        InitializeComponent();
        if (ConfigPage.DataContext is ConfigPageModel model)
        {
            model.Append<GlobalSettings>();
            CustomizeButtons(model);
        }
    }

    /// <summary>
    /// Handles settings properties changes and save events.
    /// </summary>
    /// <param name="e">Data context changed event</param>
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (this.DataContext is ConfigurationViewModel) {
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
            vm.GoHome();
        }
    }
}
