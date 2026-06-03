using pp.Models;
using System;

namespace pp.ViewModels
{
    /// <summary>
    /// ViewModel для отдельной задачи
    /// </summary>
    public class TaskViewModel : BaseViewModel
    {
        private Models.Task _task;

        public Models.Task Task
        {
            get => _task;
            set => SetProperty(ref _task, value);
        }

        public TaskViewModel(Models.Task task)
        {
            Task = task ?? new Models.Task();
        }

        /// <summary>
        /// Осталось дней до дедлайна
        /// </summary>
        public int? DaysUntilDeadline
        {
            get
            {
                if (!Task.Deadline.HasValue) return null;
                return (Task.Deadline.Value.Date - DateTime.Today).Days;
            }
        }

        /// <summary>
        /// Просрочена ли задача
        /// </summary>
        public bool IsOverdue => Task.Deadline.HasValue &&
                                  Task.Deadline.Value.Date < DateTime.Today &&
                                  !Task.IsCompleted;

        /// <summary>
        /// Цвет для отображения (красный если просрочено)
        /// </summary>
        public string StatusColor
        {
            get
            {
                if (Task.IsCompleted) return "#90EE90"; // Зеленый
                if (IsOverdue) return "#FF6B6B"; // Красный
                if (DaysUntilDeadline <= 3) return "#FFD93D"; // Желтый
                return "#4D96FF"; // Синий
            }
        }
    }
}