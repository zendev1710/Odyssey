namespace Odyssey.Icons.Generator.Tests;

public class StrHashTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("Hodor", 140703196)]
    [InlineData("treegrow", 992591507)]
    [InlineData("irongolem", 981864732)]
    public void Compute_matches_the_server(string name, int expected)
    {
        Assert.Equal(expected, StrHash.Compute(name));
    }
}
