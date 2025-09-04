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

        private readonly IAdvertisingService _advertisingService; // Приватное поле для сервиса работы с данными (внедрение зависимости)

        // Конструктор контроллера, принимающий зависимость IAdvertisingService
        public HomeController(IAdvertisingService advertisingService)
        {
            _advertisingService = advertisingService;
        }

        // HTTP GET метод для главной страницы
        [HttpGet]
        public IActionResult Index()
        {
            
            var stats = _advertisingService.GetLastParsingResult(); // Получение статистики последней обработки данных из сервиса
            var platforms = _advertisingService.GetAllPlatforms();  // Получение всех рекламных площадок из сервиса

            // Создание модели представления для главной страницы
            var model = new HomeViewModel
            {
                
                TotalPlatforms = platforms.Count,   // Установка общего количества площадок
                LastProcessingTime = stats?.ProcessingTime ?? TimeSpan.Zero,    // Установка времени последней обработки (если есть статистика) или TimeSpan.Zero
                LastUpdate = DateTime.Now   // Установка времени последнего обновления (текущее время)
            };
            // Возврат представления с созданной моделью
            return View(model);
        }


        // HTTP GET метод для страницы информации с указанием маршрута "info"
        [HttpGet("info")]
        public IActionResult Info()
        {
            // Создание модели представления для страницы информации
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


        // HTTP GET метод для страницы статистики с указанием маршрута "stats"
        [HttpGet("stats")]
        public IActionResult Statistics()
        {
           
            var stats = _advertisingService.GetLastParsingResult(); // Получение статистики последней обработки данных из сервиса
            var platforms = _advertisingService.GetAllPlatforms();  // Получение всех площадок из сервиса

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
