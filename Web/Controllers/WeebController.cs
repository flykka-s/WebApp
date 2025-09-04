using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Web.Models;



namespace Web.Controllers
{
    
    [ApiController]              // Атрибут, указывающий что это API контроллер
    [Route("api/[controller]")]  // Базовый маршрут для всех методов контроллера
    public class AdvertisingController : ControllerBase
    {
        
        private static ConcurrentBag<AdvertisingPlatform> _platforms = new ConcurrentBag<AdvertisingPlatform>(); // Коллекция для хранения данных в памяти
        private readonly IHttpClientFactory _httpClientFactory; // Фабрика для создания HTTP клиентов
        private readonly ILogger<AdvertisingController> _logger; // Логгер для записи событий и ошибок

        // Конструктор с внедрением зависимостей
        public AdvertisingController(IHttpClientFactory httpClientFactory, ILogger<AdvertisingController> logger)
        {
            // Инициализация фабрики и логгера
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        //Получение всего
        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    return Ok(_platforms.ToList());
        //}

        // HTTP POST метод для загрузки данных из сети или файла
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFromNetwork([FromForm] UploadRequest request)
        {
            try
            {
                // Проверка наличия файла или URL
                if (request.File == null && string.IsNullOrEmpty(request.FileUrl))
                {
                    return BadRequest("Необходимо предоставить файл или URL");
                }

                
                Stream stream; // Переменная для потока данных

                // Обработка случая с загрузкой файла 
                if (request.File != null)  
                {
                    stream = request.File.OpenReadStream(); // Открытие потока для чтения файла
                }
                else
                {

                    var httpClient = _httpClientFactory.CreateClient(); // Создание HTTP клиента для загрузки по URL
                    var response = await httpClient.GetAsync(request.FileUrl, HttpCompletionOption.ResponseHeadersRead); // Асинхронная загрузка файла по URL с чтением только заголовков

                    if (!response.IsSuccessStatusCode) // Проверка успешности HTTP запроса
                    {
                        return BadRequest($"Не удалось загрузить файл по URL: {response.StatusCode}"); //error
                    }

                    stream = await response.Content.ReadAsStreamAsync();  // Чтение содержимого ответа как потока
                }

                var platforms = await ProcessStreamAsync(stream); // Обработка потока данных и получение списка платформ

                // Атомарная замена данных
                _platforms = new ConcurrentBag<AdvertisingPlatform>(platforms);

                // Возврат успешного результата с данными
                return Ok(new
                {
                    Message = "Данные успешно загружены",
                    Count = platforms.Count, // Количество загруженных записей
                    Platforms = platforms.Take(10) // Возвращаем первые 10 для preview
                });
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Ошибка при загрузке данных"); // Логирование ошибки
                return StatusCode(500, $"Ошибка при обработке файла: {ex.Message}"); // Возврат ошибки сервера
            }
        }

        // Приватный метод для потоковой обработки данных
        private async Task<List<AdvertisingPlatform>> ProcessStreamAsync(Stream stream)
        {
            var platforms = new List<AdvertisingPlatform>(); //Список для результатов

            try
            {
                using var reader = new StreamReader(stream);  // Создание потокового читателя

                // Чтение потока построчно до конца
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(); // Асинхронное чтение строки
                    if (!string.IsNullOrWhiteSpace(line)) // Пропуск пустых строк
                    {
                        try
                        {
                            var platform = JsonSerializer.Deserialize<AdvertisingPlatform>(line); // Десериализация JSON строки в объект AdvertisingPlatform
                            // Добавление в список если десериализация успешна
                            if (platform != null)
                            {
                                platforms.Add(platform);
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning($"Ошибка десериализации строки: {line}. Error: {ex.Message}");  // Логирование предупреждения о ошибке десериализации
                        }
                    }

                    // Ограничение памяти - обрабатываем порциями (регулирование нагрузки на память - пауза каждые 1000 записей)
                    if (platforms.Count % 1000 == 0)
                    {
                        await Task.Delay(1); // Даем потоку передышку
                        _logger.LogInformation($"Обработано {platforms.Count} записей");   // Логирование прогресса обработки
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при потоковой обработке JSON");
                throw;
            }

            return platforms;   // Возврат обработанного списка платформ
        }

        [HttpGet("search")]  // HTTP GET метод для поиска платформ
        public IActionResult SearchPlatforms([FromQuery] string term)
        {   
            // Проверка что поисковый запрос не пустой
            if (string.IsNullOrWhiteSpace(term))  
            {
                return BadRequest("Параметр поиска не может быть пустым");
            }

            // Поиск платформ по имени или путям (без учета регистра)
            var results = _platforms
                .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                           p.Paths.Any(path => path.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            
            // Проверка наличия результатов поиска
            if (!results.Any())
            {
                return NotFound("Площадки по заданному критерию не найдены");
            }
            
            
            return Ok(results); // Возврат найденных результатов
        }
    }
}









// old ЧТЕНИЕЕ ИЗ ФАЙЛА txt


//    [ApiController]
//    [Route("[controller]")]
//    public class WeebController : ControllerBase
//    {
//        private static List<AdvertisingPlatform> _platforms = new List<AdvertisingPlatform>();
//        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "advertising.txt");

//        //вывод всех  данных
//        //[HttpGet]
//        //public IActionResult GetAll()
//        //{
//        //    return Ok(_platforms);
//        //}

//        //загрузка и перезапись данных
//        [HttpPost("Загрузка данных")]
//        public IActionResult UploadFromFile()
//        {
//            try
//            {
//                // Чтение всех строк из файла
//                var lines = System.IO.File.ReadAllLines(_filePath);

//                // Очищаем текущие данные
//                _platforms = new List<AdvertisingPlatform>();

//                // Обрабатываем каждую строку
//                foreach (var line in lines)
//                {
//                    var parts = line.Split(':');
//                    if (parts.Length == 2)
//                    {
//                        var platform = new AdvertisingPlatform
//                        {
//                            Name = parts[0].Trim(),
//                            Paths = new List<string>(parts[1].Split(','))
//                        };
//                        _platforms.Add(platform);
//                    }
//                }

//                return Ok(new
//                {
//                    Message = "Данные успешно загружены",
//                    Platforms = _platforms
//                });
//            }
//            catch (FileNotFoundException)
//            {
//                return NotFound("Файл advertising.txt не найден");
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Ошибка при обработке файла: {ex.Message}");
//            }
//        }
//        //поиск
//        [HttpGet("Поиск")]
//        public IActionResult SearchPlatforms([FromQuery] string term)
//        {
//            if (string.IsNullOrWhiteSpace(term))
//            {
//                return BadRequest("Параметр поиска не может быть пустым");
//            }

//            term = term.ToLower();

//            var results = _platforms
//                .Where(p => p.Name.ToLower().Contains(term) ||
//                           p.Paths.Any(path => path.ToLower().Contains(term)))
//                .ToList();

//            if (!results.Any())
//            {
//                return NotFound("Результаты не найдены");
//            }

//            return Ok(results);
//        }

//    }
//}

