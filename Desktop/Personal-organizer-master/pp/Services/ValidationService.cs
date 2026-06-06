using System.Text.RegularExpressions;

namespace pp.Services
{
    public static class ValidationService
    {
        /// <summary>
        /// Проверяет email на валидность.
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
    }
}
