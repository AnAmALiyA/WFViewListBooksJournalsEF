using System;
using System.Collections.Generic;
using System.Linq;
using WFViewListBooksJournals.Entities;
using WFViewListBooksJournals.Models.Repositories;
using WFViewListBooksJournals.Presenters.Common;
using WFViewListBooksJournals.Views.Interfaces;

namespace WFViewListBooksJournals.Presenters
{
    public class MainFormPresenter
    {
        private IMainForm _mainForm;
        private AuthorRepository _authorRepository;
        private BookRepository _bookRepository;
        private JournalRepository _journalRepository;
        private NewspaperRepository _newspaperRepository;

        private List<string> _publications;
        public Author SelectedAuthor { get; set; }

        public MainFormPresenter(IMainForm mainForm)
        {
            _mainForm = mainForm;
            _authorRepository = AuthorRepository.Instance;
            _bookRepository = BookRepository.Instance;
            _journalRepository = JournalRepository.Instance;
            _newspaperRepository = NewspaperRepository.Instance;

            _publications = new List<string>() { PublicationsType.Book.ToString(), PublicationsType.Journal.ToString(), PublicationsType.Newspaper.ToString() };
        }

        public void InitializeComponentMainForm()
        {
            FillComboBoxAllAuthors();
            FillComboBoxAllPublications();
            FillListBoxMain();
        }

        public void FillComboBoxAllAuthors()
        {
            _mainForm.ClearComboBoxAuthors();

            var authorsList = _authorRepository.GetAll();
            _mainForm.FillComboBoxAuthors(authorsList);
        }

        private void FillComboBoxAllPublications()
        {
            _mainForm.ClearComboBoxPublications();
            _mainForm.FillComboBoxPublications(_publications);
        }

        public void FillListBoxMain()
        {
            var bookList = _bookRepository.GetAll();
            var journalList = _journalRepository.GetAll();
            var newspaperList = _newspaperRepository.GetAll();

            _mainForm.FillListBoxMain(_publications, bookList, journalList, newspaperList);
        }

        public void ShowAllArticles()
        {
            try
            {
                _mainForm.ClearListBoxMain();

                var queryJournal = _journalRepository.GetAll().SelectMany(j => j.Articles.Select(ar => ar)).ToList();
                _mainForm.FillListBoxMain(queryJournal);

                var queryNewspaper = _newspaperRepository.GetAll().SelectMany(n => n.Articles.Select(a => a)).ToList();
                _mainForm.FillListBoxMain(queryNewspaper);
            }
            catch (Exception ex)
            {
                _mainForm.FillListBoxMain(ex.Message);
            }
        }

        public void SaveBook()
        {
            _bookRepository.SaveDB();
        }

        public void SaveJournals()
        {
            _journalRepository.Save();
        }

        public void SaveNewspaper()
        {
            _newspaperRepository.Save();
        }

        public void FillListBoxPublicationAuthor()
        {
            _mainForm.ClearListBoxMain();

            var queryBooks = GetAllBooks(SelectedAuthor);
            if (queryBooks.Count() != (int)SystemVariablesPublications.Empty)
            {
                _mainForm.FillListBoxMainBooks(_publications, queryBooks);
            }

            var queryJournals = GetAllJournals(SelectedAuthor);
            if (queryJournals.Count() != (int)SystemVariablesPublications.Empty)
            {
                _mainForm.FillListBoxMainJournals(_publications, queryJournals);
            }

            var queryNewspapers = GetAllNewspapers(SelectedAuthor);
            if (queryNewspapers.Count() != (int)SystemVariablesPublications.Empty)
            {
                _mainForm.FillListBoxMainNewspapers(_publications, queryNewspapers);
            }
        }

        //нужно вернуть книгу в которой есть искомый автор.
        private IEnumerable<Book> GetAllBooks(Author author)
        {
            var queryBooks = _bookRepository.GetAll()
                .Where(b => b.Authors
                .Where(a => a == author)
                .Count() != 0);

            return queryBooks;
        }

        //нужно вернуть журнал в котором есть искомый автор.
        private IEnumerable<Journal> GetAllJournals(Author author)
        {
            var queryJournals = _journalRepository.GetAll()
                .Where(j => j.Articles.Where(ar => ar.Authors
                .Where(a => a == author)
                .Count() != 0)
                .Count() != 0);
            
            //var queryJournals = _journalRepository.GetAll().Where(j => j == j.Articles.Where(ar => ar == ar.Authors.Where(a => a == author)));
            //var queryJournals = _journalRepository.GetAll().Where(j => j.Articles == j.Articles.Where(ar => ar.Authors == ar.Authors.Where(a => a == author)));

            return queryJournals;
        }

        //нужно вернуть газету в которой есть искомый автор.
        private IEnumerable<Newspaper> GetAllNewspapers(Author author)
        {
            var queryNewspapers = _newspaperRepository.GetAll()
                .Where(n => n.Articles
                .Where(ar => ar.Authors
                .Where(a => a == author)
                .Count() != 0)
                .Count() != 0);

            return queryNewspapers;
        }
    }
}
