using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Web.Models;



namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisingController : ControllerBase
    {
        private static ConcurrentBag<AdvertisingPlatform> _platforms = new ConcurrentBag<AdvertisingPlatform>();
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AdvertisingController> _logger;

        public AdvertisingController(IHttpClientFactory httpClientFactory, ILogger<AdvertisingController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }


        //получение всего
        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    return Ok(_platforms.ToList());
        //}

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFromNetwork([FromForm] UploadRequest request)
        {
            try
            {
                if (request.File == null && string.IsNullOrEmpty(request.FileUrl))
                {
                    return BadRequest("Необходимо предоставить файл или URL");
                }

                Stream stream;
                if (request.File != null)
                {
                    stream = request.File.OpenReadStream();
                }
                else
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync(request.FileUrl, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        return BadRequest($"Не удалось загрузить файл по URL: {response.StatusCode}");
                    }

                    stream = await response.Content.ReadAsStreamAsync();
                }

                var platforms = await ProcessStreamAsync(stream);

                // Атомарная замена данных
                _platforms = new ConcurrentBag<AdvertisingPlatform>(platforms);

                return Ok(new
                {
                    Message = "Данные успешно загружены",
                    Count = platforms.Count,
                    Platforms = platforms.Take(10) // Возвращаем первые 10 для preview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке данных");
                return StatusCode(500, $"Ошибка при обработке файла: {ex.Message}");
            }
        }

        private async Task<List<AdvertisingPlatform>> ProcessStreamAsync(Stream stream)
        {
            var platforms = new List<AdvertisingPlatform>();

            try
            {
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            var platform = JsonSerializer.Deserialize<AdvertisingPlatform>(line);
                            if (platform != null)
                            {
                                platforms.Add(platform);
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning($"Ошибка десериализации строки: {line}. Error: {ex.Message}");
                        }
                    }

                    // Ограничение памяти - обрабатываем порциями
                    if (platforms.Count % 1000 == 0)
                    {
                        await Task.Delay(1); // Даем потоку передышку
                        _logger.LogInformation($"Обработано {platforms.Count} записей");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при потоковой обработке JSON");
                throw;
            }

            return platforms;
        }

        [HttpGet("search")]
        public IActionResult SearchPlatforms([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Параметр поиска не может быть пустым");
            }

            var results = _platforms
                .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                           p.Paths.Any(path => path.Contains(term, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!results.Any())
            {
                return NotFound("Площадки по заданному критерию не найдены");
            }

            return Ok(results);
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

