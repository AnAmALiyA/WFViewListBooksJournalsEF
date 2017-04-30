using System.Collections.Generic;
using WFViewListBooksJournals.Entities;
using WFViewListBooksJournals.Models.Repositories;
using WFViewListBooksJournals.Views.Interfaces;

namespace WFViewListBooksJournals.Presenters
{
    public class AuthorFormPresenter
    {
        private IAuthorForm _authorForm;
        private AuthorRepository _authorRepository;

        public AuthorFormPresenter(IAuthorForm authorForm)
        {
            _authorForm = authorForm;
            _authorRepository = AuthorRepository.Instance;            
        }

        public bool ExistAuthor(Author author)
        {
            return _authorRepository.ExistAuthor(author);
        }

        public void Save(Author author)
        {
            _authorRepository.Add(author);
        }
    }
}
