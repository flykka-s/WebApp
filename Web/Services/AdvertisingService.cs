using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Web.Models;
using System.Text.Json;

namespace Web.Services
{
    public class AdvertisingService : IAdvertisingService
    {
        private readonly ConcurrentBag<AdvertisingPlatform> _platforms = new(); // Потокобезопасная коллекция для хранения данных
        private readonly ILogger<AdvertisingService> _logger;   // Логгер для записи событий и ошибок
        private ParsingResult _lastParsingResult = new(); // Переменная для хранения результатов последнего парсинга

        // Конструктор класса с внедрением зависимостей
        public AdvertisingService(ILogger<AdvertisingService> logger)
        {
            _logger = logger;
        }

        // Асинхронный метод обработки рекламных данных из потока
        public async Task<ParsingResult> ProcessAdvertisingDataAsync(Stream stream)
        {
            var startTime = DateTime.UtcNow; // Запись времени начала обработки
            var startMemory = GC.GetTotalMemory(false); // Запись используемой памяти до начала обработки

            var result = new ParsingResult();   // Создание объекта для хранения результатов парсинга 
            var platforms = new List<AdvertisingPlatform>();    // Временный список для хранения обработанных платформ
            var lineNumber = 0; // Счетчик строк для отслеживания прогресса и ошибок

            try
            {
                using var reader = new StreamReader(stream);    // Создание читателя потока данных

                while (!reader.EndOfStream)
                {
                    lineNumber++;   // Увеличение счетчика строк
                    var line = await reader.ReadLineAsync(); // Асинхронное чтение строки

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        var platform = JsonSerializer.Deserialize<AdvertisingPlatform>(line);   // Десериализация JSON строки в объект AdvertisingPlatform
                        // Проверка валидности объекта
                        if (platform != null && !string.IsNullOrEmpty(platform.Name))
                        {
                            platforms.Add(platform);    // Добавление платформы в временный список
                            result.TotalPaths += platform.Paths?.Count ?? 0;    // Подсчет общего количества путей
                        }
                    }
                    catch (JsonException ex)
                    {
                        result.Errors.Add($"Ошибка JSON в строке {lineNumber}: {ex.Message}");
                    }
                }

                ReplaceAllData(platforms);  // Замена всех данных в основной коллекции

                result.Success = true;  // Установка флага успешного выполнения
                result.TotalPlatforms = platforms.Count;    // Запись общего количества платформ
                result.ProcessingTime = DateTime.UtcNow - startTime;    // Расчет времени обработки
                result.MemoryUsedMB = (GC.GetTotalMemory(false) - startMemory) / 1024 / 1024;   // Расчет использованной памяти в мегабайтах
                result.SamplePlatforms = platforms.Take(5).ToList();    // Сохранение первых 5 платформ как образца

                _lastParsingResult = result;    // Сохранение результатов последнего парсинга

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Критическая ошибка: {ex.Message}");
                return result;
            }
        }

        // Метод для полной замены данных в коллекции
        public void ReplaceAllData(IEnumerable<AdvertisingPlatform> platforms)
        {
            var newCollection = new ConcurrentBag<AdvertisingPlatform>(platforms);  // Создание новой потокобезопасной коллекции

            // Очищаем и добавляем новые данные
            while (_platforms.TryTake(out _)) { }

            // Добавление всех новых элементов в коллекцию
            foreach (var platform in newCollection)
            {
                _platforms.Add(platform);
            }
        }


        public List<AdvertisingPlatform> GetAllPlatforms() => _platforms.ToList();  // Метод для получения всех платформ в виде списка

        // Метод поиска платформ по поисковому запросу
        public List<AdvertisingPlatform> SearchPlatforms(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<AdvertisingPlatform>(); // Возврат пустого списка при пустом запросе


            // Поиск платформ по имени или путям
            return _platforms
                .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           p.Paths?.Any(path => path.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) == true)
                .ToList();
        }

        // Метод для получения результатов последнего парсинга
        public ParsingResult GetLastParsingResult() => _lastParsingResult;
    }
}
