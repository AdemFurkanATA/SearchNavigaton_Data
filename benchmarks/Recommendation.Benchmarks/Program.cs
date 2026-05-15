using BenchmarkDotNet.Running;
using Recommendation.Benchmarks;

BenchmarkRunner.Run<SearchStrategyBenchmarks>();
BenchmarkRunner.Run<ConcurrentAccessBenchmarks>();
