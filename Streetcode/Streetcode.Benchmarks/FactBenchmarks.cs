using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Streetcode.Benchmarks;

[MemoryDiagnoser]
public class FactBenchmarks
{
    private const string ControllerBase = "/api/FactController";
    private const string MinimalBase = "/api/FactMinimalApi";

    private readonly HttpClient _client;

    public FactBenchmarks()
    {
        var factory = new WebApplicationFactory<WebApi.Program>();
        _client = factory.CreateClient();
    }

    [Benchmark(Baseline = true)]
    public async Task Controller_GetAll()
    {
        var response = await _client.GetAsync($"{ControllerBase}/GetAll");

        response.EnsureSuccessStatusCode();
    }

    [Benchmark]
    public async Task MinimalApi_GetAll()
    {
        var response = await _client.GetAsync($"{MinimalBase}/GetAll");

        response.EnsureSuccessStatusCode();
    }

    [Benchmark]
    public async Task Controller_GetById()
    {
        var response = await _client.GetAsync($"{ControllerBase}/GetById/{1}");

        response.EnsureSuccessStatusCode();
    }

    [Benchmark]
    public async Task MinimalApi_GetById()
    {
        var response = await _client.GetAsync($"{MinimalBase}/GetById/{1}");

        response.EnsureSuccessStatusCode();
    }
}