
namespace Web.Models
{
    public class HomeViewModel
    {
        public int TotalPlatforms { get; set; }
        public TimeSpan LastProcessingTime { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public class InfoViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Features { get; set; }
        public string ContactEmail { get; set; }
        public string Version { get; set; }
    }

    public class StatsViewModel
    {
        public int TotalPlatforms { get; set; }
        public int TotalPaths { get; set; }
        public ParsingResult LastParsingResult { get; set; }
        public List<string> PlatformNames { get; set; }
        public string MemoryUsage { get; set; }
    }
}
