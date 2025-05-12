using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace TextExtraction
{
    /// <summary>
    /// Extracts text from PDF files, including native text and OCR-processed scanned images.
    /// Returns a list of TextSection objects containing extracted text and metadata.
    /// </summary>
    public class PdfTextExtractor
    {
        private readonly ILogger<PdfTextExtractor> _logger;
        private readonly string _tessdataPath;

        public PdfTextExtractor(ILogger<PdfTextExtractor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tessdataPath = @"C:\Users\MSI\Desktop\pfe\VAB\VAB\Helpers\tessdata";
            _logger.LogInformation($"Using Tesseract data path: {_tessdataPath}");
        }

        /// <summary>
        /// Extracts text from a PDF file asynchronously.
        /// </summary>
        /// <param name="filePath">Path to the PDF file.</param>
        /// <returns>A list of TextSection objects containing extracted text.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the PDF file does not exist.</exception>
        /// <exception cref="Exception">Thrown for other extraction errors.</exception>
        public async Task<List<TextSection>> ExtractAsync(string filePath)
        {
            _logger.LogInformation($"Starting text extraction from PDF: {filePath}");

            if (!File.Exists(filePath))
            {
                _logger.LogError($"PDF file not found: {filePath}");
                throw new FileNotFoundException("PDF file not found", filePath);
            }

            var textSections = new List<TextSection>();

            try
            {
                using (PdfDocument document = PdfDocument.Open(filePath))
                {
                    for (int i = 0; i < document.NumberOfPages; i++)
                    {
                        var page = document.GetPage(i + 1);
                        int pageNumber = i + 1;

                        // Extract native text
                        if (!string.IsNullOrEmpty(page.Text))
                        {
                            textSections.Add(CreateNativeTextSection(page.Text, pageNumber));
                        }

                        // Extract text from images via OCR
                        var images = page.GetImages();
                        if (images?.Any() == true)
                        {
                            var ocrSections = await ExtractTextFromImagesAsync(images, pageNumber);
                            textSections.AddRange(ocrSections);
                        }
                    }
                }

                _logger.LogInformation($"Successfully extracted {textSections.Count} text sections from {filePath}");
                return textSections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to extract text from PDF: {filePath}");
                throw;
            }
        }

        private TextSection CreateNativeTextSection(string text, int pageNumber)
        {
            return new TextSection
            {
                Title = $"Page {pageNumber} - Native Text",
                Content = text,
                Type = "Text",
                Number = pageNumber
            };
        }

        private async Task<List<TextSection>> ExtractTextFromImagesAsync(IEnumerable<IPdfImage> images, int pageNumber)
        {
            var sections = new List<TextSection>();
            int imageIndex = 0;

            foreach (var image in images)
            {
                imageIndex++;
                if (image.RawBytes.IsEmpty || !IsValidImage(image.RawBytes.ToArray()))
                {
                    _logger.LogWarning($"Page {pageNumber} Image {imageIndex}: Invalid or unsupported image format");
                    continue;
                }

                try
                {
                    using (var ms = new MemoryStream(image.RawBytes.ToArray()))
                    using (var bitmap = new Bitmap(ms))
                    using (var processedBitmap = PreprocessImage(bitmap))
                    {
                        string ocrText = await Task.Run(() => PerformOcr(processedBitmap));
                        if (!string.IsNullOrWhiteSpace(ocrText))
                        {
                            sections.Add(new TextSection
                            {
                                Title = $"Page {pageNumber} - OCR Image {imageIndex}",
                                Content = ocrText,
                                Type = "scannedText",
                                Number = pageNumber
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing image {imageIndex} on page {pageNumber}");
                }
            }

            return sections;
        }

        private string PerformOcr(Bitmap bitmap)
        {
            try
            {
                if (!Directory.Exists(_tessdataPath))
                {
                    _logger.LogError($"Tesseract data directory not found: {_tessdataPath}");
                    throw new DirectoryNotFoundException("Tesseract data directory not found");
                }

                using (var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_pageseg_mode", "PSM_AUTO");
                    using (var ms = new MemoryStream())
                    {
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        using (var pix = Pix.LoadFromMemory(ms.ToArray()))
                        using (var page = engine.Process(pix))
                        {
                            return page.GetText();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR processing failed");
                return string.Empty;
            }
        }

        private Bitmap PreprocessImage(Bitmap original)
        {
            try
            {
                var grayImage = new Bitmap(original.Width, original.Height);
                for (int y = 0; y < original.Height; y++)
                {
                    for (int x = 0; x < original.Width; x++)
                    {
                        Color pixel = original.GetPixel(x, y);
                        int grayScale = (int)((pixel.R * 0.3) + (pixel.G * 0.59) + (pixel.B * 0.11));
                        grayImage.SetPixel(x, y, Color.FromArgb(grayScale, grayScale, grayScale));
                    }
                }
                return grayImage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image preprocessing failed");
                return original;
            }
        }

        private bool IsValidImage(byte[] bytes)
        {
            if (bytes.Length < 4) return false;

            // Check for common image format signatures
            return (bytes[0] == 0xFF && bytes[1] == 0xD8) || // JPEG
                   (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) || // PNG
                   (bytes[0] == 0x42 && bytes[1] == 0x4D) || // BMP
                   (bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00) || // TIFF
                   (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A) || // TIFF
                   (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38) || // GIF
                   (bytes[0] == 0x97 && bytes[1] == 0x4A && bytes[2] == 0x42 && bytes[3] == 0x32); // JBIG2
        }
    }

  
    public class TextSection
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public int Number { get; set; }
    }
}