using Microsoft.AspNetCore.Mvc;
using Okala.Models;
using System.Diagnostics;
using System.Net;
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
        public async Task<IActionResult> Index([FromForm] string? symbol)
        {
            var x = _coinMarketCapService.Search(symbol ?? "btc");
            return View(x);
        }

        [HttpGet]
        public IActionResult Search([FromQuery]string symbol)
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


        static string makeAPICall()
        {
            //var URL = new UriBuilder("https://sandbox-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest?symbol=BTC");
            var URL = new UriBuilder("https://sandbox-api.coinmarketcap.com/v2/tools/price-conversion?symbol=BTC");

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["start"] = "1";
            queryString["limit"] = "5000";
            queryString["convert"] = "USD";

            //URL.Query = queryString.ToString();

            var client = new WebClient();
            client.Headers.Add("X-CMC_PRO_API_KEY", "0de2dfc8-c1b0-440a-8afd-e8d2795827b3");
            client.Headers.Add("Accepts", "application/json");
            return client.DownloadString(URL.ToString());

        }
    }

    public class CoinMarketCapHttpClient(HttpClient httpClient)
    {
        public async Task<CoinMarketResponse<SymbolInfo2>> Get()
        {
            await GetLatestQuotes();
            ///
            //return await httpClient.GetFromJsonAsync<CoinMarketResponse>("/v2/cryptocurrency/quotes/latest?id=1027&convert=usd,, EUR, BRL, GBP, and AUD");
            return await httpClient.GetFromJsonAsync<CoinMarketResponse<SymbolInfo2>>("/v2/cryptocurrency/quotes/latest?symbol=btc&convert=usd");//,eur,brl,gbp,aud");
            var resuul = await httpClient.GetStringAsync("/v2/cryptocurrency/quotes/latest?symbol=btc&convert=ltc,usd");
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<CoinMarketResponse>(resuul);
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
/* "id": 30021,
            "name": "Quark (Atomicals)",
            "symbol": "QUARK",
            "slug": "quark-atomicals",
            "is_active": 1,*/

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
}

public class SymbolInfo2
{
    public string Name { get; set; }
    public string Symbol { get; set; }
    public string Description { get; set; }

    public Dictionary<string, Pricing> Quote { get; set; }
}

public class Pricing
{
    public decimal Price { get; set; }
}

