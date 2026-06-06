using System.IO;
using System.Text;
using pp.Models;

namespace pp.Services
{
    public class VCardImporter
    {
        /// <summary>
        /// Импортирует контакты из файла vCard.
        /// </summary>
        public List<Contact> ImportFromFile(string filePath)
        {
            var contacts = new List<Contact>();
            // Чтение с кодировкой UTF-8 для поддержки кириллицы
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            Contact current = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("BEGIN:VCARD")) current = new Contact();
                else if (line.StartsWith("END:VCARD") && current != null)
                {
                    contacts.Add(current);
                    current = null;
                }
                else if (current != null)
                {
                    ParseVCardLine(line, current);
                }
            }
            return contacts;
        }

        // Парсит vCard строку в объект Contact
        private void ParseVCardLine(string line, Contact contact)
        {
            if (line.StartsWith("FN:"))
            {
                var names = line.Substring(3).Split(' ');
                contact.FirstName = names[0];
                if (names.Length > 1) contact.LastName = names[1];
            }
            else if (line.StartsWith("TEL:")) contact.Phone = line.Substring(4);
            else if (line.StartsWith("EMAIL:")) contact.Email = line.Substring(6);
        }

        public void ExportToFile(List<Contact> contacts, string filePath)
        {
            if (contacts == null || contacts.Count == 0)
                throw new ArgumentException("Список контактов пуст", nameof(contacts));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не указан", nameof(filePath));

            var sb = new StringBuilder();

            foreach (var contact in contacts)
            {
                sb.AppendLine("BEGIN:VCARD");
                sb.AppendLine("VERSION:3.0");
                sb.AppendLine("PRODID:-//Personal Organizer//RU");

                // Имя
                var fullName = $"{contact.FirstName} {contact.LastName}".Trim();
                if (!string.IsNullOrEmpty(fullName))
                {
                    sb.AppendLine($"FN:{fullName}");
                    sb.AppendLine($"N:{contact.LastName};{contact.FirstName};;;");
                }

                // Телефон
                if (!string.IsNullOrEmpty(contact.Phone))
                {
                    sb.AppendLine($"TEL;TYPE=CELL:{contact.Phone}");
                }

                // Email
                if (!string.IsNullOrEmpty(contact.Email))
                {
                    sb.AppendLine($"EMAIL;TYPE=INTERNET:{contact.Email}");
                }

                // Адрес
                if (!string.IsNullOrEmpty(contact.Address))
                {
                    sb.AppendLine($"ADR;TYPE=HOME:;;{contact.Address};;;;");
                }

                // День рождения
                if (contact.Birthday.HasValue)
                {
                    sb.AppendLine($"BDAY:{contact.Birthday.Value:yyyyMMdd}");
                }

                sb.AppendLine("END:VCARD");
                sb.AppendLine(); // Пустая строка между контактами
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
