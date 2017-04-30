using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WFViewListBooksJournals.Entities;
using WFViewListBooksJournals.Models.Services;

namespace WFViewListBooksJournals.Models.Repositories
{
    public class BookRepository
    {
        private static BookRepository _instance;
        private MockDataProvider _dataBase;
        private readonly ADOContext _context;
        

        public BookRepository()
        {
            _dataBase = MockDataProvider.Instance;

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _context = new ADOContext(connectionString);
        }

        public static BookRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BookRepository();
                }
                return _instance;
            }
        }

        public List<Book> GetAll()
        {
            var bookList = new List<Book>();
            var books = _dataBase.Books;
            bookList.AddRange(books);
            return bookList;
        }

        public void SaveDB()
        {
            _context.DeleteBooksAuthors();
            _context.DeleteAuthors();
            _context.DeleteBooks();
            
            _context.SaveBooks(_dataBase.Books, _dataBase.Authors.Select(x=>x.Value).ToList());
        }
        
        public void Create(Author author, string namePublication, DateTime date, int pages)
        {
            var authorList = new List<Author>();            
            authorList.Add(author);
            
            var newBook = new Book() { Authors = authorList, Name = namePublication, Date = date, Pages = pages };
            _dataBase.Books.Add(newBook);
        }

        public void Update(Book selectedBook, Author selectedAuthor, string namePublication, DateTime date, int pages)
        {
            var bookList = _dataBase.Books;
            var updateBook = bookList.First(x => x == selectedBook);

            updateBook.Name = namePublication;
            updateBook.Date = date;
            updateBook.Pages = pages;

            Author existAuthor = updateBook.Authors.First(a=>a == selectedAuthor);             
            if (existAuthor == null)
            {
                updateBook.Authors.Add(selectedAuthor);
            }
        }

        public void Delete(Book book)
        {
            var bookList = _dataBase.Books;
            bookList.Remove(book);
            _dataBase.Books = bookList;
        }
    }
}
