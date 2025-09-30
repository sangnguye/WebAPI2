using WebAPI.Models.Domain;
using System.Collections.Generic;

namespace WebAPI.Repositories
{
    public interface IPublisherRepository
    {
        IEnumerable<Publisher> GetAllPublishers();
        Publisher? GetPublisherById(int id);
        Publisher AddPublisher(Publisher publisher);
        Publisher? UpdatePublisherById(int id, Publisher publisher);
        Publisher? DeletePublisherById(int id);

        bool ExistsByName(string name, int? excludeId = null);
        bool ExistsById(int id);
    }
}
