namespace Recommendation.Search.Options;

public sealed class SearchOptions
{
    public int FuzzyMaxEditDistance { get; set; } = 2;
    public int CacheTtlMinutes { get; set; } = 5;
    public string DefaultStrategy { get; set; } = "prefix";
    public int DefaultLimit { get; set; } = 10;
}
