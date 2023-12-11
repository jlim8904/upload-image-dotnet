namespace Upload.Models
{
    public class UploadApi
    {
        public IFormFile? Files { get; set; }
        public string? Base64 { get; set; }
        public string? ImgName { get; set; }
    }

    public class Result
    {
        public List<string>? OCRResult { get; set; }
    }
}
