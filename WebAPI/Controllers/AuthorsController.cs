using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Data;
using WebAPI.Models.DTO;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        public AuthorsController(IAuthorRepository authorRepository)
        {
            _authorRepository = authorRepository;
        }

        [HttpGet]
        public IActionResult GetAllAuthors()
        {
            return Ok(_authorRepository.GetAllAuthors());
        }

        [HttpGet("{id}")]
        public IActionResult GetAuthorById(int id)
        {
            var author = _authorRepository.GetAuthorById(id);
            if (author == null) return NotFound();
            return Ok(author);
        }

        [HttpPost]
        public IActionResult AddAuthor([FromBody]AddAuthorRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var added = _authorRepository.AddAuthor(dto);
            return CreatedAtAction(nameof(GetAuthorById), new { id = added }, added);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAuthorById(int id, [FromBody] AuthorNoIdDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = _authorRepository.UpdateAuthorById(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAuthorById(int id)
        {
            // kiểm tra tác giả tồn tại
            var author = _authorRepository.GetAuthorById(id);
            if (author == null) return NotFound();

            // kiểm tra còn sách liên kết không
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                bool hasBooks = dbContext.Books_Authors.Any(ba => ba.AuthorId == id);
                if (hasBooks)
                {
                    return Conflict(new
                    {
                        error = "AuthorInUse",
                        message = $"Cannot delete Author {id} because there are Books linked to it. Please remove links in Book_Author first."
                    });
                }
            }

            var deleted = _authorRepository.DeleteAuthorById(id);
            return Ok(new { message = $"Author {id} deleted successfully" });
        }
    }
}

