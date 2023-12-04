using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Upload.Models;
using PaddleOCRSharp;

namespace Upload.Controllers;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    PaddleOCREngine? engine = null;

    public HomeController(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public IActionResult UploadImage()
    {
        var file = Request.Form.Files[0];

        if (file.Length > 0)
        {
            engine ??= new PaddleOCREngine(null, new OCRParameter());
            var fileName = Path.GetFileName(file.FileName);
            ViewBag.FileName = file.FileName;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
            var fileBase64Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{fileNameWithoutExtension}_base64.txt");


            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var imageBytes = ms.ToArray();
            var base64String = Convert.ToBase64String(imageBytes);
            System.IO.File.WriteAllText(fileBase64Path, base64String);
            System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(base64String));

            OCRResult ocrResult = engine.DetectText(filePath);
            ViewBag.OCRResult = ocrResult.Text;

            ViewBag.ImageBase64 = $"data:image/png;base64,{base64String}";
        }

        return View("Upload");
    }

    [HttpPost]
    public IActionResult UploadBase64(string base64, string fileName)
    {
        engine ??= new PaddleOCREngine(null, new OCRParameter());
        if (string.IsNullOrEmpty(fileName))
            fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

        if (base64.StartsWith("data:image"))
        {
            int commaIndex = base64.IndexOf(',');

            if (commaIndex != -1)
            {
                base64 = base64.Substring(commaIndex + 1);
            }
        }

        ViewBag.ImageBase64 = $"data:image/png;base64,{base64}";
        ViewBag.FileName = $"{fileName}.jpeg";

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{fileName}.jpeg");

        System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(base64));

        OCRResult ocrResult = engine.DetectText(filePath);
        ViewBag.OCRResult = ocrResult.Text;

        return View("Upload");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
