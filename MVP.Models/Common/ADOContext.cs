using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using WFViewListBooksJournals.Entities;
using WFViewListBooksJournals.Models.Infrastructure;

namespace WFViewListBooksJournals.Models.Services
{
    public class ADOContext
    {
        private string _сonnectionString;
        private SqlConnection _connection;
        private Dictionary<string, string> _commandString;
        private Dictionary<string, string> _tableFields;

        public ADOContext(string connectionString)
        {
            _сonnectionString = connectionString;
            _connection = new SqlConnection(_сonnectionString);

            _commandString = new Dictionary<string, string> {
                {"SelectAuthors", "SELECT * FROM Authors" },
                {"SelectBooks", "SELECT * FROM Books" },
                {"SelectAuthorsBooks", "SELECT * FROM AuthorsBooks"},
                {"InsertAuthors", "INSERT Authors VALUES (@FirstName, @SecondName, @LastName, @Age, @InitialsOption, @BookId)" },
                {"InsertBooks", "INSERT Books VALUES (@Name, @Date, @Pages, @AuthorId)" },
                {"SelectIdAuthors", "SELECT Id FROM Authors WHERE FirstName = '{0}' AND SecondName = '{1}' AND LastName = '{2}' AND Age = {3} AND InitialsOption = {4}" },                
                {"SelectIdBooks", "SELECT Id FROM Books WHERE Name = '{0}' AND Date = '{1}' AND Pages = {2}" },
                {"InsertAuthorsBooks", "INSERT AuthorsBooks VALUES (@BookId, @AuthorId)" },
                {"DeleteAuthors", "DELETE Authors" },
                {"DeleteBooks", "DELETE Books" },
                {"DeleteAuthorsBooks", "DELETE AuthorsBooks" },
                {"SelectIdAuthorsNullFild", "SELECT Id FROM Authors WHERE FirstName = '{0}' AND SecondName = '{1}' AND Age = {2} AND InitialsOption = {3}" }
            };

            _tableFields = new Dictionary<string, string> {
                {"Authors", "Authors" },
                {"Books", "Books"},
                {"Id", "Id" },
                {"FirstName", "FirstName"},
                {"SecondName", "SecondName"},
                {"LastName", "LastName"},
                {"Age", "Age"},
                {"InitialsOption", "InitialsOption"},
                {"BookId", "BookId"},
                {"Name", "Name"},
                {"Date", "Date"},
                {"Pages", "Pages"},
                {"AuthorId", "AuthorId"}
            };
        }
        
        public void SaveBooks(List<Book> books, List<Author> authors)
        {
            string sqlCommandString = string.Format(_commandString["SelectBooks"] + ";" + _commandString["SelectAuthors"] + ";" + _commandString["SelectAuthorsBooks"]);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlCommandString, _connection);
            DataSet dataSet = new DataSet();

            FillAdapter(adapter, dataSet);
                        
            DataTable authorsTable = dataSet.Tables[(int)SystemVariablesModel.AuthorsTable];
            DataTable booksTable = dataSet.Tables[(int)SystemVariablesModel.BooksTable];

            FillAuthorsDataTable(authors, authorsTable);
            FillBooksDataTable(books, booksTable);

            UpdateDataSet(adapter, dataSet);
            
            dataSet.Clear();
            FillAdapter(adapter, dataSet);
            
            DataTable authorsBooksTable = dataSet.Tables[(int)SystemVariablesModel.AuthorsBooksTable];

            InsertAuthorsBooks(books, authorsBooksTable, adapter, dataSet);            
        }

        private void FillAdapter(SqlDataAdapter adapter, DataSet dataSet)
        {
            _connection.Open();
            adapter.Fill(dataSet);
            _connection.Close();
        }

        private void UpdateDataSet(SqlDataAdapter adapter, DataSet dataSet)
        {
            _connection.Open();
            SqlCommandBuilder commandBuilder = new SqlCommandBuilder(adapter);
            adapter.Update(dataSet);
            _connection.Close();
        }

        private void FillBooksDataTable(List<Book> books, DataTable booksTable)
        {
            foreach (Book book in books)
            {
                DataRow newRow = booksTable.NewRow();
                newRow[_tableFields["Name"]] = book.Name;
                newRow[_tableFields["Date"]] = book.Date;
                newRow[_tableFields["Pages"]] = book.Pages;

                try
                {
                    booksTable.Rows.Add(newRow);
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
        }
        
        private void InsertAuthorsBooks(List<Book> books, DataTable tableAuthorsBooks, SqlDataAdapter adapter, DataSet dataSet)
        {
            foreach (Book book in books)
            {
                int bookId = FindBookId(book);
                if (bookId == (int) SystemVariablesModel.NoExist)
                {
                    continue;
                }

                List<int> listAuthorId = new List<int>();
                foreach (Author author in book.Authors)
                {
                    int authorId = FindAuthorId(author);
                    listAuthorId.Add(authorId);
                }

                FillBooksDataTable(tableAuthorsBooks, bookId, listAuthorId, adapter, dataSet);
            }
        }

        private void FillBooksDataTable(DataTable tableAuthorsBooks, int bookId, List<int> listAuthorId, SqlDataAdapter adapter, DataSet dataSet)
        {
            DataRow newRow = tableAuthorsBooks.NewRow();
            foreach (int authorId in listAuthorId)
            {
                newRow[_tableFields["BookId"]] = bookId;
                newRow[_tableFields["AuthorId"]] = authorId;
            }

            try
            {
                tableAuthorsBooks.Rows.Add(newRow);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private void FillAuthorsDataTable(List<Author> authors, DataTable tableAuthors)
        {
            foreach (Author author in authors)
            {
                DataRow newRow = tableAuthors.NewRow();
                newRow[_tableFields["FirstName"]] = author.FirstName;
                newRow[_tableFields["SecondName"]] = author.SecondName;
                newRow[_tableFields["LastName"]] = author.LastName;
                newRow[_tableFields["Age"]] = author.Age;
                newRow[_tableFields["InitialsOption"]] = author.InitialsOption;

                try
                {
                    tableAuthors.Rows.Add(newRow);
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
        }
        
        public int FindBookId(Book book)
        {
            string commandString = String.Format(_commandString["SelectIdBooks"], book.Name, book.Date, book.Pages);

            _connection.Open();

            SqlCommand command = new SqlCommand(commandString, _connection);

            var temp = command.ExecuteScalar();
            _connection.Close();

            if (temp != null)
            {
                int index = Int32.Parse(temp.ToString());
                return index;
            }
            return (int)SystemVariablesModel.NoExist;
        }

        public int FindAuthorId(Author author)
        {
            string commandString = string.Empty;
            if (author.LastName != null)
            {
                commandString = String.Format(_commandString["SelectIdAuthors"], author.FirstName, author.SecondName, author.LastName, author.Age, GetIdentifyBit(author.InitialsOption));
            }

            if (author.LastName == null)
            {
                commandString = String.Format(_commandString["SelectIdAuthorsNullFild"], author.FirstName, author.SecondName, author.Age, GetIdentifyBit(author.InitialsOption));
            }

            _connection.Open();

            SqlCommand cmd = new SqlCommand(commandString, _connection);
            
            var temp = cmd.ExecuteScalar();
            _connection.Close();
            
            if (temp != null)
            {
                int index = Int32.Parse(temp.ToString());
                return index;
            }
            return (int)SystemVariablesModel.NoExist;
        }

        private int GetIdentifyBit(bool initialsOption)
        {
            if (initialsOption)
            {
                return (int)SystemVariablesModel.BitTrue;
            }
            return (int)SystemVariablesModel.BitFalse;
        }
        
        public void DeleteAuthors()
        {
            DeleteDataInTable(_commandString["DeleteAuthors"]);
        }

        public void DeleteBooks()
        {
            DeleteDataInTable(_commandString["DeleteBooks"]);
        }

        public void DeleteBooksAuthors()
        {
            DeleteDataInTable(_commandString["DeleteAuthorsBooks"]);
        }

        private void DeleteDataInTable(string commandString)
        {
            _connection.Open();

            SqlCommand command = new SqlCommand(commandString, _connection);

            command.ExecuteNonQuery();
            _connection.Close();
        }
    }
}
