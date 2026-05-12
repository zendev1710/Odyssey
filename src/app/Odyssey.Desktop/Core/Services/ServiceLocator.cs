namespace Odyssey.Core.Services;

public static class ServiceLocator
{
    public static ILanguageService LanguageService { get; private set; }

    public static void Initialize()
    {
        LanguageService = new LanguageService();
    }
}

