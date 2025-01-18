using Microsoft.AspNetCore.Mvc;
using Okala.Models;
using System.Diagnostics;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Web;

namespace Okala.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CoinMarketCapService _coinMarketCapService;

        public HomeController(ILogger<HomeController> logger, CoinMarketCapService coinMarketCapService)
        {
            _logger = logger;
            _coinMarketCapService = coinMarketCapService;
        }

        public async Task<IActionResult> Index()
        {
            return View(Array.Empty<SymbolInfo>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm] int symbolId)
        {
            var x = await _coinMarketCapService.Get(symbolId);
            return View(x);
        }

        [HttpGet]
        public IActionResult Search([FromQuery] string symbol)
        {
            return Ok(new { Results = symbol == null ? [] : _coinMarketCapService.Search(symbol) });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }

    public class CoinMarketCapHttpClient(HttpClient httpClient)
    {
        public async Task<CoinMarketResponse<SymbolInfo2>> Get(int id)
        {
            var tasks = new List<Task<string>>();
            foreach (var x in new string[] { "usd", "eur", "brl", "gbp", "aud" })
                tasks.Add(httpClient.GetStringAsync($"/v2/cryptocurrency/quotes/latest?id={id}&convert={x}"));

            await Task.WhenAll(tasks);

            var x2 = JsonNode.Parse(tasks[0].Result);
            //x2["data"][id.ToString()]["quote:UDS"]
            return default;
        }

        public async Task<CoinMarketResponse<SymbolInfo[]>> GetLatestQuotes(int start = 1, int size = 5000)
        {
            return await httpClient.GetFromJsonAsync<CoinMarketResponse<SymbolInfo[]>>($"/v1/cryptocurrency/listings/latest?limit={size}&start={start}");//,eur,brl,gbp,aud");


        }
    }
    public class HttpResponseMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            var sss = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");

            return response;
        }
    }
}
public class CoinMarketResponse<T>
{
    public required Status Status { get; set; }
    public T Data { get; set; }
}
public record SymbolInfo
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public required string Symbol { get; set; }
    public required string Slug { get; set; }

    public string Text => $"{Symbol} ({Name})";

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; }
}

public class Status
{
    public DateTime timestamp { get; set; }
    public int error_code { get; set; }
    public string error_message { get; set; }
    public int elapsed { get; set; }
    public int credit_count { get; set; }
    public object notice { get; set; }
    public DateTime Timestamp { get; set; }
    public int ErrorCode { get; set; }
    public string? ErrorMessage { get; set; } // Nullable
    public int Elapsed { get; set; }
    public int CreditCount { get; set; }
    public string? Notice { get; set; } // Nullable
}

public class SymbolInfo2
{
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }

    public Dictionary<string, SymbolPrice> Quote { get; set; }
}

public class SymbolPrice
{
    public decimal Price { get; set; }
    public decimal Volume24h { get; set; }
    public decimal VolumeChange24h { get; set; }
    public decimal PercentChange1h { get; set; }
    public decimal PercentChange24h { get; set; }
    public decimal PercentChange7d { get; set; }
    public decimal PercentChange30d { get; set; }
    public decimal PercentChange60d { get; set; }
    public decimal PercentChange90d { get; set; }
    public decimal MarketCap { get; set; }
    public decimal MarketCapDominance { get; set; }
    public decimal FullyDilutedMarketCap { get; set; }
    public DateTime LastUpdated { get; set; }
}
