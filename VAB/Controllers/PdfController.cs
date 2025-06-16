using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json.Linq;
using VAB.Models;
using VAB.Data;
using Microsoft.EntityFrameworkCore;

public class PdfController : Controller
{
    private readonly string apiKey = "[REDACTED]";
    private readonly ApplicationDbContext _context;

    public PdfController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> ExtractTable(IFormFile uploadedFile)
    {
        if (uploadedFile == null || uploadedFile.Length == 0)
        {
            TempData["Error"] = "Veuillez télécharger un fichier PDF ou image valide.";
            return RedirectToAction("Extract", "AgentSaisie");
        }

        try
        {
            // Upload file to PDF.co
            var uploadResult = await UploadFileToPdfCo(uploadedFile);
            if (!uploadResult.success)
            {
                TempData["Error"] = "Échec du téléchargement du fichier: " + uploadResult.error;
                return RedirectToAction("Extract", "AgentSaisie");
            }

            // Extract table from uploaded file URL
            var tableResult = await ExtractTableFromPdf(uploadResult.url);
            if (!tableResult.success)
            {
                TempData["Error"] = "Échec de l'extraction du tableau: " + tableResult.error;
                return RedirectToAction("Extract", "AgentSaisie");
            }

            // Parse the JSON and create a simple table structure
            var tableData = ParseJsonToTable(tableResult.data);

            // Store data in session instead of TempData to avoid header size issues
            HttpContext.Session.SetString("TableData", Newtonsoft.Json.JsonConvert.SerializeObject(tableData));
            HttpContext.Session.SetString("RawJson", tableResult.data);
            HttpContext.Session.SetString("FileName", uploadedFile.FileName);

            ViewBag.TableData = tableData;
            ViewBag.RawJson = tableResult.data;
            ViewBag.FileName = uploadedFile.FileName;

            return View("TableResult");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Une erreur s'est produite lors de l'extraction: " + ex.Message;
            return RedirectToAction("Extract", "AgentSaisie");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SaveExtractedData()
    {
        try
        {
            // Get the table data from Session
            var tableDataJson = HttpContext.Session.GetString("TableData");
            var rawJson = HttpContext.Session.GetString("RawJson");
            var fileName = HttpContext.Session.GetString("FileName");

            if (string.IsNullOrEmpty(tableDataJson) || string.IsNullOrEmpty(rawJson))
            {
                return Json(new { success = false, message = "Aucune donnée à sauvegarder. Veuillez extraire un nouveau fichier." });
            }

            var tableData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(tableDataJson);

            // Generate a unique number for this extraction
            var numero = GenerateUniqueNumber();
            var currentUser = User.Identity?.Name ?? "Utilisateur inconnu";

            // Create new ExtractedPdfData record
            var extractedData = new ExtractedPdfData
            {
                Numero = numero,
                FileName = fileName ?? "Fichier inconnu",
                DateExtraction = DateTime.Now,
                JsonData = rawJson,
                Description = $"Extrait de {fileName ?? "fichier"}",
                IsProcessed = false,
                ExtractedBy = currentUser,
                Notes = "Extraction simple de tableau"
            };

            _context.ExtractedPdfData.Add(extractedData);
            await _context.SaveChangesAsync();

            // Save table data as simple items
            var itemCount = await SaveTableItems(tableData, extractedData.Id, numero);

            // Clear session data after successful save
            HttpContext.Session.Remove("TableData");
            HttpContext.Session.Remove("RawJson");
            HttpContext.Session.Remove("FileName");

            return Json(new
            {
                success = true,
                message = "Données sauvegardées avec succès",
                numero = numero,
                id = extractedData.Id,
                itemCount = itemCount,
                extractedBy = currentUser
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur lors de la sauvegarde: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            return Json(new
            {
                success = false,
                message = "Une erreur s'est produite lors de la sauvegarde: " + ex.Message
            });
        }
    }

    private List<List<string>> ParseJsonToTable(string jsonData)
    {
        var table = new List<List<string>>();

        try
        {
            var jsonObject = JObject.Parse(jsonData);
            var rows = jsonObject["document"]?["page"]?["row"];

            if (rows != null)
            {
                JArray rowArray = rows is JArray ? (JArray)rows : new JArray(rows);

                foreach (JToken row in rowArray)
                {
                    var tableRow = new List<string>();
                    var columns = row["column"];

                    if (columns != null)
                    {
                        JArray columnArray = columns is JArray ? (JArray)columns : new JArray(columns);

                        foreach (JToken column in columnArray)
                        {
                            var text = GetTextFromColumn(column);
                            tableRow.Add(text);
                        }
                    }

                    table.Add(tableRow);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'analyse JSON: {ex.Message}");
        }

        return table;
    }

    private string GetTextFromColumn(JToken column)
    {
        try
        {
            var text = column?["text"];
            if (text == null) return "";

            if (text.Type == JTokenType.Object)
            {
                return text["#text"]?.ToString() ?? "";
            }
            else
            {
                return text.ToString();
            }
        }
        catch
        {
            return "";
        }
    }

    private async Task<int> SaveTableItems(List<List<string>> tableData, int extractedPdfDataId, string extractionNumber)
    {
        var items = new List<ExtractedPdfItem>();
        var itemCounter = 1;

        for (int rowIndex = 0; rowIndex < tableData.Count; rowIndex++)
        {
            var row = tableData[rowIndex];
            for (int colIndex = 0; colIndex < row.Count; colIndex++)
            {
                var cellText = row[colIndex];
                if (!string.IsNullOrWhiteSpace(cellText))
                {
                    // Generate a unique item code
                    var itemCode = GenerateItemCode(extractionNumber, itemCounter);

                    // Try to parse numeric values for prices and quantities
                    decimal unitPrice = 0;
                    decimal totalPrice = 0;
                    decimal quantity = 0;

                    // Simple parsing attempt - you can enhance this logic based on your data structure
                    if (decimal.TryParse(cellText.Replace(",", "."), out decimal numericValue))
                    {
                        // If it's a numeric value, assume it could be a price or quantity
                        if (numericValue > 0 && numericValue < 1000)
                        {
                            quantity = numericValue;
                        }
                        else if (numericValue >= 1000)
                        {
                            totalPrice = numericValue;
                            unitPrice = numericValue; // Set same value for now
                        }
                    }

                    var item = new ExtractedPdfItem
                    {
                        ExtractedPdfDataId = extractedPdfDataId,
                        ItemCode = itemCode, // Now properly set
                        ItemName = cellText.Length > 200 ? cellText.Substring(0, 200) : cellText,
                        ItemDescription = cellText,
                        RowNumber = rowIndex + 1,
                        ColumnNumber = colIndex + 1,
                        RawData = cellText,
                        Category = "Général",
                        UnitPrice = unitPrice,
                        TotalPrice = totalPrice,
                        Quantity = quantity
                    };

                    items.Add(item);
                    itemCounter++;
                }
            }
        }

        if (items.Any())
        {
            // Save in batches to avoid large transactions
            const int batchSize = 50;
            for (int i = 0; i < items.Count; i += batchSize)
            {
                var batch = items.Skip(i).Take(batchSize).ToList();
                _context.ExtractedPdfItems.AddRange(batch);
                await _context.SaveChangesAsync();
            }
        }

        return items.Count;
    }

    private string GenerateItemCode(string extractionNumber, int itemCounter)
    {
        // Generate a unique item code based on extraction number and item counter
        return $"{extractionNumber}-ITEM-{itemCounter:D4}";
    }

    private string GenerateUniqueNumber()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var random = new Random().Next(1000, 9999);
        return $"PDF-{timestamp}-{random}";
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
                tableDetection = true,
                outputFormat = "json"
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

    public async Task<IActionResult> ViewExtractions()
    {
        var extractions = await _context.ExtractedPdfData
            .Include(e => e.ExtractedItems)
            .OrderByDescending(e => e.DateExtraction)
            .ToListAsync();

        return View(extractions);
    }

    [HttpGet]
    public async Task<IActionResult> GetExtractionDetails(int id)
    {
        try
        {
            var extraction = await _context.ExtractedPdfData
                .Include(e => e.ExtractedItems)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extraction == null)
            {
                return NotFound();
            }

            // Parse the JSON to recreate the table
            var tableData = ParseJsonToTable(extraction.JsonData);
            ViewBag.TableData = tableData;
            ViewBag.Extraction = extraction;

            return PartialView("_ExtractionDetails", extraction);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la récupération des détails: {ex.Message}");
            return BadRequest("Erreur lors du chargement des détails");
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteExtraction(int id)
    {
        try
        {
            var extraction = await _context.ExtractedPdfData
                .Include(e => e.ExtractedItems)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (extraction == null)
            {
                return Json(new { success = false, message = "Extraction non trouvée" });
            }

            _context.ExtractedPdfData.Remove(extraction);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Extraction supprimée" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la suppression: {ex.Message}");
            return Json(new { success = false, message = "Une erreur s'est produite" });
        }
    }
}
