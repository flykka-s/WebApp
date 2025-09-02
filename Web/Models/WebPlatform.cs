namespace Web.Models
{
    public class AdvertisingPlatform
    {
        public string Name { get; set; }
        public List<string> Paths { get; set; }
    }

    public class UploadRequest { 
    
        public IFormFile File { get; set; }
        public string FileUrl { get; set; }
    
    }
}