using System.ComponentModel.DataAnnotations.Schema;

namespace WFViewListBooksJournals.Entities
{
    public class AuthorsBooks
    {
        public int Id { get; set; }

        [ForeignKey("Book")]
        public int Book { get; set; }

        [ForeignKey("Author")]
        public int Author { get; set; }
    }
}
