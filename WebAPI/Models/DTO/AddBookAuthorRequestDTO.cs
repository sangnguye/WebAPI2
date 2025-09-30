using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.DTO
{
    public class AddBookAuthorRequestDTO
    {
        [Required(ErrorMessage = "BookId is required")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "AuthorId is required")]
        public int AuthorId { get; set; }
    }
}
