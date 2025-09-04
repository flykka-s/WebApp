using System.Text.Json;
using Web.Models;

namespace Web.Services
{
    //Класс для обработки JSON данных в потоковом режиме
    public class StreamingJsonProcessor
    {
        // Асинхронный метод, возвращающий асинхронную последовательность объектов AdvertisingPlatform
        public async IAsyncEnumerable<AdvertisingPlatform> ProcessStreamAsync(Stream stream)
        {

            using var reader = new StreamReader(stream);    //StreamReader для чтения данных из потока с автоматическим освобождением ресурсов (using)

            // Цикл чтения потока
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync(); // Асинхронное чтение одной строки из потока
                // Проверка, что строка не является null, пустой или состоящей только из пробельных символов
                if (!string.IsNullOrWhiteSpace(line))
                {

                    // Десериализация JSON строки в объект типа AdvertisingPlatform (преобразования JSON-данных в экземпляр класса)
                    var platform = JsonSerializer.Deserialize<AdvertisingPlatform>(line);
                    if (platform != null)
                    {
                        // Возврат объекта через yield return как части асинхронной последовательности
                        yield return platform;
                    }
                }
            }
        }
    }
}