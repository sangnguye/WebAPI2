using System.ComponentModel.DataAnnotations;
using WebAPI.Models.Domain;

namespace WebAPI.Models.DTO
{
    public class AddBookRequestDTO
    {
        [Required(ErrorMessage = "Title is required")]
        [RegularExpression(@"^[\p{L}0-9\s]+$", ErrorMessage = "Title cannot contain special characters")]
        [MinLength(10, ErrorMessage = "Title must be at least 10 characters long")]
        public string? Title { get; set; }

        public string? Description { get; set; }
        public bool IsRead { get; set; }
        public DateTime? DateRead { get; set; }
        public int? Rate { get; set; }
        public string? Genre { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime DateAdded { get; set; }

        [Required(ErrorMessage = "PublisherId is required")]
        public int PublisherId { get; set; }

        // Bắt buộc có ít nhất 1 author
        [Required(ErrorMessage = "At least one author is required")]
        [MinLength(1, ErrorMessage = "At least one author is required")]
        public List<int> AuthorIds { get; set; } = new List<int>();
    }
}
