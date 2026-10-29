using CommunityToolkit.Mvvm.ComponentModel;
using Odyssey.Models.Documents;

namespace Odyssey.ViewModels.Tools;

public partial class UnitInspectorViewModel : DocumentToolViewModelBase
{
    [ObservableProperty]
    private UnitViewModel? selectedUnit;

    public UnitInspectorViewModel() : base()
    {
    }

    protected override void OnActiveDocumentChanged(CRDocument report)
    {
        base.OnActiveDocumentChanged(report);
        SelectedUnit = null;
    }

    public void SetSelectedUnit(int? unitId)
    {
        if (!HasDocument || unitId is null)
        {
            SelectedUnit = null;
            return;
        }

        SelectedUnit = UnitViewModel.FromDocument(Report, unitId.Value);
    }
}