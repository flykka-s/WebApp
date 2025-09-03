using Web.Models;
namespace Web.Services
{
    public interface IAdvertisingService
    {
        Task<ParsingResult> ProcessAdvertisingDataAsync(Stream stream);
        List<AdvertisingPlatform> GetAllPlatforms();
        List<AdvertisingPlatform> SearchPlatforms(string searchTerm);
        void ReplaceAllData(IEnumerable<AdvertisingPlatform> platforms);
        ParsingResult GetLastParsingResult();
    }
}
