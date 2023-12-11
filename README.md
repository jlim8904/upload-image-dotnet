# 圖片辨識 API
---
PaddleOCRSharp 開源 [raoyutian/PaddleOCRSharp]

Nuget Package [NuGet Gallery | PaddleOCRSharp 2.0.0]

## Installation

Install the dependencies (PaddleOCRSharp).

```sh
dotnet add package PaddleOCRSharp --version 2.0.0
```

## Detection
Upload via (either one):
1. File
2. Base64 + Filename(not required)

### Web
- Upload 
![image](https://github.com/jlim8904/upload-image-dotnet/assets/92771562/5d8eb80b-fcc5-43c7-8f7c-367d129400b4)
- Result
![image](https://github.com/jlim8904/upload-image-dotnet/assets/92771562/5284528e-d180-43cf-bd4f-4be83b145acb)

- Code

Upload.cshtml (convert image to base64 if upload via file)
```js
function uploadImage() {
    var input = document.getElementById('image');
    var file = input.files[0];

    if (file) {
        var reader = new FileReader();
        reader.onload = function (e) {
            var base64String = e.target.result;
            var form = document.getElementById('imageForm');

            var hiddenInput = document.createElement('input');
            hiddenInput.type = 'hidden';
            hiddenInput.name = 'base64';
            hiddenInput.value = base64String;

            var filenameInput = document.createElement('input');
            filenameInput.type = 'hidden';
            filenameInput.name = 'filename';
            filenameInput.value = file.name;

            form.appendChild(hiddenInput);
            form.appendChild(filenameInput);
            form.submit();
        };
        reader.readAsDataURL(file);
    }
}
```
HomeController.cs
```cs
public IActionResult Result(string base64, string fileName)
{
    try {
        engine ??= new PaddleOCREngine(null, new OCRParameter());
        
        // Set filname as datetime now if filname is null.
        if (string.IsNullOrEmpty(fileName))
            fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
        
        // Unified base64 format
        if (base64.StartsWith("data:image"))
        {
            int commaIndex = base64.IndexOf(',');

            if (commaIndex != -1)
            {
                base64 = base64[(commaIndex + 1)..];
            }
        }
        
        // Add file extension
        if (!(fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg")))
            fileName = $"{fileName}.jpeg";

        ViewBag.ImageBase64 = $"data:image/png;base64,{base64}";
        ViewBag.FileName = fileName;

        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

        System.IO.File.WriteAllBytes(filePath, Convert.FromBase64String(base64));

        // OCR Detect
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
```


### API (Postman)
- Upload 
![image](https://github.com/jlim8904/upload-image-dotnet/assets/92771562/559489ff-9552-4e81-9fac-45b472be654d)
- Result
![image](https://github.com/jlim8904/upload-image-dotnet/assets/92771562/06e5ee52-7f8b-47b2-89d9-731eab5a4693)
- Code

UploadApiController.cs
```cs
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
```


   [raoyutian/PaddleOCRSharp]: https://github.com/raoyutian/PaddleOCRSharp
   [NuGet Gallery | PaddleOCRSharp 2.0.0]: https://www.nuget.org/packages/PaddleOCRSharp/2.0.0Dillinger
