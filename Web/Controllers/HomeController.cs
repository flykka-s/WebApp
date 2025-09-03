using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly IAdvertisingService _advertisingService;

        public HomeController(IAdvertisingService advertisingService)
        {
            _advertisingService = advertisingService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var stats = _advertisingService.GetLastParsingResult();
            var platforms = _advertisingService.GetAllPlatforms();

            var model = new HomeViewModel
            {
                TotalPlatforms = platforms.Count,
                LastProcessingTime = stats?.ProcessingTime ?? TimeSpan.Zero,
                LastUpdate = DateTime.Now
            };

            return View(model);
        }

        [HttpGet("info")]
        public IActionResult Info()
        {
            var model = new InfoViewModel
            {
                Title = "Информация о системе",
                Description = "Система для управления рекламными площадками",
                Features = new List<string>
                {
                    "Загрузка данных из JSON файлов",
                    "Потоковая обработка больших файлов",
                    "Поиск по площадкам и путям",
                    "Статистика обработки данных"
                },
                ContactEmail = "support@example.com",
                Version = "1.0.0"
            };

            return View(model);
        }

        [HttpGet("stats")]
        public IActionResult Statistics()
        {
            var stats = _advertisingService.GetLastParsingResult();
            var platforms = _advertisingService.GetAllPlatforms();

            var model = new StatsViewModel
            {
                TotalPlatforms = platforms.Count,
                TotalPaths = platforms.Sum(p => p.Paths?.Count ?? 0),
                LastParsingResult = stats,
                PlatformNames = platforms.Select(p => p.Name).ToList(),
                MemoryUsage = (GC.GetTotalMemory(false) / 1024 / 1024) + " MB"
            };

            return View(model);
        }
    }

}
