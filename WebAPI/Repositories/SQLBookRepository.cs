using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models.Domain;
using WebAPI.Models.DTO;

namespace WebAPI.Repositories
{
    public class SQLBookRepository : IBookRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLBookRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET ALL
        public List<BookWithAuthorAndPublisherDTO> GetAllBooks()
        {
            return _dbContext.Books
                .Select(book => new BookWithAuthorAndPublisherDTO()
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.IsRead ? book.DateRead.Value : null,
                    Rate = book.IsRead ? book.Rate.Value : null,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors
                                      .Select(ba => ba.Author.FullName)
                                      .ToList()
                })
                .ToList();
        }

        // GET BY ID
        public BookWithAuthorAndPublisherDTO? GetBookById(int id)
        {
            return _dbContext.Books
                .Where(b => b.Id == id)
                .Select(book => new BookWithAuthorAndPublisherDTO()
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.DateRead,
                    Rate = book.Rate,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors
                                      .Select(ba => ba.Author.FullName)
                                      .ToList()
                })
                .FirstOrDefault();
        }

        // ADD
        public BookWithAuthorAndPublisherDTO AddBook(AddBookRequestDTO addBookRequestDTO)
        {
            var bookDomainModel = new Book
            {
                Title = addBookRequestDTO.Title,
                Description = addBookRequestDTO.Description,
                IsRead = addBookRequestDTO.IsRead,
                DateRead = addBookRequestDTO.DateRead,
                Rate = addBookRequestDTO.Rate,
                Genre = addBookRequestDTO.Genre,
                CoverUrl = addBookRequestDTO.CoverUrl,
                DateAdded = addBookRequestDTO.DateAdded,
                PublisherId = addBookRequestDTO.PublisherId
            };

            _dbContext.Books.Add(bookDomainModel);
            _dbContext.SaveChanges();

            // thêm Authors (AddRange để tiết kiệm SaveChanges)
            var bookAuthors = addBookRequestDTO.AuthorIds
                .Distinct()
                .Select(aid => new Book_Author
                {
                    BookId = bookDomainModel.Id,
                    AuthorId = aid
                }).ToList();

            _dbContext.Books_Authors.AddRange(bookAuthors);
            _dbContext.SaveChanges();

            return MapToDTO(bookDomainModel.Id);
        }

        // UPDATE
        public BookWithAuthorAndPublisherDTO? UpdateBookById(int id, AddBookRequestDTO bookDTO)
        {
            var bookDomain = _dbContext.Books.FirstOrDefault(b => b.Id == id);
            if (bookDomain == null) return null;

            // cập nhật Book
            bookDomain.Title = bookDTO.Title;
            bookDomain.Description = bookDTO.Description;
            bookDomain.IsRead = bookDTO.IsRead;
            bookDomain.DateRead = bookDTO.DateRead;
            bookDomain.Rate = bookDTO.Rate;
            bookDomain.Genre = bookDTO.Genre;
            bookDomain.CoverUrl = bookDTO.CoverUrl;
            bookDomain.DateAdded = bookDTO.DateAdded;
            bookDomain.PublisherId = bookDTO.PublisherId;

            // xóa quan hệ cũ
            var oldAuthors = _dbContext.Books_Authors.Where(ba => ba.BookId == id);
            _dbContext.Books_Authors.RemoveRange(oldAuthors);

            // thêm quan hệ mới
            var newAuthors = bookDTO.AuthorIds
                .Distinct()
                .Select(aid => new Book_Author
                {
                    BookId = id,
                    AuthorId = aid
                }).ToList();
            _dbContext.Books_Authors.AddRange(newAuthors);

            _dbContext.SaveChanges();

            return MapToDTO(id);
        }

        // DELETE
        public Book? DeleteBookById(int id)
        {
            var bookDomain = _dbContext.Books
                .Include(b => b.Book_Authors)
                .FirstOrDefault(b => b.Id == id);

            if (bookDomain == null) return null;

            // xóa Book + Book_Authors
            _dbContext.Books_Authors.RemoveRange(bookDomain.Book_Authors);
            _dbContext.Books.Remove(bookDomain);
            _dbContext.SaveChanges();

            return bookDomain;
        }

        // PRIVATE: Map Book + Authors + Publisher sang DTO
        private BookWithAuthorAndPublisherDTO MapToDTO(int bookId)
        {
            return _dbContext.Books
                .Where(b => b.Id == bookId)
                .Select(book => new BookWithAuthorAndPublisherDTO()
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    IsRead = book.IsRead,
                    DateRead = book.DateRead,
                    Rate = book.Rate,
                    Genre = book.Genre,
                    CoverUrl = book.CoverUrl,
                    PublisherName = book.Publisher.Name,
                    AuthorNames = book.Book_Authors
                                      .Select(ba => ba.Author.FullName)
                                      .ToList()
                })
                .First();
        }
    }
}
