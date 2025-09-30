using WebAPI.Data;
using WebAPI.Models.Domain;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WebAPI.Repositories
{
    public class SQLPublisherRepository : IPublisherRepository
    {
        private readonly AppDbContext _dbContext;

        public SQLPublisherRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Publisher> GetAllPublishers()
        {
            return _dbContext.Publishers.ToList();
        }

        public Publisher? GetPublisherById(int id)
        {
            return _dbContext.Publishers.FirstOrDefault(p => p.Id == id);
        }

        public Publisher AddPublisher(Publisher publisher)
        {
            _dbContext.Publishers.Add(publisher);
            _dbContext.SaveChanges();
            return publisher;
        }

        public Publisher? UpdatePublisherById(int id, Publisher publisher)
        {
            var existing = _dbContext.Publishers.FirstOrDefault(p => p.Id == id);
            if (existing == null) return null;

            existing.Name = publisher.Name;
            _dbContext.SaveChanges();
            return existing;
        }

        public Publisher? DeletePublisherById(int id)
        {
            var publisher = _dbContext.Publishers.FirstOrDefault(p => p.Id == id);
            if (publisher == null) return null;

            bool hasBooks = _dbContext.Books.Any(b => b.PublisherId == id);
            if (hasBooks)
            {
                // Ném exception để Controller bắt và trả về 409
                throw new InvalidOperationException($"Cannot delete Publisher {id} because there are Books referencing it");
            }

            _dbContext.Publishers.Remove(publisher);
            _dbContext.SaveChanges();
            return publisher;
        }

        public bool ExistsByName(string name, int? excludeId = null)
        {
            var q = _dbContext.Publishers.AsQueryable();
            if (excludeId.HasValue) q = q.Where(p => p.Id != excludeId.Value);
            return q.Any(p => p.Name.ToLower() == name.ToLower());
        }

        public bool ExistsById(int id) => _dbContext.Publishers.Any(p => p.Id == id);
    }
}
