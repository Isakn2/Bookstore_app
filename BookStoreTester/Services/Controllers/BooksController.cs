// BookController.cs
using BookStoreTester.Models;
using BookStoreTester.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BookStoreTester.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookGenerator _bookGenerator;
        private readonly ILogger<BooksController> _logger;

        public BooksController(BookGenerator bookGenerator, ILogger<BooksController> logger)
        {
            _bookGenerator = bookGenerator;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetBooks([FromQuery] BookRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid request parameters: {@Errors}", 
                    ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(new {
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                });
            }

            try
            {
                // Validate and normalize request parameters
                request.Page = Math.Max(1, request.Page);
                request.PageSize = Math.Clamp(request.PageSize, 1, 100);

                _logger.LogInformation("Generating books with request: {@Request}", request);
                var books = _bookGenerator.GenerateBooks(request);
                _logger.LogDebug("Generated {Count} books", books.Count);

                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating books for request: {@Request}", request);
                return StatusCode(500, new {
                    error = "An error occurred while generating books",
                    details = ex.Message
                });
            }
        }

        [HttpGet("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ExportBooks([FromQuery] BookRequest request, [FromQuery] int pages = 1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                pages = Math.Clamp(pages, 1, 100);
                var allBooks = new List<Book>();

                _logger.LogInformation("Exporting {Pages} pages of books", pages);
                
                for (int i = 1; i <= pages; i++)
                {
                    request.Page = i;
                    var books = _bookGenerator.GenerateBooks(request);
                    allBooks.AddRange(books);
                }

                using var writer = new StringWriter();
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(allBooks);
                
                _logger.LogInformation("Exported {Count} books to CSV", allBooks.Count);
                return File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", "books.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting books");
                return StatusCode(500, new {
                    error = "An error occurred while exporting books",
                    details = ex.Message
                });
            }
        }
    }
}