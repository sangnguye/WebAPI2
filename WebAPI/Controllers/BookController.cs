using Microsoft.AspNetCore.Mvc;
using WebAPI.CustomActionFilter;
using WebAPI.Data;
using WebAPI.Models.DTO;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IBookRepository _bookRepository;

        public BooksController(AppDbContext dbContext, IBookRepository bookRepository)
        {
            _dbContext = dbContext;
            _bookRepository = bookRepository;
        }

        // GET: api/books/get-all-books
        [HttpGet("get-all-books")]
        public IActionResult GetAll()
        {
            var allBooks = _bookRepository.GetAllBooks();
            return Ok(allBooks);
        }

        // GET: api/books/get-book-by-id/{id}
        [HttpGet("get-book-by-id/{id}")]
        public IActionResult GetBookById([FromRoute] int id)
        {
            var bookWithIdDTO = _bookRepository.GetBookById(id);
            if (bookWithIdDTO == null)
                return NotFound($"Book with id {id} not found");

            return Ok(bookWithIdDTO);
        }

        // POST: api/books/add-book
        [HttpPost("add-book")]
        [ValidateModel]
        public IActionResult AddBook([FromBody] AddBookRequestDTO addBookRequestDTO)
        {
            if (addBookRequestDTO == null)
                return BadRequest("Book data is required");

            // Validate Publisher
            if (!ValidatePublisher(addBookRequestDTO.PublisherId))
                return BadRequest(ModelState);

            // Validate các rule khác
            if (!ValidateAddBook(addBookRequestDTO))
                return BadRequest(ModelState);

            var bookAdd = _bookRepository.AddBook(addBookRequestDTO);
            return Ok(bookAdd);
        }

        // PUT: api/books/update-book-by-id/{id}
        [HttpPut("update-book-by-id/{id}")]
        public IActionResult UpdateBookById(int id, [FromBody] AddBookRequestDTO bookDTO)
        {
            if (bookDTO == null)
                return BadRequest("Book data is required");

            // Validate Publisher
            if (!ValidatePublisher(bookDTO.PublisherId))
                return BadRequest(ModelState);

            // Validate các rule khác
            if (!ValidateAddBook(bookDTO))
                return BadRequest(ModelState);

            var updateBook = _bookRepository.UpdateBookById(id, bookDTO);
            if (updateBook == null)
                return NotFound($"Book with id {id} not found");

            return Ok(updateBook);
        }

        // DELETE: api/books/delete-book-by-id/{id}
        [HttpDelete("delete-book-by-id/{id}")]
        public IActionResult DeleteBookById(int id)
        {
            var deleteBook = _bookRepository.DeleteBookById(id);
            if (deleteBook == null)
                return NotFound($"Book with id {id} not found");

            return Ok(deleteBook);
        }
        #region Private methods 

        private bool ValidatePublisher(int publisherId)
        {
            if (!_dbContext.Publishers.Any(p => p.Id == publisherId))
            {
                ModelState.AddModelError("PublisherId", $"PublisherId {publisherId} does not exist in Publishers table");
                return false;
            }
            return true;
        }

        private bool ValidateAddBook(AddBookRequestDTO addBookRequestDTO)
        {
            if (string.IsNullOrEmpty(addBookRequestDTO.Description))
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Description), $"{nameof(addBookRequestDTO.Description)} cannot be null");
            }

            if (addBookRequestDTO.Rate < 0 || addBookRequestDTO.Rate > 5)
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.Rate), $"{nameof(addBookRequestDTO.Rate)} must be between 0 and 5");
            }

            // 🔹 Kiểm tra AuthorIds
            if (addBookRequestDTO.AuthorIds == null || !addBookRequestDTO.AuthorIds.Any())
            {
                ModelState.AddModelError(nameof(addBookRequestDTO.AuthorIds), "Book must have at least one author.");
            }
            else
            {
                // Kiểm tra AuthorId có tồn tại trong bảng Authors không
                var validAuthorCount = _dbContext.Authors
                    .Count(a => addBookRequestDTO.AuthorIds.Contains(a.Id));

                if (validAuthorCount != addBookRequestDTO.AuthorIds.Distinct().Count())
                {
                    ModelState.AddModelError(nameof(addBookRequestDTO.AuthorIds), "One or more authors do not exist in Authors table.");
                }
            }

            return ModelState.ErrorCount == 0;
        }
        #endregion
    }
}

