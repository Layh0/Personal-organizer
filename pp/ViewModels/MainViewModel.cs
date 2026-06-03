using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using pp.Data;
using pp.Models;
using pp.Services;

namespace pp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly AppDbContext _context;
        private readonly VCardImporter _vCardImporter;
        private string? _searchText;
        private Contact? _selectedContact;
        private Models.Task? _selectedTask;
        private Note? _selectedNote;

        public ObservableCollection<Contact> Contacts { get; set; }
        public ObservableCollection<Models.Task> Tasks { get; set; }
        public ObservableCollection<Note> Notes { get; set; }
        public ObservableCollection<Category> Categories { get; set; }

        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterContacts();
                }
            }
        }

        public Contact? SelectedContact
        {
            get => _selectedContact;
            set => SetProperty(ref _selectedContact, value);
        }

        public Models.Task? SelectedTask
        {
            get => _selectedTask;
            set => SetProperty(ref _selectedTask, value);
        }

        public Note? SelectedNote
        {
            get => _selectedNote;
            set => SetProperty(ref _selectedNote, value);
        }

        // Команды с private set
        public RelayCommand AddContactCommand { get; private set; }
        public RelayCommand EditContactCommand { get; private set; }
        public RelayCommand DeleteContactCommand { get; private set; }
        public RelayCommand ImportVCardCommand { get; private set; }
        public RelayCommand ExportVCardCommand { get; private set; }

        public RelayCommand AddTaskCommand { get; private set; }
        public RelayCommand EditTaskCommand { get; private set; }
        public RelayCommand DeleteTaskCommand { get; private set; }
        public RelayCommand CompleteTaskCommand { get; private set; }

        public RelayCommand AddNoteCommand { get; private set; }
        public RelayCommand EditNoteCommand { get; private set; }
        public RelayCommand DeleteNoteCommand { get; private set; }

        public MainViewModel()
        {
            _context = new AppDbContext();
            _vCardImporter = new VCardImporter();

            Contacts = new ObservableCollection<Contact>();
            Tasks = new ObservableCollection<Models.Task>();
            Notes = new ObservableCollection<Note>();
            Categories = new ObservableCollection<Category>();

            InitializeCommands();
            LoadDataAsync();
            CheckBirthdayReminders();
        }

        private void InitializeCommands()
        {
            AddContactCommand = new RelayCommand(_ => AddContact());
            EditContactCommand = new RelayCommand(_ => EditContact(), _ => SelectedContact != null);
            DeleteContactCommand = new RelayCommand(_ => DeleteContact(), _ => SelectedContact != null);
            ImportVCardCommand = new RelayCommand(_ => ImportVCard());
            ExportVCardCommand = new RelayCommand(_ => ExportVCard(), _ => Contacts.Any());

            AddTaskCommand = new RelayCommand(_ => AddTask());
            EditTaskCommand = new RelayCommand(_ => EditTask(), _ => SelectedTask != null);
            DeleteTaskCommand = new RelayCommand(_ => DeleteTask(), _ => SelectedTask != null);
            CompleteTaskCommand = new RelayCommand(_ => CompleteTask(), _ => SelectedTask != null);

            AddNoteCommand = new RelayCommand(_ => AddNote());
            EditNoteCommand = new RelayCommand(_ => EditNote(), _ => SelectedNote != null);
            DeleteNoteCommand = new RelayCommand(_ => DeleteNote(), _ => SelectedNote != null);
        }

        private async void LoadDataAsync()
        {
            await LoadCategoriesAsync();
            await LoadContactsAsync();
            await LoadTasksAsync();
            await LoadNotesAsync();
        }

        private async System.Threading.Tasks.Task LoadCategoriesAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            Categories.Clear();
            foreach (var cat in categories)
                Categories.Add(cat);
        }

        private async System.Threading.Tasks.Task LoadContactsAsync()
        {
            var contacts = await _context.Contacts.ToListAsync();
            Contacts.Clear();
            foreach (var contact in contacts)
                Contacts.Add(contact);
        }

        private async System.Threading.Tasks.Task LoadTasksAsync()
        {
            var tasks = await _context.Tasks.Include(t => t.Contact).ToListAsync();
            Tasks.Clear();
            foreach (var task in tasks)
                Tasks.Add(task);
        }

        private async System.Threading.Tasks.Task LoadNotesAsync()
        {
            var notes = await _context.Notes.Include(n => n.Category).ToListAsync();
            Notes.Clear();
            foreach (var note in notes)
                Notes.Add(note);
        }

        private async void FilterContacts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadContactsAsync();
                return;
            }

            var filtered = await _context.Contacts
                .Where(c => c.FirstName != null && c.FirstName.Contains(SearchText) ||
                           c.LastName != null && c.LastName.Contains(SearchText) ||
                           c.Phone != null && c.Phone.Contains(SearchText) ||
                           c.Email != null && c.Email.Contains(SearchText))
                .ToListAsync();

            Contacts.Clear();
            foreach (var contact in filtered)
                Contacts.Add(contact);
        }

        private async void AddContact()
        {
            string? firstName = Microsoft.VisualBasic.Interaction.InputBox("Имя:", "Новый контакт", "");
            if (string.IsNullOrWhiteSpace(firstName)) return;

            string? phone = Microsoft.VisualBasic.Interaction.InputBox("Телефон:", "Новый контакт", "");
            string? email = Microsoft.VisualBasic.Interaction.InputBox("Email:", "Новый контакт", "");

            if (!string.IsNullOrEmpty(email) && !ValidationService.IsValidEmail(email))
            {
                MessageBox.Show("Некорректный email!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var contact = new Contact
            {
                FirstName = firstName,
                Phone = phone,
                Email = email,
                Birthday = DateTime.SpecifyKind(DateTime.Today.AddDays(3), DateTimeKind.Utc)
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            await LoadContactsAsync();
        }

        private async void EditContact()
        {
            if (SelectedContact == null) return;

            string? firstName = Microsoft.VisualBasic.Interaction.InputBox("Имя:", "Редактировать", SelectedContact.FirstName);
            if (string.IsNullOrWhiteSpace(firstName)) return;

            SelectedContact.FirstName = firstName;
            _context.Contacts.Update(SelectedContact);
            await _context.SaveChangesAsync();
            await LoadContactsAsync();
        }

        private async void DeleteContact()
        {
            if (SelectedContact == null) return;

            var result = MessageBox.Show($"Удалить {SelectedContact.FirstName}?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _context.Contacts.Remove(SelectedContact);
                await _context.SaveChangesAsync();
                await LoadContactsAsync();
                await LoadTasksAsync();
            }
        }

        private async void ImportVCard()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "vCard files (*.vcf)|*.vcf",
                Title = "Импорт контактов"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var contacts = _vCardImporter.ImportFromFile(dialog.FileName);
                    foreach (var contact in contacts)
                        _context.Contacts.Add(contact);

                    await _context.SaveChangesAsync();
                    await LoadContactsAsync();
                    MessageBox.Show($"Импортировано: {contacts.Count}", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportVCard()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "vCard files (*.vcf)|*.vcf",
                FileName = "contacts.vcf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _vCardImporter.ExportToFile(Contacts.ToList(), dialog.FileName);
                    MessageBox.Show("Экспорт завершен!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AddTask()
        {
            string? title = Microsoft.VisualBasic.Interaction.InputBox("Название задачи:", "Новая задача", "");
            if (string.IsNullOrWhiteSpace(title)) return;

            var task = new Models.Task { Title = title, IsCompleted = false };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            await LoadTasksAsync();
        }

        private async void EditTask()
        {
            if (SelectedTask == null) return;

            string? title = Microsoft.VisualBasic.Interaction.InputBox("Название:", "Редактировать", SelectedTask.Title);
            if (string.IsNullOrWhiteSpace(title)) return;

            SelectedTask.Title = title;
            _context.Tasks.Update(SelectedTask);
            await _context.SaveChangesAsync();
            await LoadTasksAsync();
        }

        private async void DeleteTask()
        {
            if (SelectedTask == null) return;

            var result = MessageBox.Show("Удалить задачу?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _context.Tasks.Remove(SelectedTask);
                await _context.SaveChangesAsync();
                await LoadTasksAsync();
            }
        }

        private async void CompleteTask()
        {
            if (SelectedTask == null) return;

            SelectedTask.IsCompleted = !SelectedTask.IsCompleted;
            _context.Tasks.Update(SelectedTask);
            await _context.SaveChangesAsync();
            await LoadTasksAsync();
        }

        private async void AddNote()
        {
            string? title = Microsoft.VisualBasic.Interaction.InputBox("Заголовок:", "Новая заметка", "");
            if (string.IsNullOrWhiteSpace(title)) return;

            var note = new Note { Title = title, ColorTag = "#FFD700" };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            await LoadNotesAsync();
        }

        private async void EditNote()
        {
            if (SelectedNote == null) return;

            string? title = Microsoft.VisualBasic.Interaction.InputBox("Заголовок:", "Редактировать", SelectedNote.Title);
            if (string.IsNullOrWhiteSpace(title)) return;

            SelectedNote.Title = title;
            _context.Notes.Update(SelectedNote);
            await _context.SaveChangesAsync();
            await LoadNotesAsync();
        }

        private async void DeleteNote()
        {
            if (SelectedNote == null) return;

            var result = MessageBox.Show("Удалить заметку?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _context.Notes.Remove(SelectedNote);
                await _context.SaveChangesAsync();
                await LoadNotesAsync();
            }
        }

        private void CheckBirthdayReminders()
        {
            var today = DateTime.Today;
            var upcoming = Contacts
                .Where(c => c.Birthday.HasValue)
                .Where(c =>
                {
                    var bday = new DateTime(today.Year, c.Birthday.Value.Month, c.Birthday.Value.Day);
                    var days = (bday - today).TotalDays;
                    return days >= 0 && days <= 3;
                })
                .ToList();

            if (upcoming.Any())
            {
                var names = string.Join(", ", upcoming.Select(c => $"{c.FirstName} {c.LastName}"));
                MessageBox.Show($"Напоминание: День рождения у {names} в ближайшие 3 дня!", "Органайзер",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}