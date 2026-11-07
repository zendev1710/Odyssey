using Avalonia.Controls;
using Avalonia.Interactivity;
using Odyssey.Models.Localization;
using Odyssey.ViewModels;
using System;

namespace Odyssey.Views;

public partial class SkillsSettingsView : UserControl
{
    public SkillsSettingsView()
    {
        InitializeComponent();
        SettingsPanels.Children.Remove(SkillSortPanel);
        SettingsPanels.Children.Remove(CategoryOrderPanel);
        SettingsPanels.Children.Remove(SkillOrderPanel);
        GroupSkillsByCategoryCheckBox.Content = Labels.Localize("skills_group_by_category");
        RestoreCategoryOrderButton.Content = Labels.Localize("skill_category_reset_default");
        SortAlphabeticallyButton.Content = "A -> Z";
    }

    public Control SkillSortPanelContent => SkillSortPanel;

    public Control CategoryOrderPanelContent => CategoryOrderPanel;

    public Control SkillOrderPanelContent => SkillOrderPanel;

    public void SetSettingsDataContext(object? dataContext)
    {
        DataContext = dataContext;
        SkillSortPanel.DataContext = dataContext;
        CategoryOrderPanel.DataContext = dataContext;
        SkillOrderPanel.DataContext = dataContext;
    }

    private void RestoreCategoryOrder_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigurationViewModel vm)
        {
            vm.RestoreDefaultCategoryOrder();
        }
    }

    private void MoveCategory_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SkillCategoryOrderItemViewModel item, Tag: string direction } || DataContext is not ConfigurationViewModel vm)
        {
            return;
        }

        if (direction == "move-up")
        {
            vm.MoveCategoryUp(item);
        }
        else if (direction == "move-down")
        {
            vm.MoveCategoryDown(item);
        }
    }

    private void SortAlphabetically_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ConfigurationViewModel vm)
        {
            vm.SortSkillsAlphabetically();
        }
    }

    private void MoveSkill_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: SkillOrderItemViewModel item, Tag: string direction } || DataContext is not ConfigurationViewModel vm)
        {
            return;
        }

        if (direction == "move-up")
        {
            vm.MoveSkillUp(item);
        }
        else if (direction == "move-down")
        {
            vm.MoveSkillDown(item);
        }
    }
}
