using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Upload.Models;
using PaddleOCRSharp;
using Newtonsoft.Json.Linq;

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
            if (!(fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg")))
                fileName = $"{fileName}.jpeg";

            ViewBag.ImageBase64 = $"data:image/png;base64,{base64}";
            ViewBag.FileName = fileName;

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(base64));

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
            ViewBag.OCRResult = string.Join("<br/>", results.ToArray());
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
