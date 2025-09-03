using System.Text.Json;
using Web.Models;

namespace Web.Services
{
    public class StreamingJsonProcessor
    {
        public async IAsyncEnumerable<AdvertisingPlatform> ProcessStreamAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var platform = JsonSerializer.Deserialize<AdvertisingPlatform>(line);
                    if (platform != null)
                    {
                        yield return platform;
                    }
                }
            }
        }
    }
}