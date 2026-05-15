using FluentAssertions;
using Recommendation.Search.Trie;

namespace Recommendation.Search.Tests;

public sealed class TrieTests
{
    [Fact]
    public void InsertAndSearch_ReturnsCorrectProducts()
    {
        var trie = new Trie.Trie();
        trie.Insert("wireless", "product-1");
        trie.Insert("wired", "product-2");
        trie.Insert("wire", "product-3");
        trie.Freeze();

        var results = trie.GetByPrefix("wire");
        results.Should().Contain(new[] { "product-1", "product-2", "product-3" });
    }
}
