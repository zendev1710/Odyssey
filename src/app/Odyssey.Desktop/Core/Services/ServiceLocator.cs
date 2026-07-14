using Odyssey.Services;

namespace Odyssey.Core.Services;

public static class ServiceLocator
{
    public static ILanguageService LanguageService { get; private set; } = default!;
    public static IActiveDocumentService ActiveDocumentService { get; private set; } = default!;
    public static ISelectionService SelectionService { get; private set; } = default!;

    public static void Initialize()
    {
        LanguageService = new LanguageService();
        ActiveDocumentService = new ActiveDocumentService();
        SelectionService = new SelectionService();
    }
}
