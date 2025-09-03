using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class ParsingResult
    {
        public bool Success { get; set; }
        public int TotalPlatforms { get; set; }
        public int TotalPaths { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public long MemoryUsedMB { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<AdvertisingPlatform> SamplePlatforms { get; set; } = new List<AdvertisingPlatform>();

        public string Summary =>
            $"Обработано {TotalPlatforms} площадок с {TotalPaths} путями за {ProcessingTime.TotalSeconds:F2} сек. " +
            $"Память: {MemoryUsedMB} MB. Ошибок: {Errors.Count}";
    }
}
