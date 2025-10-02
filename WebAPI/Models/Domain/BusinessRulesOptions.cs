namespace WebAPI.Models.Domain
{
    public class BusinessRulesOptions
    {
        public int MaxBooksPerAuthor { get; set; } = 20;
        public int MaxBooksPerPublisherPerYear { get; set; } = 25;
    }

}
