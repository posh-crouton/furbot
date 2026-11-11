using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FurBot.Configuration;
using FurBot.Types.Gelbooru;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FurBot.Clients;

internal sealed class GelbooruClient : IGelbooruClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<GelbooruOptions> _options;
    private readonly ILogger<GelbooruClient> _logger;

    private bool _initialised;

    public GelbooruClient(HttpClient httpClient, IOptions<GelbooruOptions> options, ILogger<GelbooruClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public void Init()
    {
        if (_initialised)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Value.ContactInfo))
        {
            throw new InvalidOperationException($"{nameof(_options.Value.ContactInfo)} is required (API policy)");
        }

        if (string.IsNullOrWhiteSpace(_options.Value.Username))
        {
            throw new InvalidOperationException($"{nameof(_options.Value.Username)} is required (API policy)");
        }

        if (string.IsNullOrWhiteSpace(_options.Value.ApiKey))
        {
            throw new InvalidOperationException($"{nameof(_options.Value.ApiKey)} is required (API policy)");
        }

        _httpClient.BaseAddress = new Uri(_options.Value.InstanceUri);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent",
            $"FurBot (github.com/posh-crouton/FurBot; contact: {_options.Value.ContactInfo})");

        string credentials = Convert.ToBase64String( Encoding.UTF8.GetBytes($"{_options.Value.Username}:{_options.Value.ApiKey}") );
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        _initialised = true;
    }

    public async Task<IEnumerable<GelbooruPost>> GetPostsAsync(string query, int limit, CancellationToken ct = default)
    {
        if (limit > 320)
        {
            throw new ArgumentOutOfRangeException(nameof(limit),
                $"{nameof(limit)} must be less than or equal to 320 (API policy)");
        }

        string url = $"posts.json?limit={limit}&tags={Uri.EscapeDataString(query)}";

        using HttpResponseMessage response = await _httpClient.GetAsync(url, ct);

        string content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Call to {url} failed with status {status}: {message}", url, response.StatusCode, content);
        }

        response.EnsureSuccessStatusCode();

        PostResponse? result = JsonSerializer.Deserialize<PostResponse>(content);
        return result?.Posts ?? [];
    }

    private sealed class PostResponse
    {
        [JsonPropertyName("posts")] public List<GelbooruPost> Posts { get; init; } = [];
    }
}

internal interface IGelbooruClient
{
    void Init();
    Task<IEnumerable<GelbooruPost>> GetPostsAsync(string query, int limit, CancellationToken ct = default);
}

