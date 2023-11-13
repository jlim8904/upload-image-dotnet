using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Upload.Models;
using System.Text.RegularExpressions;

namespace Upload.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage()
    {
        var file = Request.Form.Files[0];

        if (file.Length > 0)
        {
            var fileName = Path.GetFileName(file.FileName);
            ViewBag.FileName = file.FileName;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var fileNamePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{fileNameWithoutExtension}.txt");
            var fileBase64Path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{fileNameWithoutExtension}_base64.txt");

            System.IO.File.WriteAllText(fileNamePath, fileName);
        
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                var base64String = Convert.ToBase64String(imageBytes);
                System.IO.File.WriteAllText(fileBase64Path, base64String);

                ViewBag.ImageBase64 = $"data:image/png;base64,{base64String}";
            }

            _logger.LogInformation($"Information Image {fileName} uploaded successfully.");
        }
        
        return View("Upload");
    }

    [HttpPost]
    public IActionResult UploadBase64(string base64, string fileName)
    {
        if(string.IsNullOrEmpty(fileName))
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

        string fileNamePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{fileName}.jpeg");

        System.IO.File.WriteAllBytes(fileNamePath, Convert.FromBase64String(base64));
        
        return View("Upload");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
