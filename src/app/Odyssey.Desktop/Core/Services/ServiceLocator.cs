using Odyssey.Services;

namespace Odyssey.Core.Services;

public static class ServiceLocator
{
    public static ILanguageService LanguageService { get; private set; } = new LanguageService();
    public static IDocumentService DocumentService { get; private set; } = new DocumentService();
    //public static ISelectionService SelectionService { get; private set; } = default!;
    public static IEntitySelectionService EntitySelectionService { get; private set; } = new EntitySelectionService();

    public static void Initialize()
    {
        LanguageService = new LanguageService();
        DocumentService = new DocumentService();
        //SelectionService = new SelectionService();
        EntitySelectionService = new EntitySelectionService();
    }
}
