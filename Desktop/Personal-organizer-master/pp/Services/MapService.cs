using System.Diagnostics;

namespace pp.Services
{
    public static class MapService
    {
        public static void ShowAddress(string? address)
        {
            if (string.IsNullOrEmpty(address))
                return;

            string url = $"https://maps.yandex.ru/?text={Uri.EscapeDataString(address)}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}