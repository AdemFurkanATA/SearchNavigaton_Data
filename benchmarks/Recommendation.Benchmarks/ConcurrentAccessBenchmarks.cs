using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Recommendation.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class ConcurrentAccessBenchmarks
{
    private readonly List<int> _list = new();
    private readonly ConcurrentDictionary<int, int> _concurrent = new();

    [Benchmark]
    public void ListAccess()
    {
        var list = _list;
        list.Clear();

        Parallel.For(0, 100, i =>
        {
            lock (list)
            {
                list.Add(i);
            }
        });
    }

    [Benchmark]
    public void ConcurrentDictionaryAccess()
    {
        _concurrent.Clear();
        Parallel.For(0, 100, i =>
        {
            _concurrent.AddOrUpdate(i, i, (_, __) => i);
        });
    }
}
