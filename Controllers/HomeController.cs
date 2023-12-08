using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Upload.Models;
using PaddleOCRSharp;

namespace Upload.Controllers;

public class HomeController : Controller
{
    PaddleOCREngine? engine = null;

    public IActionResult Upload()
    {
        return View();
    }

    [AcceptVerbs("Get", "Post")]
    public IActionResult Result(string base64, string fileName)
    {
        try {
            engine ??= new PaddleOCREngine(null, new OCRParameter());
            if (string.IsNullOrEmpty(fileName))
                fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (base64.StartsWith("data:image"))
            {
                int commaIndex = base64.IndexOf(',');

                if (commaIndex != -1)
                {
                    base64 = base64[(commaIndex + 1)..];
                }
            }

            ViewBag.ImageBase64 = $"data:image/png;base64,{base64}";
            ViewBag.FileName = $"{fileName}.jpeg";

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", $"{fileName}.jpeg");

            System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(base64));

            OCRResult ocrResult = engine.DetectTextBase64(base64);
            ViewBag.OCRResult = ocrResult.Text;
            return View();
        }
        catch (Exception)
        {
            return RedirectToAction("Upload");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
