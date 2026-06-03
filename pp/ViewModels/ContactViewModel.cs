using pp.Models;
using System;

namespace pp.ViewModels
{
    /// <summary>
    /// ViewModel для отдельного контакта (для диалоговых окон)
    /// </summary>
    public class ContactViewModel : BaseViewModel
    {
        private Contact _contact;

        public Contact Contact
        {
            get => _contact;
            set => SetProperty(ref _contact, value);
        }

        public ContactViewModel(Contact contact)
        {
            Contact = contact ?? new Contact();
        }

        /// <summary>
        /// Полное имя контакта (для отображения)
        /// </summary>
        public string FullName => $"{Contact.FirstName} {Contact.LastName}".Trim();

        /// <summary>
        /// Возраст контакта (если указана дата рождения)
        /// </summary>
        public int? Age
        {
            get
            {
                if (!Contact.Birthday.HasValue) return null;

                var today = DateTime.Today;
                var age = today.Year - Contact.Birthday.Value.Year;
                if (Contact.Birthday.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}