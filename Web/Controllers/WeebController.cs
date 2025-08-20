using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Collections.Generic;


namespace Web.Controllers
{
    public class AdvertisingPlatform
    {
        public string Name { get; set; }
        public List<string> Paths { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class WeebController : ControllerBase
    {
        private static List<AdvertisingPlatform> _platforms = new List<AdvertisingPlatform>();
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "advertising.txt");

        //вывод всех  данных
        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    return Ok(_platforms);
        //}

        //загрузка и перезапись данных
        [HttpPost("Загрузка данных")]
        public IActionResult UploadFromFile()
        {
            try
            {
                // Чтение всех строк из файла
                var lines = System.IO.File.ReadAllLines(_filePath);

                // Очищаем текущие данные
                _platforms = new List<AdvertisingPlatform>();

                // Обрабатываем каждую строку
                foreach (var line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        var platform = new AdvertisingPlatform
                        {
                            Name = parts[0].Trim(),
                            Paths = new List<string>(parts[1].Split(','))
                        };
                        _platforms.Add(platform);
                    }
                }

                return Ok(new
                {
                    Message = "Данные успешно загружены",
                    Platforms = _platforms
                });
            }
            catch (FileNotFoundException)
            {
                return NotFound("Файл advertising.txt не найден");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при обработке файла: {ex.Message}");
            }
        }
        //поиск
        [HttpGet("Поиск")]
        public IActionResult SearchPlatforms([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest("Параметр поиска не может быть пустым");
            }

            term = term.ToLower();

            var results = _platforms
                .Where(p => p.Name.ToLower().Contains(term) ||
                           p.Paths.Any(path => path.ToLower().Contains(term)))
                .ToList();

            if (!results.Any())
            {
                return NotFound("Результаты не найдены");
            }

            return Ok(results);
        }

    }
}

