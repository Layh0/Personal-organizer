using pp.Models;
using System.Windows.Media;

namespace pp.ViewModels
{
    /// <summary>
    /// ViewModel для заметки с цветовой индикацией
    /// </summary>
    public class NoteViewModel : BaseViewModel
    {
        private Note _note;

        public Note Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        public NoteViewModel(Note note)
        {
            Note = note ?? new Note();
        }

        /// <summary>
        /// Цвет фона заметки (из HEX)
        /// </summary>
        public SolidColorBrush BackgroundColor
        {
            get
            {
                if (string.IsNullOrEmpty(Note.ColorTag))
                    return new SolidColorBrush(Colors.White);

                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(Note.ColorTag);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return new SolidColorBrush(Colors.White);
                }
            }
        }

        /// <summary>
        /// Короткое содержание (первые 100 символов)
        /// </summary>
        public string ShortContent
        {
            get
            {
                if (string.IsNullOrEmpty(Note.Content)) return string.Empty;
                return Note.Content.Length > 100
                    ? Note.Content.Substring(0, 100) + "..."
                    : Note.Content;
            }
        }
    }
}