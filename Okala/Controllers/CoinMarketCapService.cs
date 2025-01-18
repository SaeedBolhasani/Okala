using System.Text.Json;

namespace Okala.Controllers
{
    public class CoinMarketCapService
    {
        private readonly CoinMarketCapHttpClient _httpClient;

        private List<SymbolInfo> _symbols = [];

        private ReaderWriterLockSlim _lock = new();

        public CoinMarketCapService(CoinMarketCapHttpClient httpClient)
        {
            _httpClient = httpClient;
            Initiate().GetAwaiter().GetResult();
        }

        private async Task Initiate()
        {
            if (File.Exists("result.json"))
            {
                _symbols = JsonSerializer.Deserialize<List<SymbolInfo>>(File.ReadAllText("result.json"));
            }
            else
            {
                CoinMarketResponse<SymbolInfo[]> latest;
                var size = 5000;
                var start = 1;
                var result = new List<SymbolInfo>();
                do
                {
                    latest = await _httpClient.GetLatestQuotes(start, size);
                    start += size;
                    result.AddRange(latest.Data);

                } while (latest.Data.Length > 0);
                _lock.EnterWriteLock();
                _symbols.Clear();
                _symbols.AddRange(result);
                _lock.ExitWriteLock();

                File.WriteAllText("result.json", JsonSerializer.Serialize(result));
            }
        }

        public SymbolInfo[] Search(string symbol)
        {
            SymbolInfo[] result;

            _lock.EnterReadLock();
            result = _symbols
                .Where(i => i.Symbol.Contains(symbol, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(i => i.Symbol.Length)
                .ToArray();
            _lock.ExitReadLock();

            return result;
        }

        public async Task<object> Get(int id)=> await _httpClient.Get(id);
    }
}

