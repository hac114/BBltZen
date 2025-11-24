namespace BBltZen.Utilities
{
    public static class FormattingHelper
    {
        public static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;

            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }

        public static string FormatTimespan(TimeSpan timeSpan)
        {
            return timeSpan.TotalHours >= 1 ? $"{timeSpan.TotalHours:F1}h" :
                   timeSpan.TotalMinutes >= 1 ? $"{timeSpan.TotalMinutes:F1}m" :
                   $"{timeSpan.TotalSeconds:F1}s";
        }
    }
}