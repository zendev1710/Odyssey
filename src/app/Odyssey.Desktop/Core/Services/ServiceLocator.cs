using Odyssey.Services;

namespace Odyssey.Core.Services;

public static class ServiceLocator
{
    public static ILanguageService LanguageService { get; private set; } = default!;
    public static IDocumentService DocumentService { get; private set; } = default!;
    //public static ISelectionService SelectionService { get; private set; } = default!;
    public static IEntitySelectionService EntitySelectionService { get; private set; } = default!;

    public static void Initialize()
    {
        LanguageService = new LanguageService();
        DocumentService = new DocumentService();
        //SelectionService = new SelectionService();
        EntitySelectionService = new EntitySelectionService();
    }
}
