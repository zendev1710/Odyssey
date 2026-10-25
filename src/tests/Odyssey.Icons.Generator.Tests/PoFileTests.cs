namespace Odyssey.Icons.Generator.Tests;

public class PoFileTests
{
    [Fact]
    public void Parses_spell_entries_and_ignores_other_contexts()
    {
        using var temp = new TempDirectory();
        var path = temp.Write("strings.po", """
            # Translator comment
            msgid ""
            msgstr "Content-Type: text/plain; charset=UTF-8\n"

            msgctxt "spell"
            msgid "treegrow"
            msgstr "Hain des Lebens"

            msgctxt "spell"
            msgid "igjarjuk"
            msgstr ""

            msgctxt "item"
            msgid "sword"
            msgstr "Schwert"

            msgctxt "spell"
            msgid "long_spell"
            msgstr "Ein sehr "
            "langer Name"
            """);

        var entries = PoFile.ParseSpellTranslations(path);

        Assert.Equal(3, entries.Count);
        Assert.Equal("Hain des Lebens", entries["treegrow"]);
        Assert.Equal(string.Empty, entries["igjarjuk"]);
        Assert.Equal("Ein sehr langer Name", entries["long_spell"]);
        Assert.False(entries.ContainsKey("sword"));
    }

    [Fact]
    public void Unescapes_quotes_and_backslashes()
    {
        using var temp = new TempDirectory();
        var path = temp.Write("strings.po", """
            msgctxt "spell"
            msgid "quoted"
            msgstr "a \"quoted\" name"
            """);

        var entries = PoFile.ParseSpellTranslations(path);

        Assert.Equal("a \"quoted\" name", entries["quoted"]);
    }
}
