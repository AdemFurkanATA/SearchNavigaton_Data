using System.Collections.Immutable;

namespace Recommendation.Search.Trie;

public sealed class TrieNode
{
    public IReadOnlyDictionary<char, TrieNode> Children { get; }
    public IReadOnlyCollection<string> ProductIds { get; }

    public TrieNode(IReadOnlyDictionary<char, TrieNode> children, IReadOnlyCollection<string> productIds)
    {
        Children = children;
        ProductIds = productIds;
    }

    public static TrieNode Empty { get; } = new(ImmutableDictionary<char, TrieNode>.Empty, ImmutableHashSet<string>.Empty);
}
