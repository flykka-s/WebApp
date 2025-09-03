using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Web.Models;
using System.Text.Json;

namespace Web.Services
{
    public class AdvertisingService : IAdvertisingService
    {
        private readonly ConcurrentBag<AdvertisingPlatform> _platforms = new();
        private readonly ILogger<AdvertisingService> _logger;
        private ParsingResult _lastParsingResult = new();

        public AdvertisingService(ILogger<AdvertisingService> logger)
        {
            _logger = logger;
        }

        public async Task<ParsingResult> ProcessAdvertisingDataAsync(Stream stream)
        {
            var startTime = DateTime.UtcNow;
            var startMemory = GC.GetTotalMemory(false);

            var result = new ParsingResult();
            var platforms = new List<AdvertisingPlatform>();
            var lineNumber = 0;

            try
            {
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    lineNumber++;
                    var line = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var platform = JsonSerializer.Deserialize<AdvertisingPlatform>(line);
                        if (platform != null && !string.IsNullOrEmpty(platform.Name))
                        {
                            platforms.Add(platform);
                            result.TotalPaths += platform.Paths?.Count ?? 0;
                        }
                    }
                    catch (JsonException ex)
                    {
                        result.Errors.Add($"Ошибка JSON в строке {lineNumber}: {ex.Message}");
                    }
                }

                ReplaceAllData(platforms);

                result.Success = true;
                result.TotalPlatforms = platforms.Count;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                result.MemoryUsedMB = (GC.GetTotalMemory(false) - startMemory) / 1024 / 1024;
                result.SamplePlatforms = platforms.Take(5).ToList();

                _lastParsingResult = result;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Критическая ошибка: {ex.Message}");
                return result;
            }
        }

        public void ReplaceAllData(IEnumerable<AdvertisingPlatform> platforms)
        {
            var newCollection = new ConcurrentBag<AdvertisingPlatform>(platforms);

            // Очищаем и добавляем новые данные
            while (_platforms.TryTake(out _)) { }

            foreach (var platform in newCollection)
            {
                _platforms.Add(platform);
            }
        }

        public List<AdvertisingPlatform> GetAllPlatforms() => _platforms.ToList();

        public List<AdvertisingPlatform> SearchPlatforms(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AdvertisingPlatform>();

            return _platforms
                .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           p.Paths?.Any(path => path.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) == true)
                .ToList();
        }

        public ParsingResult GetLastParsingResult() => _lastParsingResult;
    }
}
