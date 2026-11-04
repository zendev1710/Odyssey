namespace Odyssey.Icons.Generator.Tests;

public class SpellTableTests
{
    [Fact]
    public void Parses_the_spell_functions_table_and_stops_at_its_end()
    {
        using var temp = new TempDirectory();
        var path = temp.Write("spells.c", """
            #include "spells.h"

            static spelldata spell_functions[] = {
                /* Fire */
                { "treegrow", sp_treegrow, NULL },
                { "irongolem", sp_irongolem, NULL },
                { "treegrow", sp_treegrow, NULL },
                { 0, 0, 0 }
            };

            static void other(void)
            {
                { "not_a_spell", a, b },
            }
            """);

        var names = SpellTable.Parse(path);

        Assert.Equal(new[] { "treegrow", "irongolem" }, names);
    }

    [Fact]
    public void Fails_when_the_table_is_missing()
    {
        using var temp = new TempDirectory();
        var path = temp.Write("spells.c", "int main(void) { return 0; }");

        var error = Assert.Throws<InvalidOperationException>(() => SpellTable.Parse(path));

        Assert.Contains("spell_functions[]", error.Message, StringComparison.Ordinal);
    }
}
