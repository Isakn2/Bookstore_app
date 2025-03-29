using BookStoreTester.Models;
using BookStoreTester.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.IO;
using System.Text;

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
        public IActionResult GetBooks([FromQuery] BookRequest request)
        {
            try
            {
                // Validate request
                if (request == null)
                {
                    return BadRequest("Request parameters cannot be null");
                }

                request.Page = Math.Max(1, request.Page);
                request.PageSize = Math.Clamp(request.PageSize, 1, 100);

                var books = _bookGenerator.GenerateBooks(request);
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating books");
                return StatusCode(500, "An error occurred while generating books");
            }
        }

        [HttpGet("export")]
        public IActionResult ExportBooks([FromQuery] BookRequest request, [FromQuery] int pages = 1)
        {
            try
            {
                pages = Math.Clamp(pages, 1, 100);
                var allBooks = new List<Book>();
                
                for (int i = 1; i <= pages; i++)
                {
                    request.Page = i;
                    allBooks.AddRange(_bookGenerator.GenerateBooks(request));
                }

                using var writer = new StringWriter();
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(allBooks);
                return File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", "books.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting books");
                return StatusCode(500, "An error occurred while exporting books");
            }
        }
    }
}