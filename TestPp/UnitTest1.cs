using pp.Models;
using pp.Services;
using pp.ViewModels;
using System.Text;
using System.Windows.Media;
using Xunit;

namespace pp.Tests
{
    public class ValidationServiceTests
    {
        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user.name@domain.org", true)]
        [InlineData("user+tag@gmail.com", true)]
        [InlineData("test@sub.domain.com", true)]
        public void IsValidEmail_ValidEmails_ReturnsTrue(string email, bool expected)
        {
            var result = ValidationService.IsValidEmail(email);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("invalid")]
        [InlineData("invalid@")]
        [InlineData("@domain.com")]
        [InlineData("test@.com")]
        [InlineData("test @example.com")]
        public void IsValidEmail_InvalidEmails_ReturnsFalse(string email)
        {
            var result = ValidationService.IsValidEmail(email);
            Assert.False(result);
        }

        [Fact]
        public void IsValidEmail_NullEmail_ReturnsFalse()
        {
            var result = ValidationService.IsValidEmail(null!);
            Assert.False(result);
        }
    }

    public class VCardImporterTests
    {
        private readonly VCardImporter _importer = new();

        [Fact]
        public void ImportFromFile_ValidVCard_ReturnsContacts()
        {
            // Arrange
            var vcardContent = @"BEGIN:VCARD
VERSION:3.0
FN:Иван Иванов
TEL:+79001234567
EMAIL:ivan@example.com
END:VCARD";
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, vcardContent, Encoding.UTF8);

            try
            {
                // Act
                var contacts = _importer.ImportFromFile(tempFile);

                // Assert
                Assert.Single(contacts);
                Assert.Equal("Иван", contacts[0].FirstName);
                Assert.Equal("Иванов", contacts[0].LastName);
                Assert.Equal("+79001234567", contacts[0].Phone);
                Assert.Equal("ivan@example.com", contacts[0].Email);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ImportFromFile_MultipleVCards_ReturnsAllContacts()
        {
            var vcardContent = @"BEGIN:VCARD
FN:Иван Иванов
END:VCARD
BEGIN:VCARD
FN:Анна Петрова
END:VCARD";
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, vcardContent, Encoding.UTF8);

            try
            {
                var contacts = _importer.ImportFromFile(tempFile);
                Assert.Equal(2, contacts.Count);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ExportToFile_ValidContacts_CreatesFile()
        {
            // Arrange
            var contacts = new List<Contact>
            {
                new Contact
                {
                    FirstName = "Иван",
                    LastName = "Иванов",
                    Phone = "+79001234567",
                    Email = "ivan@example.com",
                    Birthday = new DateTime(1990, 5, 15)
                }
            };
            var tempFile = Path.GetTempFileName();

            try
            {
                // Act
                _importer.ExportToFile(contacts, tempFile);

                // Assert
                Assert.True(File.Exists(tempFile));
                var content = File.ReadAllText(tempFile, Encoding.UTF8);
                Assert.Contains("BEGIN:VCARD", content);
                Assert.Contains("FN:Иван Иванов", content);
                Assert.Contains("TEL;TYPE=CELL:+79001234567", content);
                Assert.Contains("EMAIL;TYPE=INTERNET:ivan@example.com", content);
                Assert.Contains("BDAY:19900515", content);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public void ExportToFile_EmptyList_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                _importer.ExportToFile(new List<Contact>(), "test.vcf"));
        }

        [Fact]
        public void ExportToFile_NullPath_ThrowsException()
        {
            var contacts = new List<Contact> { new Contact { FirstName = "Test" } };
            Assert.Throws<ArgumentException>(() =>
                _importer.ExportToFile(contacts, null!));
        }
    }

    public class NoteViewModelTests
    {
        [Fact]
        public void BackgroundColor_ValidHex_ReturnsCorrectColor()
        {
            var note = new Note { ColorTag = "#FF0000" };
            var vm = new NoteViewModel(note);
            Assert.Equal(Colors.Red, vm.BackgroundColor.Color);
        }

        [Fact]
        public void BackgroundColor_InvalidHex_ReturnsWhite()
        {
            var note = new Note { ColorTag = "invalid" };
            var vm = new NoteViewModel(note);
            Assert.Equal(Colors.White, vm.BackgroundColor.Color);
        }

        [Fact]
        public void BackgroundColor_NullColor_ReturnsWhite()
        {
            var note = new Note { ColorTag = null };
            var vm = new NoteViewModel(note);
            Assert.Equal(Colors.White, vm.BackgroundColor.Color);
        }

        [Fact]
        public void ShortContent_LongContent_TruncatesTo100Chars()
        {
            var longText = new string('A', 150);
            var note = new Note { Content = longText };
            var vm = new NoteViewModel(note);

            Assert.Equal(103, vm.ShortContent.Length); // 100 + "..."
            Assert.EndsWith("...", vm.ShortContent);
        }

        [Fact]
        public void ShortContent_ShortContent_ReturnsFullContent()
        {
            var note = new Note { Content = "Короткий текст" };
            var vm = new NoteViewModel(note);
            Assert.Equal("Короткий текст", vm.ShortContent);
        }

        [Fact]
        public void ShortContent_NullContent_ReturnsEmpty()
        {
            var note = new Note { Content = null };
            var vm = new NoteViewModel(note);
            Assert.Equal(string.Empty, vm.ShortContent);
        }
    }
}