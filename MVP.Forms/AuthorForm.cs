using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WFViewListBooksJournals.Entities;
using WFViewListBooksJournals.Presenters;
using WFViewListBooksJournals.Views.Interfaces;
using WFViewListBooksJournals.Forms.Common;

namespace WFViewListBooksJournals.Forms
{
    public partial class AuthorForm : Form, IAuthorForm
    {
        private AuthorFormPresenter _presetnerAuthorForm;
        private DisplayOfData _displayData;
        private IPublicationForm _publicationForm;
        private Validation _validation;
        private Dictionary<string, bool> _nationality;

        public AuthorForm(IPublicationForm publicationForm)
        {
            InitializeComponent();

            _presetnerAuthorForm = new AuthorFormPresenter(this);
            _displayData = DisplayOfData.Instance;
            _publicationForm = publicationForm;
            _validation = Validation.Instance;

            _nationality = new Dictionary<string, bool>() { { "ru", false }, { "us", true } };
        }

        public void InitializeComponentAuthorForm()
        {
            var list = _nationality.Select(s => s.Key);
            _displayData.FillComboBox(comboBoxNationality, list);
        }

        private void buttonSave_Click(object sender, System.EventArgs e)
        {
            bool valid =_validation.ValidationDataAuthor(textBoxFirstName.Text, textBoxSecondName.Text, textBoxLastName.Text, textBoxAge.Text, comboBoxNationality.Text);            
            if (!valid)
            {
                return;
            }
            int age = Int32.Parse(textBoxAge.Text);
            string selectedItemString = comboBoxNationality.SelectedItem.ToString();
            bool nationality = _nationality[selectedItemString];            

            Author author = new Author() { FirstName = textBoxFirstName.Text, SecondName = textBoxSecondName.Text, LastName = textBoxLastName.Text, Age = age, InitialsOption = nationality };
            bool exist = _presetnerAuthorForm.ExistAuthor(author);
            if (exist)
            {
                return;
            }
            _presetnerAuthorForm.Save(author);            
            this.Close();
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void BeforeClosed(object sender, FormClosingEventArgs e)
        {
            _publicationForm.FillOnlyComboBoxAuthors();
        }
    }
}
