using Microsoft.AspNetCore.Mvc;
using Upload.Models;
using PaddleOCRSharp;
using Newtonsoft.Json.Linq;

namespace Upload.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UploadApiController : Controller
{
    public readonly IWebHostEnvironment _environment;
    PaddleOCREngine? engine = null;

    public UploadApiController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost]
    public Task<Result> Post([FromForm] UploadApi imgFile)
    {
        Result result = new();
        string base64;
        try
        {
            if (imgFile.Files != null && imgFile.Files.Length > 0)
            {
                using MemoryStream ms = new();
                imgFile.Files.CopyTo(ms);
                base64 = Convert.ToBase64String(ms.ToArray());
                imgFile.ImgName = imgFile.Files.FileName;
            }
            else if (!string.IsNullOrEmpty(imgFile.Base64))
            {
                base64 = imgFile.Base64;
                if (string.IsNullOrEmpty(imgFile.ImgName))
                    imgFile.ImgName = DateTime.Now.ToString("yyyyMMddHHmmss");

                if (base64.StartsWith("data:image"))
                {
                    int commaIndex = base64.IndexOf(',');

                    if (commaIndex != -1)
                    {
                        base64 = base64[(commaIndex + 1)..];
                    }
                }
            }
            else
            {
                throw new Exception("Image Not Found!");
            }
            if (!(imgFile.ImgName.EndsWith(".png") || imgFile.ImgName.EndsWith(".jpg") || imgFile.ImgName.EndsWith(".jpeg")))
                imgFile.ImgName = $"{imgFile.ImgName}.jpeg";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imgFile.ImgName);
            System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(base64));
            engine ??= new PaddleOCREngine(null, new OCRParameter());
            OCRResult ocrResult = engine.DetectTextBase64(base64);
            JArray jsonArray = JArray.Parse(ocrResult.JsonText);
            List<string> results = new();
            foreach (JObject jsonObject in jsonArray)
            {
                if (jsonObject.TryGetValue("Text", out JToken valueToken))
                {
                    results.Add(valueToken.ToString());
                }
            }
            result.OCRResult = results;
        }
        catch (Exception)
        {
            throw;
        }
        return Task.FromResult(result);
    }
}
