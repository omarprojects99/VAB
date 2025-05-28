using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json.Linq;

public class PdfController : Controller
{
    private readonly string apiKey = "[REDACTED]";

    [HttpPost]
    public async Task<IActionResult> ExtractTable(IFormFile uploadedFile)
    {
        if (uploadedFile == null || uploadedFile.Length == 0)
        {
            TempData["Error"] = "Please upload a valid PDF or image file.";
            return RedirectToAction("Index");
        }

        // Upload file to PDF.co
        var uploadResult = await UploadFileToPdfCo(uploadedFile);
        if (!uploadResult.success)
        {
            TempData["Error"] = "File upload failed: " + uploadResult.error;
            return RedirectToAction("Index");
        }

        // Extract table from uploaded file URL
        var tableResult = await ExtractTableFromPdf(uploadResult.url);
        if (!tableResult.success)
        {
            TempData["Error"] = "Table extraction failed: " + tableResult.error;
            return RedirectToAction("Index");
        }

        // Pass extracted table JSON to the view
        ViewBag.TableData = tableResult.data;
        return View("TableResult");
    }

    private async Task<(bool success, string url, string error)> UploadFileToPdfCo(IFormFile file)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            using (var content = new MultipartFormDataContent())
            using (var stream = file.OpenReadStream())
            {
                content.Add(new StreamContent(stream), "file", file.FileName);

                var response = await client.PostAsync("https://api.pdf.co/v1/file/upload", content);
                var json = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);

                if (obj["error"].Value<bool>() == false)
                {
                    return (true, obj["url"].Value<string>(), null);
                }
                else
                {
                    return (false, null, obj["message"].Value<string>());
                }
            }
        }
    }

    private async Task<(bool success, string data, string error)> ExtractTableFromPdf(string fileUrl)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);

            var payload = new
            {
                url = fileUrl,
                pages = "0-",
                inline = true,
                async = false,
                tableDetection = true, // Enable table detection
                outputFormat = "json" // Ensure JSON output
            };

            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.PostAsync("https://api.pdf.co/v1/pdf/convert/to/json", content);
            var json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            if (obj["error"].Value<bool>() == false)
            {
                var extractedData = obj["body"].ToString();
                return (true, extractedData, null);
            }
            else
            {
                return (false, null, obj["message"].Value<string>());
            }
        }
    }

   

    public IActionResult TableResult()
    {
        return View();
    }
}