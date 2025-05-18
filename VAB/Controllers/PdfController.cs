//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using System.Threading.Tasks;
//using System.IO;
//using TextExtraction;
//using Microsoft.EntityFrameworkCore;
//using UglyToad.PdfPig;
//using VAB.Models;
//using Microsoft.AspNetCore.Identity;
//using VAB.Data;

//public class PdfController : Controller
//{
    
    
    
//    private readonly ApplicationDbContext _context;
//    public PdfController( ApplicationDbContext context)
//    {
       
//        _context = context;
//    }
//    private readonly PdfTextExtractor _pdfTextExtractor;

//    public PdfController(ILogger<PdfTextExtractor> logger)
//    {
//        _pdfTextExtractor = new PdfTextExtractor(logger);
//    }

//    [HttpGet]
//    public IActionResult Upload()
//    {
//        return View();
//    }
//    [HttpPost]
//    public async Task<IActionResult> Upload(IFormFile pdfFile)
//    {
//        if (pdfFile == null || pdfFile.Length == 0)
//        {
//            ModelState.AddModelError("", "Veuillez sélectionner un fichier PDF.");
//            return View("Index");
//        }

//        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
//        Directory.CreateDirectory(uploadPath);

//        var filePath = Path.Combine(uploadPath, pdfFile.FileName);

//        using (var stream = new FileStream(filePath, FileMode.Create))
//        {
//            await pdfFile.CopyToAsync(stream);
//        }

//        // Extract data from PDF
//        var etat = ExtractEtatDesVentesFromPdf(filePath);
//        if (etat != null)
//        {
//            _context.EtatDesVentesArrivee.Add(etat);
//            await _context.SaveChangesAsync();
//        }

//        return RedirectToAction("Index");
//    }
//    private EtatDesVentesArrivee ExtractEtatDesVentesFromPdf(string path)
//    {
//        using var pdf = PdfDocument.Open(path);
//        var etat = new EtatDesVentesArrivee
//        {
//            Numero = Guid.NewGuid().ToString().Substring(0, 8),
//            DateEdition = DateTime.Now,
//            EquipageId = 1, // you may set this dynamically
//            AgentSaisieId = "default",
//            ArticleVentes = new List<ArticleVente>()
//        };

//        foreach (var page in pdf.GetPages())
//        {
//            var lines = page.Text.Split('\n');
//            foreach (var line in lines)
//            {
//                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
//                if (parts.Length >= 3 && int.TryParse(parts[1], out int qty) && decimal.TryParse(parts[2], out decimal price))
//                {
//                    etat.ArticleVentes.Add(new ArticleVente
//                    {
//                        NomArticle = parts[0],
//                        Quantite = qty,
//                        PrixUnitaire = price
//                    });
//                }
//            }
//        }

//        etat.TotalEncaisse = etat.ArticleVentes.Sum(a => a.Total);
//        return etat;
//    }


//}
