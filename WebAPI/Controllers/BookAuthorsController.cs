using Microsoft.AspNetCore.Mvc;
using WebAPI.Data;
using WebAPI.Models.Domain;
using WebAPI.Models.DTO;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookAuthorsController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public BookAuthorsController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("add-book-author")]
        public IActionResult AddBookAuthor([FromBody] AddBookAuthorRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // kiểm tra BookId tồn tại
            if (!_dbContext.Books.Any(b => b.Id == dto.BookId))
            {
                ModelState.AddModelError(nameof(dto.BookId), $"BookId {dto.BookId} does not exist");
                return BadRequest(ModelState);
            }

            // kiểm tra AuthorId tồn tại
            if (!_dbContext.Authors.Any(a => a.Id == dto.AuthorId))
            {
                ModelState.AddModelError(nameof(dto.AuthorId), $"AuthorId {dto.AuthorId} does not exist");
                return BadRequest(ModelState);
            }

            // kiểm tra trùng lặp (bài tập 6)
            if (_dbContext.Books_Authors.Any(ba => ba.BookId == dto.BookId && ba.AuthorId == dto.AuthorId))
            {
                return Conflict(new
                {
                    error = "DuplicateRelation",
                    message = $"AuthorId {dto.AuthorId} has already been assigned to BookId {dto.BookId}"
                });
            }

            var bookAuthor = new Book_Author
            {
                BookId = dto.BookId,
                AuthorId = dto.AuthorId
            };

            _dbContext.Books_Authors.Add(bookAuthor);
            _dbContext.SaveChanges();

            return Ok(bookAuthor);
        }
    }
}
