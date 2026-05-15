using System.Collections.Immutable;

namespace Recommendation.Search.Trie;

public sealed class Trie
{
    private readonly MutableTrieNode _root = new();
    private TrieNode? _frozen;
    private readonly object _lock = new();

    public void Insert(string word, string productId)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return;
        }

        lock (_lock)
        {
            var current = _root;
            foreach (var ch in word)
            {
                if (!current.Children.TryGetValue(ch, out var next))
                {
                    next = new MutableTrieNode();
                    current.Children[ch] = next;
                }

                current = next;
                current.ProductIds.Add(productId);
            }
        }
    }

    public void Freeze()
    {
        lock (_lock)
        {
            _frozen = FreezeNode(_root);
        }
    }

    public IReadOnlyCollection<string> GetByPrefix(string prefix)
    {
        var root = _frozen ?? TrieNode.Empty;
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return Array.Empty<string>();
        }

        var current = root;
        foreach (var ch in prefix)
        {
            if (!current.Children.TryGetValue(ch, out var next))
            {
                return Array.Empty<string>();
            }

            current = next;
        }

        return current.ProductIds;
    }

    private static TrieNode FreezeNode(MutableTrieNode node)
    {
        var children = node.Children.ToImmutableDictionary(
            kvp => kvp.Key,
            kvp => FreezeNode(kvp.Value));

        var products = node.ProductIds.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
        return new TrieNode(children, products);
    }

    private sealed class MutableTrieNode
    {
        public Dictionary<char, MutableTrieNode> Children { get; } = new();
        public HashSet<string> ProductIds { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
