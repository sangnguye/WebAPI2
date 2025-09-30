using WebAPI.Models.Domain;
using WebAPI.Models.DTO;

namespace WebAPI.Repositories
{
    public interface IBookRepository
    {
        List<BookWithAuthorAndPublisherDTO> GetAllBooks();
        BookWithAuthorAndPublisherDTO? GetBookById(int id);

        // Khi thêm mới trả về BookWithAuthorAndPublisherDTO để xem cả Authors + Publisher
        BookWithAuthorAndPublisherDTO AddBook(AddBookRequestDTO addBookRequestDTO);

        // Khi update cũng trả về BookWithAuthorAndPublisherDTO
        BookWithAuthorAndPublisherDTO? UpdateBookById(int id, AddBookRequestDTO bookDTO);

        Book? DeleteBookById(int id);
    }
}
